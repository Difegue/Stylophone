using ColorThiefDotNet;
using Stylophone.Helpers;
using Stylophone.Services;
using Microsoft.Toolkit.Uwp.Helpers;
using MpcNET.Commands.Playback;
using MpcNET.Commands.Playlist;
using MpcNET.Types;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Color = Windows.UI.Color;
using System.Linq;
using Windows.UI.Core;
using Stylophone.Views;
using Windows.UI;
using MpcNET.Commands.Queue;

#if UWP
using Imaging = Windows.UI.Xaml.Media.Imaging;
#else
using Imaging = System.Windows.Media.Imaging;
#endif

namespace Stylophone.ViewModels.Items
{
    public class TrackViewModel : Observable
    {
        private readonly CoreDispatcher _currentUiDispatcher;

        public IMpdFile File { get; }

        public string Name => File.HasTitle ? File.Title : File.Path.Split('/').Last();

        public bool IsPlaying => MPDConnectionService.CurrentStatus.SongId == File.Id;

        internal void UpdatePlayingStatus() => DispatcherHelper.ExecuteOnUIThreadAsync(() => OnPropertyChanged(nameof(IsPlaying)));

        public Imaging.BitmapImage AlbumArt
        {
            get => _albumArt;
            private set
            {
                DispatcherHelper.AwaitableRunAsync(_currentUiDispatcher, () => Set(ref _albumArt, value));
            }
        }

        private Imaging.BitmapImage _albumArt;

        public Color DominantColor
        {
            get => _albumColor;
            private set
            {
                DispatcherHelper.AwaitableRunAsync(_currentUiDispatcher, () => Set(ref _albumColor, value));
            }
        }

        private Color _albumColor;

        private bool _isLight;
        public bool IsLight
        {
            get => _isLight;
            private set
            {
                DispatcherHelper.ExecuteOnUIThreadAsync(() => Set(ref _isLight, value));
            }
        }


        private ICommand _playCommand;
        public ICommand PlayTrackCommand => _playCommand ?? (_playCommand = new RelayCommand<IMpdFile>(PlayTrack));

        private async void PlayTrack(IMpdFile file) => await MPDConnectionService.SafelySendCommandAsync(new PlayIdCommand(file.Id));

        private ICommand _removeCommand;
        public ICommand RemoveFromQueueCommand => _removeCommand ?? (_removeCommand = new RelayCommand<IMpdFile>(RemoveTrack));

        private async void RemoveTrack(IMpdFile file) => await MPDConnectionService.SafelySendCommandAsync(new DeleteIdCommand(file.Id));

        private ICommand _addToQueueCommand;
        public ICommand AddToQueueCommand => _addToQueueCommand ?? (_addToQueueCommand = new RelayCommand<IMpdFile>(AddToQueue));

        private async void AddToQueue(IMpdFile file)
        {
            var response = await MPDConnectionService.SafelySendCommandAsync(new AddIdCommand(file.Path));

            if (response != null)
                NotificationService.ShowInAppNotification("AddedToQueueText".GetLocalized());
        }

        private ICommand _addToPlaylistCommand;
        public ICommand AddToPlayListCommand => _addToPlaylistCommand ?? (_addToPlaylistCommand = new RelayCommand<IMpdFile>(AddToPlaylist));

        private async void AddToPlaylist(IMpdFile file)
        {
            // Adding a file to a playlist somehow triggers the server's "playlist" event, which is normally used for the queue...
            // We disable queue events temporarily in order to avoid UI jitter by a refreshed queue.
            MPDConnectionService.DisableQueueEvents = true;

            var playlistName = await DialogService.ShowAddToPlaylistDialog();
            if (playlistName == null) return;

            var req = await MPDConnectionService.SafelySendCommandAsync(new PlaylistAddCommand(playlistName, file.Path));

            if (req != null)
                NotificationService.ShowInAppNotification(string.Format("AddedToPlaylistText".GetLocalized(), playlistName));

            MPDConnectionService.DisableQueueEvents = false;
        }

        private ICommand _viewAlbumCommand;
        public ICommand ViewAlbumCommand => _viewAlbumCommand ?? (_viewAlbumCommand = new RelayCommand<IMpdFile>(GoToMatchingAlbum));

        private void GoToMatchingAlbum(IMpdFile file)
        {
            try
            {
                if (!file.HasAlbum)
                {
                    NotificationService.ShowInAppNotification("NoAlbumErrorText".GetLocalized(), 0);
                    return;
                }

                // Build an AlbumViewModel from the album name and navigate to it
                var album = new AlbumViewModel(file.Album);
                NavigationService.Navigate<LibraryDetailPage>(album);
            }
            catch (Exception e)
            {
                NotificationService.ShowInAppNotification(string.Format("GenericErrorText".GetLocalized(), e), 0);
            }
        }

        public TrackViewModel(IMpdFile file, bool getAlbumArt = false, int albumArtWidth = -1, CoreDispatcher dispatcher = null)
        {
            MPDConnectionService.SongChanged += (s, e) => UpdatePlayingStatus();

            // Use specific UI dispatcher if given
            // (Used for the compact view scenario, which rolls its own dispatcher..)
            _currentUiDispatcher = dispatcher ?? Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher;

            File = file;
            DominantColor = Colors.Black;

            // Fire off an async request to get the album art from MPD.
            if (getAlbumArt)
                Task.Run(async () =>
                {
                    // For the now playing bar, the album art is rendered at 70px wide.
                    // Kinda hackish propagating the width all the way from PlaybackViewModel to here...
                    var art = await AlbumArtService.GetAlbumArtAsync(File, true, albumArtWidth, _currentUiDispatcher);

                    if (art != null)
                    {
                        // This is RAM-intensive as it has to convert the image, so we only do it if needed (aka now playing bar and full playback only)
                        if (albumArtWidth != -1)
                        {
                            DominantColor = art.DominantColor.ToWindowsColor();
                            IsLight = !(art.DominantColor.IsDark);
                        }

                        AlbumArt = art.ArtBitmap;
                    }
                });
        }

    }
}
