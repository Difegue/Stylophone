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
using MpcNET.Commands.Database;
using MpcNET.Tags;
using Sundew.Base.Collections;
using Windows.UI.Xaml.Data;

namespace FluentMPC.ViewModels
{
    public class LazyLoadingAlbumCollection : ObservableCollection<AlbumViewModel>, IList<AlbumViewModel>, ICollection<AlbumViewModel>, IReadOnlyList<AlbumViewModel>,
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
                _ = Task.Run(async () =>
                {
                    using (var c = await MPDConnectionService.GetConnectionAsync(token))
                        for (var i = visibleRange.FirstIndex; i < visibleRange.LastIndex + 1; i++) // Increment LastIndex by one to properly cover the visible range
                        {
                            var album = this[i];

                            if (album.Files.Count == 0 && !token.IsCancellationRequested)
                                await album.LoadAlbumDataAsync(c.InternalResource, token);
                            else if (token.IsCancellationRequested)
                                album.IsDetailLoading = false;
                        }

                });
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

        public LazyLoadingAlbumCollection FilteredSource { get; } = new LazyLoadingAlbumCollection();

        public List<AlbumViewModel> Source { get; } = new List<AlbumViewModel>();

        public bool IsSourceEmpty => FilteredSource.Count == 0;

        public LibraryViewModel()
        {
            FilteredSource.CollectionChanged += (s, e) => OnPropertyChanged(nameof(IsSourceEmpty));
        }

        public async Task LoadDataAsync()
        {
            Source.Clear();
            var response = await MPDConnectionService.SafelySendCommandAsync(new ListCommand(MpdTags.Album));

            if (response != null)
                GroupAlbumsByName(response);

            FilteredSource.AddRange(Source);
        }

        internal void FilterLibrary(string text)
        {
            if (text == "" && FilteredSource.Count < Source.Count)
            {
                FilteredSource.Clear();
                FilteredSource.AddRange(Source);
                return;
            }

            var filtered = Source.Where(album => album.Name.Contains(text, StringComparison.InvariantCultureIgnoreCase)).ToList();
            RemoveNonMatching(filtered);
            AddBack(filtered);
        }

        public void GroupAlbumsByName(List<string> albums)
        {
            var query = from item in albums
                        group item by GetGroupHeader(item) into g
                        orderby g.Key
                        select new { GroupName = g.Key, Items = g };

            foreach (var g in query)
            {
                //TODO: For whenever grouping + lazy-loading becomes easy...
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

        /* These functions go through the current list being displayed (contactsFiltered), and remove
        any items not in the filtered collection (any items that don't belong), or add back any items
        from the original allContacts list that are now supposed to be displayed (i.e. when backspace is hit). */

        private void RemoveNonMatching(IEnumerable<AlbumViewModel> filteredData)
        {
            for (int i = FilteredSource.Count - 1; i >= 0; i--)
            {
                var item = FilteredSource[i];
                // If album is not in the filtered argument list, remove it from the ListView's source.
                if (!filteredData.Contains(item))
                    FilteredSource.Remove(item);
            }
        }

        private void AddBack(IEnumerable<AlbumViewModel> filteredData)
        {
            foreach (var item in filteredData)
            {
                // If item in filtered list is not currently in ListView's source collection, add it back in
                if (!FilteredSource.Contains(item))
                    FilteredSource.Add(item);
            }
        }
    }
}
