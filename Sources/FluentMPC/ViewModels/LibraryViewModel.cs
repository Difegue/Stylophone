using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
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

            // Cancel all previous album loading
            cts?.Cancel();
            //cts?.Dispose();

            cts = new CancellationTokenSource();
            var token = cts.Token;

            Task.Run(() =>
            {
                // Do nothing for 250ms to avoid triggering a ton of loads if the user is just scrolling thru
                Thread.Sleep(250);

                if (token.IsCancellationRequested)
                    return;

                // Not cancelled yet, go on
                // Load album data for the visible range of data
                for (var i = visibleRange.FirstIndex; i < visibleRange.LastIndex + 1; i++) // Increment LastIndex by one to properly cover the visible range
                {
                    var album = this[i];

                    if (album.Files.Count == 0 && !album.IsDetailLoading)
                        _ = Task.Run(async () => await album.LoadAlbumDataAsync(token));
                }
            });
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
                    GroupAlbumsByName(response.Response.Content);
            }
        }

        public void GroupAlbumsByName(List<string> albums)
        {
            var query = from item in albums
                        group item by GetGroupHeader(item) into g
                        orderby g.Key
                        select new { GroupName = g.Key, Items = g };

            foreach (var g in query)
            {
                // For whenever grouping + lazy-loading becomes easy...
                //GroupInfosList info = new GroupInfosList();
                //info.Key = g.GroupName + " (" + g.Items.Count() + ")";

                foreach (var item in g.Items.OrderBy(s => s.ToLower()))
                {
                    Source.Add(new AlbumViewModel(item));
                }
            }
        }

        private string GetGroupHeader(string title)
        {
            char c = title.ToUpperInvariant().ToCharArray().First();
            return char.IsLetter(c) ? c.ToString() : char.IsDigit(c) ? "#" : "&";
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
