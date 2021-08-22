using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Foundation;
using Stylophone.Common.Interfaces;
using Stylophone.Common.Services;
using Stylophone.Common.ViewModels;
using UIKit;

namespace Stylophone.iOS.ViewModels
{
    /// <summary>
    /// ObservableCollection of AlbumViewModels that implements UICollectionViewDataSourcePrefetching.
    /// </summary>
    public class PrefetchableAlbumCollection : NSObject, IUICollectionViewDataSourcePrefetching, IDisposable
    {
        private CancellationTokenSource cts;
        private MPDConnectionService _mpdService;
        public RangedObservableCollection<AlbumViewModel> BackingCollection;

        public PrefetchableAlbumCollection(MPDConnectionService mpdService)
        {
            _mpdService = mpdService;
            BackingCollection = new RangedObservableCollection<AlbumViewModel>();
        }

        public void PrefetchItems(UICollectionView collectionView, NSIndexPath[] indexPaths)
        {
            // Cancel all previous album prefetching
            cts?.Cancel();
            //cts?.Dispose();

            cts = new CancellationTokenSource();
            var token = cts.Token;

            var visiblePaths = collectionView.IndexPathsForVisibleItems;

            Task.Run(() =>
            {
                // Concatenate the currently visible indexPaths with the one given by the prefetching API
                var indexes = visiblePaths.Select(path => path.Row).Concat(indexPaths.Select(path => path.Row));

                // Do nothing for 250ms to avoid triggering a ton of loads if the user is just scrolling thru
                // Unless those are the first albums, gotta keep things snappy
                if (!indexes.Contains(0))
                    Thread.Sleep(250);

                if (token.IsCancellationRequested)
                    return;

                // Not cancelled yet, go on
                LoadItems(indexes, token);
                
            }).ConfigureAwait(false);
        }

        internal void LoadItems(IEnumerable<int> indexes, CancellationToken token = default)
        {
            foreach (var i in indexes)
            {
                var album = BackingCollection[i];
                // Set all visible albums to loading for clearer UI (even if it's a lie!)
                if (album.Files.Count == 0)
                    album.IsDetailLoading = true;
            }

            // Load album data for the visible range of data; We use only one connection for loading all albums to avoid overloading the connection pool.
            // Albumart loads still use their own connections.
            Task.Run(async () =>
            {
                using (var c = await _mpdService.GetConnectionAsync(token))
                    foreach (var i in indexes)
                    {
                        var album = BackingCollection[i];

                        if (album.Files.Count == 0 && !token.IsCancellationRequested)
                            await album.LoadAlbumDataAsync(c.InternalResource);
                        else if (token.IsCancellationRequested)
                            album.IsDetailLoading = false;
                    }
            });
        }
    }

    public class LibraryViewModel : LibraryViewModelBase
    {
        public override RangedObservableCollection<AlbumViewModel> FilteredSource => PrefetchableCollection.BackingCollection;
        public PrefetchableAlbumCollection PrefetchableCollection;

        public LibraryViewModel(INavigationService navigationService, IDispatcherService dispatcherService, MPDConnectionService mpdService, AlbumViewModelFactory albumViewModelFactory) :
            base(navigationService, dispatcherService, mpdService, albumViewModelFactory)
        {
            PrefetchableCollection = new PrefetchableAlbumCollection(mpdService);
        }
    }
}
