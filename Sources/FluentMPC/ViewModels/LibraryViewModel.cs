using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

using FluentMPC.Helpers;
using FluentMPC.Services;
using FluentMPC.ViewModels.Items;
using FluentMPC.Views;

using Microsoft.Toolkit.Uwp.UI.Animations;
using Microsoft.Toolkit.Uwp.UI.Converters;
using MpcNET.Commands.Database;
using MpcNET.Tags;
using Windows.UI.Xaml.Data;

namespace FluentMPC.ViewModels
{
    public class LazyLoadingAlbumCollection: ObservableCollection<AlbumViewModel>, IList<AlbumViewModel>, ICollection<AlbumViewModel>, IReadOnlyList<AlbumViewModel>,
        IReadOnlyCollection<AlbumViewModel>, IEnumerable<AlbumViewModel>, INotifyCollectionChanged, INotifyPropertyChanged, IItemsRangeInfo, IDisposable
    {
        private CancellationTokenSource cts;

        public void RangesChanged(ItemIndexRange visibleRange, IReadOnlyList<ItemIndexRange> trackedItems)
        {

            // Cancel all current heavy album loading
            cts?.Cancel();
            //cts?.Dispose();

            cts = new CancellationTokenSource();
            var token = cts.Token;

            // Load album data for the visible range of data
            for (var i = visibleRange.FirstIndex; i < visibleRange.LastIndex + 1; i++) // Increment LastIndex by one to properly cover the visible range
            {
                var album = this[i];

                if (!album.IsDetailLoading && !album.IsFullyLoaded)
                    Task.Run(async () => await album.LoadAlbumDataAsync(token));
                
            }
        }

        public void Dispose()
        {
        }
    }

    public class LibraryViewModel : Observable
    {
        private ICommand _itemClickCommand;

        public ICommand ItemClickCommand => _itemClickCommand ?? (_itemClickCommand = new RelayCommand<AlbumViewModel>(OnItemClick));

        public LazyLoadingAlbumCollection Source { get; } = new LazyLoadingAlbumCollection();

        public LibraryViewModel()
        {
        }

        public async Task LoadDataAsync()
        {
            Source.Clear();

            using (var c = await MPDConnectionService.GetConnectionAsync())
            {
                var response = await c.InternalResource.SendAsync(new ListCommand(MpdTags.Album));

                if (response.IsResponseValid)
                    foreach (var item in response.Response.Content)
                    {
                        Source.Add(new AlbumViewModel(item));
                    }
            }
        }

        private void OnItemClick(AlbumViewModel clickedItem)
        {
            if (clickedItem != null)
            {
                NavigationService.Frame.SetListDataItemForNextConnectedAnimation(clickedItem);
                NavigationService.Navigate<LibraryDetailPage>(clickedItem);
            }
        }
    }
}
