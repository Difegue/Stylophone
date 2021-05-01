using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using MpcNET.Commands.Database;
using MpcNET.Commands.Playlist;
using MpcNET.Commands.Queue;
using MpcNET.Commands.Reflection;
using MpcNET.Tags;
using Stylophone.Common.Interfaces;
using Stylophone.Common.Services;
using Stylophone.Localization.Strings;

namespace Stylophone.Common.ViewModels
{
    public class SearchResultsViewModel : ViewModelBase
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

        private string _search;
        public string QueryText
        {
            get { return _search; }
            set { Set(ref _search, value); }
        }

        private bool _isSearching;
        public bool IsSearchInProgress
        {
            get { return _isSearching; }
            set {
                Set(ref _isSearching, value); 
                OnPropertyChanged(nameof(IsSourceEmpty)); 
            }
        }

        private bool _searchTracks;
        public bool SearchTracks
        {
            get { return _searchTracks; }
            set {
                Set(ref _searchTracks, value);

                if (value)
                {
                    SearchAlbums = false;
                    SearchArtists = false;
                }

                if (value || (!SearchArtists && !SearchAlbums && !SearchTracks))
                    UpdateSource();
            }
        }

        private bool _searchAlbums;
        public bool SearchAlbums
        {
            get { return _searchAlbums; }
            set {
                Set(ref _searchAlbums, value);

                if (value)
                {
                    SearchTracks = false;
                    SearchArtists = false;
                }

                if (value || (!SearchArtists && !SearchAlbums && !SearchTracks))
                    UpdateSource();
            }
        }

        private bool _searchArtists;
        public bool SearchArtists
        {
            get { return _searchArtists; }
            set {
                Set(ref _searchArtists, value);

                if (value)
                {
                    SearchTracks = false;
                    SearchAlbums = false;
                }

                if (value || (!SearchArtists && !SearchAlbums && !SearchTracks))
                    UpdateSource();
            }
        }

        public ObservableCollection<TrackViewModel> Source { get; } = new ObservableCollection<TrackViewModel>();

        public bool IsSourceEmpty => !IsSearchInProgress && Source.Count == 0;

        #region Commands

        private bool IsSingleTrackSelected(object list)
        {
            // Cast the received __ComObject
            var selectedTracks = (IList<object>)list;

            return (selectedTracks?.Count == 1);
        }

        private ICommand _addToQueueCommand;
        public ICommand AddToQueueCommand => _addToQueueCommand ?? (_addToQueueCommand = new RelayCommand<IList<object>>(QueueTrack));

        private async void QueueTrack(object list)
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
                    _notificationService.ShowInAppNotification(Resources.AddedToQueueText);
            }
        }

        private ICommand _addToPlaylistCommand;
        public ICommand AddToPlayListCommand => _addToPlaylistCommand ?? (_addToPlaylistCommand = new RelayCommand<IList<object>>(AddToPlaylist));

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
                    _notificationService.ShowInAppNotification(string.Format(Resources.AddedToPlaylistText, playlistName));
            }
        }

        private ICommand _viewAlbumCommand;
        public ICommand ViewAlbumCommand => _viewAlbumCommand ?? (_viewAlbumCommand = new RelayCommand<IList<object>>(ViewAlbum, IsSingleTrackSelected));

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

                await _dispatcherService.ExecuteOnUIThreadAsync(() => IsSearchInProgress = false);
            });
        }

        private async Task DoSearchAsync(ITag tag)
        {
            var response = await _mpdService.SafelySendCommandAsync(new SearchCommand(tag, QueryText));

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
