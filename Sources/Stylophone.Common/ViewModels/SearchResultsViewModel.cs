using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MpcNET.Commands.Database;
using MpcNET.Commands.Playlist;
using MpcNET.Commands.Queue;
using MpcNET.Commands.Reflection;
using MpcNET.Tags;
using MpcNET.Types;
using MpcNET.Types.Filters;
using Stylophone.Common.Interfaces;
using Stylophone.Common.Services;
using Stylophone.Localization.Strings;

namespace Stylophone.Common.ViewModels
{
    public partial class SearchResultsViewModel : ViewModelBase
    {

        private INotificationService _notificationService;
        private IDialogService _dialogService;
        private TrackViewModelFactory _trackVmFactory;
        private MPDConnectionService _mpdService;

        public SearchResultsViewModel(IDispatcherService dispatcherService, INotificationService notificationService, IDialogService dialogService, TrackViewModelFactory trackViewModelFactory, MPDConnectionService mpdService):
            base(dispatcherService)
        {
            _dispatcherService = dispatcherService;
            _notificationService = notificationService;
            _dialogService = dialogService;
            _trackVmFactory = trackViewModelFactory;
            _mpdService = mpdService;

            Source.CollectionChanged += (s, e) => OnPropertyChanged(nameof(IsSourceEmpty));
            _searchTracks = true;
        }

        public static new string GetHeader() => string.Format(Resources.SearchResultsFor, "..."); // Fallback when we return to this page via navigation

        public ObservableCollection<TrackViewModel> Source { get; } = new ObservableCollection<TrackViewModel>();
        public bool IsSourceEmpty => !IsSearchInProgress && Source.Count == 0;

        [ObservableProperty]
        private string _queryText;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsSourceEmpty))]
        private bool _isSearchInProgress;

        [ObservableProperty]
        private bool _searchTracks;

        [ObservableProperty]
        private bool _searchAlbums;

        [ObservableProperty]
        private bool _searchArtists;

        partial void OnSearchTracksChanged(bool value)
        {
            if (value)
            {
                SearchAlbums = false;
                SearchArtists = false;
            }

            if (value || (!SearchArtists && !SearchAlbums && !SearchTracks))
                UpdateSource();
        }

        partial void OnSearchAlbumsChanged(bool value)
        {
            if (value)
            {
                SearchTracks = false;
                SearchArtists = false;
            }
            if (value || (!SearchArtists && !SearchAlbums && !SearchTracks))
                UpdateSource();
        }

        partial void OnSearchArtistsChanged(bool value)
        {
            if (value)
            {
                SearchTracks = false;
                SearchAlbums = false;
            }
            if (value || (!SearchArtists && !SearchAlbums && !SearchTracks))
                UpdateSource();
        }

        #region Commands

        private bool IsSingleTrackSelected(object list)
        {
            // Cast the received __ComObject
            var selectedTracks = (IList<object>)list;

            return (selectedTracks?.Count == 1);
        }

        [RelayCommand]
        private async void AddToQueue(object list)
        {
            var selectedTracks = (IList<object>)list;

            if (selectedTracks?.Count > 0)
            {
                var commandList = new CommandList();

                foreach (var f in selectedTracks)
                {
                    var trackVM = f as TrackViewModel;
                    commandList.Add(new AddIdCommand(trackVM.File.Path));
                }

                var r = await _mpdService.SafelySendCommandAsync(commandList);
                if (r != null)
                    _notificationService.ShowInAppNotification(Resources.NotificationAddedToQueue);
            }
        }

        [RelayCommand]
        private async void AddToPlaylist(object list)
        {
            var playlistName = await _dialogService.ShowAddToPlaylistDialog();
            if (playlistName == null) return;

            var selectedTracks = (IList<object>)list;

            if (selectedTracks?.Count > 0)
            {
                var commandList = new CommandList();

                foreach (var f in selectedTracks)
                {
                    var trackVM = f as TrackViewModel;
                    commandList.Add(new PlaylistAddCommand(playlistName, trackVM.File.Path));
                }

                var req = await _mpdService.SafelySendCommandAsync(commandList);

                if (req != null)
                    _notificationService.ShowInAppNotification(string.Format(Resources.NotificationAddedToPlaylist, playlistName));
            }
        }

        [RelayCommand(CanExecute=nameof(IsSingleTrackSelected))]
        private void ViewAlbum(object list)
        {
            // Cast the received __ComObject
            var selectedTracks = (IList<object>)list;

            if (selectedTracks?.Count > 0)
            {
                var trackVM = selectedTracks.First() as TrackViewModel;
                trackVM.ViewAlbumCommand.Execute(trackVM.File);
            }
        }
        #endregion

        public void Initialize(string searchQuery)
        {
            QueryText = searchQuery;
            UpdateSource();
        }

        private void UpdateSource()
        {
            IsSearchInProgress = true;
            Source.Clear();

            _ = Task.Run(async () =>
            {
                if (SearchTracks)  await DoSearchAsync(FindTags.Title);
                if (SearchAlbums)  await DoSearchAsync(FindTags.Album);
                if (SearchArtists) await DoSearchAsync(FindTags.Artist);

                IsSearchInProgress = false;
            });
        }

        private async Task DoSearchAsync(ITag tag)
        {
            List<IFilter> filterList = new()
            {
                new FilterTag(tag, QueryText, FilterOperator.Contains)
            };
            var response = await _mpdService.SafelySendCommandAsync(new SearchCommand(filterList));

            if (response != null)
            {
                await _dispatcherService.ExecuteOnUIThreadAsync(() =>
                {
                    foreach (var f in response)
                    {
                        Source.Add(_trackVmFactory.GetTrackViewModel(f));
                    }
                });
            }
        }
    }
}
