using ColorThiefDotNet;
using FluentMPC.Helpers;
using FluentMPC.Services;
using Microsoft.Toolkit.Uwp.Helpers;
using Microsoft.Toolkit.Uwp.UI.Controls.TextToolbarSymbols;
using MpcNET.Commands.Database;
using MpcNET.Commands.Playback;
using MpcNET.Commands.Playlist;
using MpcNET.Types;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml.Media.Imaging;

namespace FluentMPC.ViewModels.Items
{
    public class TrackViewModel : Observable
    {
        public IMpdFile File { get; }

        public bool IsPlaying => MPDConnectionService.CurrentStatus.SongId == File.Id;

        public BitmapImage AlbumArt
        {
            get => _albumArt;
            private set
            {
                DispatcherHelper.ExecuteOnUIThreadAsync(() => Set(ref _albumArt, value));

                //TODO: get dominant color of albumart
                //var colorThief = new ColorThief();
                //QuantizedColor quantizedColor = colorThief.GetColor(decoder);
            }
        }

        private BitmapImage _albumArt;

        private ICommand _playCommand;
        public ICommand PlayTrackCommand => _playCommand ?? (_playCommand = new RelayCommand<IMpdFile>(PlayTrack));

        private async void PlayTrack(IMpdFile file)
        {
            using (var c = await MPDConnectionService.GetConnectionAsync())
            {
                var response = await c.SendAsync(new PlayIdCommand(file.Id));
            }
        }

        public ICommand PlayTrackNextCommand;

        private ICommand _removeCommand;
        public ICommand RemoveFromQueueCommand => _removeCommand ?? (_removeCommand = new RelayCommand<IMpdFile>(RemoveTrack));

        private async void RemoveTrack(IMpdFile file)
        {
            using (var c = await MPDConnectionService.GetConnectionAsync())
            {
                var response = await c.SendAsync(new DeleteIdCommand(file.Id));
            }
        }

        public ICommand AddToPlayListCommand;

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
                    var art = await MiscHelpers.GetAlbumArtAsync(File, albumArtWidth);
                    AlbumArt = art;
                });
        }

    }
}
