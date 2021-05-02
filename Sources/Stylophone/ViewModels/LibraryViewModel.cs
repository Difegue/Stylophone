using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Stylophone.Common.Interfaces;
using Stylophone.Common.Services;
using Stylophone.Common.ViewModels;
using Windows.UI.Xaml.Data;

namespace Stylophone.ViewModels
{
    /// <summary>
    /// ObservableCollection of AlbumViewModels that implements UWP's IItemsRangeInfo.
    /// </summary>
    public class LazyLoadingAlbumCollection : RangedObservableCollection<AlbumViewModel>, IList<AlbumViewModel>, ICollection<AlbumViewModel>, IReadOnlyList<AlbumViewModel>,
        IReadOnlyCollection<AlbumViewModel>, IEnumerable<AlbumViewModel>, INotifyCollectionChanged, INotifyPropertyChanged, IItemsRangeInfo, IDisposable
    {
        private CancellationTokenSource cts;
        private MPDConnectionService _mpdService;

        public LazyLoadingAlbumCollection(MPDConnectionService mpdService)
        {
            _mpdService = mpdService;
        }

        public void RangesChanged(ItemIndexRange visibleRange, IReadOnlyList<ItemIndexRange> trackedItems)
        {

            // Cancel all previous album loading
            cts?.Cancel();
            //cts?.Dispose();

            cts = new CancellationTokenSource();
            var token = cts.Token;

            Task.Run(() =>
            {
                // Do nothing for 250ms to avoid triggering a ton of loads if the user is just scrolling thru
                // Unless those are the first albums, gotta keep things snappy
                if (visibleRange.FirstIndex != 0)
                    Thread.Sleep(250);

                if (token.IsCancellationRequested)
                    return;

                // Not cancelled yet, go on
                for (var i = visibleRange.FirstIndex; i < visibleRange.LastIndex + 1; i++) // Increment LastIndex by one to properly cover the visible range
                {
                    var album = this[i];
                    // Set all visible albums to loading for clearer UI (even if it's a lie!)
                    if (album.Files.Count == 0)
                        album.IsDetailLoading = true;
                }

                // Load album data for the visible range of data; We use only one connection for loading all albums to avoid overloading the connection pool.
                // Albumart loads still use their own connections.
                Task.Run(async () =>
                {
                    using (var c = await _mpdService.GetConnectionAsync(token))
                        for (var i = visibleRange.FirstIndex; i < visibleRange.LastIndex + 1; i++) // Increment LastIndex by one to properly cover the visible range
                        {
                            var album = this[i];

                            if (album.Files.Count == 0 && !token.IsCancellationRequested)
                                await album.LoadAlbumDataAsync(c.InternalResource);
                            else if (token.IsCancellationRequested)
                                album.IsDetailLoading = false;
                        }
                });
            }).ConfigureAwait(false);
        }

        public void Dispose()
        {
        }
    }

    public class LibraryViewModel : LibraryViewModelBase
    {
        private LazyLoadingAlbumCollection _source;
        public override RangedObservableCollection<AlbumViewModel> FilteredSource => _source;

        public LibraryViewModel(INavigationService navigationService, IDispatcherService dispatcherService, MPDConnectionService mpdService, AlbumViewModelFactory albumViewModelFactory) :
            base(navigationService, dispatcherService, mpdService, albumViewModelFactory)
        {
            _source = new LazyLoadingAlbumCollection(mpdService);
        }
    }
}
