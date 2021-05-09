using MpcNET.Commands.Playback;
using MpcNET.Commands.Playlist;
using MpcNET.Types;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Linq;
using MpcNET.Commands.Queue;
using System.Threading;
using Stylophone.Common.Services;
using SkiaSharp;
using Stylophone.Common.Interfaces;
using Stylophone.Localization.Strings;
using Stylophone.Common.Helpers;
using MvvmCross.Commands;
using MvvmCross.Navigation;

namespace Stylophone.Common.ViewModels
{

    public class TrackViewModelFactory
    {
        public IDispatcherService DispatcherService;
        public INotificationService NotificationService;
        public IMvxNavigationService NavigationService;
        public IDialogService DialogService;
        public IInteropService InteropService;
        public AlbumArtService AlbumArtService;
        public AlbumViewModelFactory AlbumViewModelFactory;
        public MPDConnectionService MPDService;

        public TrackViewModelFactory(IDispatcherService dispatcherService, INotificationService notificationService, IMvxNavigationService navigationService, IDialogService dialogService, IInteropService interop, AlbumArtService albumArtService, AlbumViewModelFactory albumFactory, MPDConnectionService mpdService)
        {
            DispatcherService = dispatcherService;
            NotificationService = notificationService;
            NavigationService = navigationService;
            DialogService = dialogService;
            AlbumViewModelFactory = albumFactory;
            AlbumArtService = albumArtService;
            InteropService = interop;
            MPDService = mpdService;
        }

        public TrackViewModel GetTrackViewModel(IMpdFile file)
        {
            if (file == null) return null;

            return new TrackViewModel(this, file);
        }
    }

    public class TrackViewModel : ViewModelBase
    {
        private INotificationService _notificationService;
        private IMvxNavigationService _navigationService;
        private IDialogService _dialogService;
        private IInteropService _interop;
        private AlbumArtService _albumArtService;
        private AlbumViewModelFactory _albumVmFactory;
        private MPDConnectionService _mpdService;

        internal TrackViewModel(TrackViewModelFactory factory, IMpdFile file): base(factory.DispatcherService)
        {
            _notificationService = factory.NotificationService;
            _navigationService = factory.NavigationService;
            _dialogService = factory.DialogService;
            _interop = factory.InteropService;
            _albumArtService = factory.AlbumArtService;
            _albumVmFactory = factory.AlbumViewModelFactory;
            _mpdService = factory.MPDService;

            _mpdService.SongChanged += (s, e) => UpdatePlayingStatus();

            File = file;
            DominantColor = new SKColor();
        }

        public IMpdFile File { get; }

        public string Name => File.HasTitle ? File.Title : File.Path.Split('/').Last();

        public bool IsPlaying => _mpdService.CurrentStatus.SongId == File.Id;

        public void UpdatePlayingStatus() => _dispatcherService.ExecuteOnUIThreadAsync(() => RaisePropertyChanged(nameof(IsPlaying)));

        private SKImage _albumArt;
        public SKImage AlbumArt
        {
            get => _albumArt;
            private set => Set(ref _albumArt, value);
        }

        private SKColor _albumColor;
        public SKColor DominantColor
        {
            get => _albumColor;
            private set => Set(ref _albumColor, value);
        }

        private bool _isLight;
        public bool IsLight
        {
            get => _isLight;
            private set => Set(ref _isLight, value);
        }


        private ICommand _playCommand;
        public ICommand PlayTrackCommand => _playCommand ?? (_playCommand = new MvxCommand<IMpdFile>(PlayTrack));

        private async void PlayTrack(IMpdFile file) => await _mpdService.SafelySendCommandAsync(new PlayIdCommand(file.Id));

        private ICommand _removeCommand;
        public ICommand RemoveFromQueueCommand => _removeCommand ?? (_removeCommand = new MvxCommand<IMpdFile>(RemoveTrack));

        private async void RemoveTrack(IMpdFile file) => await _mpdService.SafelySendCommandAsync(new DeleteIdCommand(file.Id));

        private ICommand _addToQueueCommand;
        public ICommand AddToQueueCommand => _addToQueueCommand ?? (_addToQueueCommand = new MvxCommand<IMpdFile>(AddToQueue));

        private async void AddToQueue(IMpdFile file)
        {
            var response = await _mpdService.SafelySendCommandAsync(new AddIdCommand(file.Path));

            if (response != null)
                _notificationService.ShowInAppNotification(Resources.AddedToQueueText);
        }

        private ICommand _addToPlaylistCommand;
        public ICommand AddToPlayListCommand => _addToPlaylistCommand ?? (_addToPlaylistCommand = new MvxCommand<IMpdFile>(AddToPlaylist));

        private async void AddToPlaylist(IMpdFile file)
        {
            var playlistName = await _dialogService.ShowAddToPlaylistDialog();
            if (playlistName == null) return;

            var req = await _mpdService.SafelySendCommandAsync(new PlaylistAddCommand(playlistName, file.Path));

            if (req != null)
                _notificationService.ShowInAppNotification(string.Format(Resources.AddedToPlaylistText, playlistName));
        }

        private ICommand _viewAlbumCommand;
        public ICommand ViewAlbumCommand => _viewAlbumCommand ?? (_viewAlbumCommand = new MvxCommand<IMpdFile>(GoToMatchingAlbum));

        /// <summary>
        /// Fires off an async request to get the album art from MPD.
        /// </summary>
        /// <returns></returns>
        public async Task GetAlbumArtAsync(VisualizationType hostType = VisualizationType.None, CancellationToken albumArtCancellationToken = default)
        {
            AlbumArt = await _interop.GetPlaceholderImageAsync();

            // This is RAM-intensive as it has to convert the image, so we only do it if needed (aka now playing bar and full playback only)
            var calculateDominantColor = hostType.IsOneOf(VisualizationType.NowPlayingBar, VisualizationType.FullScreenPlayback);

            // Use the int value of the VisualizationType to know how large the decoded bitmap has to be.
            var art = await _albumArtService.GetAlbumArtAsync(File, calculateDominantColor, (int)hostType, albumArtCancellationToken);

            if (art != null)
            {
                if (calculateDominantColor)
                {
                    DominantColor = art.DominantColor.Color;
                    IsLight = !(art.DominantColor.IsDark);
                }

                AlbumArt = art.ArtBitmap;
            }
        }

        private void GoToMatchingAlbum(IMpdFile file)
        {
            try
            {
                if (!file.HasAlbum)
                {
                    _notificationService.ShowInAppNotification(Resources.NoAlbumErrorText, false);
                    return;
                }

                // Build an AlbumViewModel from the album name and navigate to it
                var album = _albumVmFactory.GetAlbumViewModel(file.Album);
                _navigationService.Navigate<AlbumDetailViewModel, AlbumViewModel>(album);
            }
            catch (Exception e)
            {
                _notificationService.ShowInAppNotification(string.Format(Resources.GenericErrorText, e), false);
            }
        }

    }
}
