using MpcNET.Commands.Playback;
using MpcNET.Commands.Playlist;
using MpcNET.Types;
using System;
using System.Threading.Tasks;
using System.Linq;
using MpcNET.Commands.Queue;
using System.Threading;
using Stylophone.Common.Services;
using SkiaSharp;
using Stylophone.Common.Interfaces;
using Stylophone.Localization.Strings;
using Stylophone.Common.Helpers;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Stylophone.Common.ViewModels
{

    public class TrackViewModelFactory
    {
        public IDispatcherService DispatcherService;
        public INotificationService NotificationService;
        public INavigationService NavigationService;
        public IDialogService DialogService;
        public IInteropService InteropService;
        public AlbumArtService AlbumArtService;
        public AlbumViewModelFactory AlbumViewModelFactory;
        public MPDConnectionService MPDService;

        public TrackViewModelFactory(IDispatcherService dispatcherService, INotificationService notificationService, INavigationService navigationService, IDialogService dialogService, IInteropService interop, AlbumArtService albumArtService, AlbumViewModelFactory albumFactory, MPDConnectionService mpdService)
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

    public partial class TrackViewModel : ViewModelBase
    {
        private INotificationService _notificationService;
        private INavigationService _navigationService;
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
        public bool IsPlaying => _mpdService.CurrentStatus.SongId != -1 && _mpdService.CurrentStatus.SongId == File.Id;
        public void UpdatePlayingStatus() => _dispatcherService.ExecuteOnUIThreadAsync(() => OnPropertyChanged(nameof(IsPlaying)));

        [ObservableProperty]
        private SKImage _albumArt;
        
        [ObservableProperty]
        private SKColor _dominantColor;

        [ObservableProperty]
        private bool _isLight;

        [RelayCommand]
        private async void PlayTrack(IMpdFile file) => await _mpdService.SafelySendCommandAsync(new PlayIdCommand(file.Id));

        [RelayCommand]
        private async void RemoveFromQueue(IMpdFile file) => await _mpdService.SafelySendCommandAsync(new DeleteIdCommand(file.Id));

        [RelayCommand]
        private async void AddToQueue(IMpdFile file)
        {
            var response = await _mpdService.SafelySendCommandAsync(new AddIdCommand(file.Path));

            if (response != null)
                _notificationService.ShowInAppNotification(Resources.NotificationAddedToQueue);
        }

        [RelayCommand]
        private async void AddToPlaylist(IMpdFile file) 
        {
            var playlistName = await _dialogService.ShowAddToPlaylistDialog();
            if (playlistName == null) return;

            var req = await _mpdService.SafelySendCommandAsync(new PlaylistAddCommand(playlistName, file.Path));

            if (req != null)
                _notificationService.ShowInAppNotification(string.Format(Resources.NotificationAddedToPlaylist, playlistName));
        }

        [RelayCommand]
        private void ViewAlbum(IMpdFile file)
        {
            try
            {
                if (!file.HasAlbum)
                {
                    _notificationService.ShowInAppNotification(Resources.ErrorNoMatchingAlbum, false);
                    return;
                }

                // Build an AlbumViewModel from the album name and navigate to it
                var album = _albumVmFactory.GetAlbumViewModel(file.Album);
                _navigationService.Navigate<AlbumDetailViewModel>(album);
            }
            catch (Exception e)
            {
                _notificationService.ShowErrorNotification(e);
            }
        }

        /// <summary>
        /// Fires off an async request to get the album art from MPD.
        /// </summary>
        /// <returns></returns>
        public async Task GetAlbumArtAsync(VisualizationType hostType = VisualizationType.None, CancellationToken albumArtCancellationToken = default)
        {
            AlbumArt = await _interop.GetPlaceholderImageAsync();

            // This is RAM-intensive as it has to convert the image, so we only do it if needed 
            var calculateDominantColor = hostType.IsOneOf(VisualizationType.NowPlayingBar, VisualizationType.FullScreenPlayback);

            // Use the int value of the VisualizationType to know how large the decoded bitmap has to be.
            var art = await _albumArtService.GetAlbumArtAsync(File, calculateDominantColor, albumArtCancellationToken);

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

    }
}
