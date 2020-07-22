using ColorThiefDotNet;
using FluentMPC.Helpers;
using FluentMPC.Services;
using Microsoft.Toolkit.Uwp.Helpers;
using MpcNET.Commands.Playback;
using MpcNET.Commands.Playlist;
using MpcNET.Types;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Color = Windows.UI.Color;
using Windows.UI.Xaml.Media.Imaging;
using System.Linq;
using FluentMPC.Core.Helpers;

namespace FluentMPC.ViewModels.Items
{
    public class TrackViewModel : Observable
    {
        public IMpdFile File { get; }

        public string Name => File.HasTitle ? File.Title : File.Path.Split('/').Last();

        public bool IsPlaying => MPDConnectionService.CurrentStatus.SongId == File.Id;

        internal void UpdatePlayingStatus() => DispatcherHelper.ExecuteOnUIThreadAsync(() => OnPropertyChanged(nameof(IsPlaying)));

        public BitmapImage AlbumArt
        {
            get => _albumArt;
            private set
            {
                DispatcherHelper.ExecuteOnUIThreadAsync(() => Set(ref _albumArt, value));
            }
        }

        private BitmapImage _albumArt;

        public Color DominantColor
        {
            get => _albumColor;
            private set
            {
                DispatcherHelper.ExecuteOnUIThreadAsync(() => Set(ref _albumColor, value));
            }
        }

        private Color _albumColor;

        private ICommand _playCommand;
        public ICommand PlayTrackCommand => _playCommand ?? (_playCommand = new RelayCommand<IMpdFile>(PlayTrack));

        private async void PlayTrack(IMpdFile file)
        {
            using (var c = await MPDConnectionService.GetConnectionAsync())
            {
                var response = await c.InternalResource.SendAsync(new PlayIdCommand(file.Id));
            }
        }

        private ICommand _removeCommand;
        public ICommand RemoveFromQueueCommand => _removeCommand ?? (_removeCommand = new RelayCommand<IMpdFile>(RemoveTrack));

        private async void RemoveTrack(IMpdFile file)
        {
            using (var c = await MPDConnectionService.GetConnectionAsync())
            {
                var response = await c.InternalResource.SendAsync(new DeleteIdCommand(file.Id));
            }
        }

        private ICommand _addToQueueCommand;
        public ICommand AddToQueueCommand => _addToQueueCommand ?? (_addToQueueCommand = new RelayCommand<IMpdFile>(AddToQueue));

        private async void AddToQueue(IMpdFile file)
        {
            using (var c = await MPDConnectionService.GetConnectionAsync())
            {
                var response = await c.InternalResource.SendAsync(new AddIdCommand(file.Path));
            }
        }

        public ICommand AddToPlayListCommand;
        // TODO add to playlist command

        public ICommand PlayTrackNextCommand;
        // TODO move track in queue

        public TrackViewModel(IMpdFile file, bool getAlbumArt = false, int albumArtWidth = -1)
        {
            File = file;
            AlbumArt = new BitmapImage(new Uri("ms-appx:///Assets/AlbumPlaceholder.png"));

            // Fire off an async request to get the album art from MPD.
            if (getAlbumArt)
                Task.Run(async () =>
                {
                    // For the now playing bar, the album art is rendered at 70px wide.
                    // Kinda hackish propagating the width all the way from PlaybackViewModel to here...

                    var art = await MiscHelpers.GetAlbumArtAsync(File);
                    DominantColor = await MiscHelpers.GetDominantColor(art);
                    AlbumArt = await MiscHelpers.WriteableBitmapToBitmapImageAsync(art, albumArtWidth);

                    Singleton<LiveTileService>.Instance.UpdatePlayingSong(this);
                });
        }

    }
}
