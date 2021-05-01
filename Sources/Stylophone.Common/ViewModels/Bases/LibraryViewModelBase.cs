using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using MpcNET.Commands.Database;
using MpcNET.Tags;
using Stylophone.Common.Helpers;
using Stylophone.Common.Interfaces;
using Stylophone.Common.Services;
using Stylophone.Localization.Strings;

namespace Stylophone.Common.ViewModels
{
    public abstract class LibraryViewModelBase : ViewModelBase
    {
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

        private ICommand _itemClickCommand;
        public ICommand ItemClickCommand => _itemClickCommand ?? (_itemClickCommand = new RelayCommand<AlbumViewModel>(OnItemClick));

        public List<AlbumViewModel> Source { get; } = new List<AlbumViewModel>();
        public bool IsSourceEmpty => FilteredSource.Count == 0;

        public async Task LoadDataAsync()
        {
            FilteredSource.CollectionChanged -= (s, e) => OnPropertyChanged(nameof(IsSourceEmpty));
            FilteredSource.CollectionChanged += (s, e) => OnPropertyChanged(nameof(IsSourceEmpty));

            Source.Clear();
            var response = await _mpdService.SafelySendCommandAsync(new ListCommand(MpdTags.Album));

            if (response != null)
                GroupAlbumsByName(response);

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
                    Source.Add(_albumVmFactory.GetAlbumViewModel(item));
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
