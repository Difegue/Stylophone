using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using MpcNET;
using MpcNET.Commands.Playback;
using MpcNET.Commands.Playlist;
using MpcNET.Commands.Queue;
using MpcNET.Commands.Reflection;
using SkiaSharp;
using Stylophone.Common.Helpers;
using Stylophone.Common.Interfaces;
using Stylophone.Common.Services;
using Stylophone.Localization.Strings;

namespace Stylophone.Common.ViewModels
{
    public partial class PlaylistViewModel : ViewModelBase
    {
        private INotificationService _notificationService;
        private INavigationService _navigationService;
        private IDialogService _dialogService;
        private IInteropService _interop;
        private MPDConnectionService _mpdService;
        private AlbumArtService _albumArtService;
        private TrackViewModelFactory _trackVmFactory;

        public PlaylistViewModel(INotificationService notificationService, INavigationService navigationService, IDispatcherService dispatcherService, IDialogService dialogService, IInteropService interop, MPDConnectionService mpdService, AlbumArtService albumArtService, TrackViewModelFactory trackVmFactory):
            base(dispatcherService)
        {
            _notificationService = notificationService;
            _navigationService = navigationService;
            _dispatcherService = dispatcherService;
            _dialogService = dialogService;
            _interop = interop;
            _mpdService = mpdService;
            _albumArtService = albumArtService;
            _trackVmFactory = trackVmFactory;

            Source.CollectionChanged += (s, e) => OnPropertyChanged(nameof(IsSourceEmpty));
        }

        private NotifyCollectionChangedAction _previousAction;
        private int _oldId;

        public ObservableCollection<TrackViewModel> Source { get; private set; } = new ObservableCollection<TrackViewModel>();

        public bool IsSourceEmpty => Source.Count == 0;

        private string _name;
        public string Name
        {
            get => _name;
            set => Set(ref _name, value);
        }

        private string _artists;
        public string Artists
        {
            get => _artists;
            private set => Set(ref _artists, value);
        }

        private string _info;
        public string PlaylistInfo
        {
            get => _info;
            private set => Set(ref _info, value);
        }

        private bool _artLoaded;
        public bool ArtLoaded
        {
            get => _artLoaded;
            set => Set(ref _artLoaded, value);
        }

        private SKImage _playlistArt;
        public SKImage PlaylistArt
        {
            get => _playlistArt;
            private set => Set(ref _playlistArt, value);
        }

        private SKImage _playlistArt2;
        public SKImage PlaylistArt2
        {
            get => _playlistArt2;
            private set => Set(ref _playlistArt2, value);
        }

        private SKImage _playlistArt3;
        public SKImage PlaylistArt3
        {
            get => _playlistArt3;
            private set => Set(ref _playlistArt3, value);
        }

        private SKColor _albumColor;
        public SKColor DominantColor
        {
            get => _albumColor;
            set => Set(ref _albumColor, value);
        }

        private bool _isLight;
        /// <summary>
        /// If the dominant color of the album is too light to show white text on top of, this boolean will be true.
        /// </summary>
        public bool IsLight
        {
            get => _isLight;
            private set => Set(ref _isLight, value);
        }

        #region Commands
        private bool IsSingleTrackSelected(object list)
        {
            // Cast the received __ComObject
            var selectedTracks = (IList<object>)list;

            return (selectedTracks?.Count == 1);
        }


        [RelayCommand]
        private async void RemovePlaylist()
        {
            var result = await _dialogService.ShowConfirmDialogAsync(Resources.DeletePlaylistContentDialog, "", Resources.OKButtonText, Resources.CancelButtonText);

            if (result)
            {
                var res = await _mpdService.SafelySendCommandAsync(new RmCommand(Name));

                if (res != null)
                {
                    _notificationService.ShowInAppNotification(Resources.NotificationPlaylistRemoved);
                    _navigationService.GoBack();
                }
            }
        }

        [RelayCommand]
        private async void LoadPlaylist()
        {
            var res = await _mpdService.SafelySendCommandAsync(new LoadCommand(Name));

            if (res != null)
                _notificationService.ShowInAppNotification(Resources.NotificationAddedToQueue);
        }
        
        [RelayCommand]
        private async void PlayPlaylist()
        {
            // Clear queue, add playlist and play
            var commandList = new CommandList(new IMpcCommand<object>[] { new ClearCommand() , new LoadCommand(Name), new PlayCommand(0) });

            if (await _mpdService.SafelySendCommandAsync(commandList) != null)
            {
                // Auto-navigate to the queue
                _navigationService.Navigate<QueueViewModel>();
            }
        }

