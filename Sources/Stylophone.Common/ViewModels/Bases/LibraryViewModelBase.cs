using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using MpcNET.Commands.Database;
using MpcNET.Tags;
using Stylophone.Common.Helpers;
using Stylophone.Common.Interfaces;
using Stylophone.Common.Services;
using Stylophone.Localization.Strings;

namespace Stylophone.Common.ViewModels
{

    public abstract partial class LibraryViewModelBase : ViewModelBase
    {
        private record Album
        {
            public string Name { get; set; }
            public string SortName { get; set; }
        }

        private INavigationService _navigationService;
        private MPDConnectionService _mpdService;
        private AlbumViewModelFactory _albumVmFactory;

        public abstract RangedObservableCollection<AlbumViewModel> FilteredSource { get; }

        public LibraryViewModelBase(INavigationService navigationService, IDispatcherService dispatcherService, MPDConnectionService mpdService, AlbumViewModelFactory albumViewModelFactory):
            base(dispatcherService)
        {
            _navigationService = navigationService;
            _mpdService = mpdService;
            _albumVmFactory = albumViewModelFactory;
        }

        public static new string GetHeader() => Resources.LibraryHeader;

        public List<AlbumViewModel> Source { get; } = new List<AlbumViewModel>();
        public bool IsSourceEmpty => FilteredSource.Count == 0;

        public async Task LoadDataAsync()
        {
            FilteredSource.CollectionChanged -= (s, e) => OnPropertyChanged(nameof(IsSourceEmpty));
            FilteredSource.CollectionChanged += (s, e) => OnPropertyChanged(nameof(IsSourceEmpty));

            Source.Clear();
            var albumList = await _mpdService.SafelySendCommandAsync(new ListCommand(MpdTags.Album));
            var albumSortList = await _mpdService.SafelySendCommandAsync(new ListCommand(MpdTags.AlbumSort));

            if (albumList != null && albumSortList != null)
            {
                // Create a list of tuples
                var response = albumList.Zip(albumSortList, (album, albumSort) => new Album { Name = album, SortName = albumSort });
                GroupAlbumsByName(response);
            }

            if (Source.Count > 0)
                FilteredSource.AddRange(Source);
        }

        public void FilterLibrary(string text)
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

        private void GroupAlbumsByName(IEnumerable<Album> albums)
        {
            var query = from item in albums
                        group item by GetGroupHeader(item.SortName) into g
                        orderby g.Key
                        select new { GroupName = g.Key, Items = g };

            foreach (var g in query)
            {
                //TODO: For whenever grouping + lazy-loading becomes easy...
                //GroupInfosList info = new GroupInfosList();
                //info.Key = g.GroupName + " (" + g.Items.Count() + ")";

                foreach (var item in g.Items.OrderBy(s => s.SortName.ToLower()))
                {
                    Source.Add(_albumVmFactory.GetAlbumViewModel(item.Name));
                }
            }
        }

        private string GetGroupHeader(string title)
        {
            char c = title.ToUpperInvariant().ToCharArray().First();
            return char.IsLetter(c) ? c.ToString() : char.IsDigit(c) ? "#" : "&";
        }

        [RelayCommand]
        private void ItemClick(AlbumViewModel clickedItem)
        {
            if (clickedItem != null)
            {
                _navigationService.SetListDataItemForNextConnectedAnimation(clickedItem);
                _navigationService.Navigate<AlbumDetailViewModel>(clickedItem);
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