        [RelayCommand]

        private async void AddToQueue(object list)
        {
            // Cast the received __ComObject
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

        [RelayCommand(CanExecute = nameof(IsSingleTrackSelected))]
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


        [RelayCommand]
        private async void RemoveTrackFromPlaylist(object list)
        {
            // Cast the received __ComObject
            var selectedTracks = (IList<object>)list;

            if (selectedTracks?.Count > 0)
            {
                var commandList = new CommandList();

                // We can't batch PlaylistDeleteCommands cleanly, since they're index-based and logically, said indexes will shift as we remove stuff from the playlist.
                // To simulate this behavior, we copy our Source list and incrementally remove the affected tracks from it to get the valid indexes as we move down the commandList.
                IList<TrackViewModel> copy = Source.ToList();

                foreach (var f in selectedTracks)
                {
                    var trackVM = f as TrackViewModel;
                    commandList.Add(new PlaylistDeleteCommand(Name, copy.IndexOf(trackVM)));
                    copy.Remove(trackVM);
                }

                var r = await _mpdService.SafelySendCommandAsync(commandList);
                if (r != null) // Reload playlist
                    await LoadDataAsync(Name);
            }
        }

        #endregion

        public async Task LoadDataAsync(string playlistName)
        {
            var placeholder = await _interop.GetPlaceholderImageAsync();

            ArtLoaded = false;
            PlaylistArt = placeholder;
            PlaylistArt2 = placeholder;
            PlaylistArt3 = placeholder;

            Name = playlistName;
            Source.CollectionChanged -= Source_CollectionChanged;
            Source.Clear();

            var findReq = await _mpdService.SafelySendCommandAsync(new ListPlaylistInfoCommand(playlistName));
            if (findReq == null) return;

            foreach (var item in findReq)
            {
                Source.Add(_trackVmFactory.GetTrackViewModel(item));
            }

            Source.CollectionChanged += Source_CollectionChanged;

            var artistList = findReq.Select(f => f.Artist).Distinct().Where(f => f != null && f != "").ToList();
            Artists = artistList.Count > 0 ? artistList.Aggregate((f1, f2) => $"{f1}, {f2}") : "";

            var totalTime = Source.Count > 0 ? Source.Select(tr => tr.File.Time).Aggregate((t1, t2) => t1 + t2) : 0;
            TimeSpan t = TimeSpan.FromSeconds(totalTime);

            PlaylistInfo = $"{Source.Count} Tracks, Total Time: {Miscellaneous.ToReadableString(t)}";


            if (Source.Count > 0)
            {
                await Task.Run(async () =>
                {
                    // Get album art for three albums to display in the playlist view
                    Random r = new Random();
                    var distinctAlbums = Source.GroupBy(tr => tr.File.Album).Select(tr => tr.First()).OrderBy((item) => r.Next()).ToList();

                    if (distinctAlbums.Count > 1)
                    {
                        var art = await _albumArtService.GetAlbumArtAsync(distinctAlbums[0].File, true);
                        PlaylistArt = art != null ? art.ArtBitmap : PlaylistArt;

                        DominantColor = (art?.DominantColor?.Color).GetValueOrDefault();
                        IsLight = (!art?.DominantColor?.IsDark).GetValueOrDefault();
                    }

                    if (distinctAlbums.Count > 2)
                    {
                        var art = await _albumArtService.GetAlbumArtAsync(distinctAlbums[1].File, false);
                        PlaylistArt2 = art != null ? art.ArtBitmap : PlaylistArt2;
                    }
                    else PlaylistArt2 = PlaylistArt;

                    if (distinctAlbums.Count > 3)
                    {
                        var art = await _albumArtService.GetAlbumArtAsync(distinctAlbums[2].File, false);
                        PlaylistArt3 = art != null ? art.ArtBitmap : PlaylistArt3;
                    }
                    else PlaylistArt3 = PlaylistArt2;

                    ArtLoaded = true;
                });
            }
        }

        private async void Source_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add && _previousAction == NotifyCollectionChangedAction.Remove)
            {
                // User reordered tracks, send matching command
                await _mpdService.SafelySendCommandAsync(new PlaylistMoveCommand(Name, _oldId, e.NewStartingIndex));
                _previousAction = NotifyCollectionChangedAction.Move;
            }
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                _previousAction = e.Action;
                _oldId = e.OldStartingIndex;
            }
        }
    }
}
