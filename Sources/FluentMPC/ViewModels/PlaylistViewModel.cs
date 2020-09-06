using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using FluentMPC.Helpers;
using FluentMPC.Services;
using FluentMPC.ViewModels.Items;
using Microsoft.Toolkit.Uwp.Helpers;
using Microsoft.Toolkit.Uwp.UI.Controls;
using MpcNET.Commands.Playback;
using MpcNET.Commands.Playlist;
using MpcNET.Commands.Queue;
using MpcNET.Types;
using Windows.UI.Xaml.Media.Imaging;

namespace FluentMPC.ViewModels
{
    public class PlaylistViewModel : Observable
    {
        public ObservableCollection<TrackViewModel> Source { get; private set; } = new ObservableCollection<TrackViewModel>();

        private ICommand _deletePlaylistCommand;
        public ICommand RemovePlaylistCommand => _deletePlaylistCommand ?? (_deletePlaylistCommand = new RelayCommand(DeletePlaylist));
        private async void DeletePlaylist()
        {
            // TODO
            throw new NotImplementedException();

            /* using (var c = await MPDConnectionService.GetConnectionAsync())
             {
                 foreach (var f in Item.Files)
                 {
                     var req = await c.InternalResource.SendAsync(new PlaylistAddCommand(playlistName, f.Path));
                 }
             }*/
        }

        private ICommand _addToQueueCommand;
        public ICommand AddPlaylistCommand => _addToQueueCommand ?? (_addToQueueCommand = new RelayCommand(AddToQueue));
        private async void AddToQueue()
        {
            using (var c = await MPDConnectionService.GetConnectionAsync())
            {
                await c.InternalResource.SendAsync(new LoadCommand(Name));
            }
        }

        private ICommand _playCommand;
        public ICommand PlayPlaylistCommand => _playCommand ?? (_playCommand = new RelayCommand(PlayAlbum));
        private async void PlayAlbum()
        {
            // Clear queue, add playlist and play
            using (var c = await MPDConnectionService.GetConnectionAsync())
            {
                var req = await c.InternalResource.SendAsync(new ClearCommand());
                if (!req.IsResponseValid) throw new Exception($"Couldn't clear queue!");

                await c.InternalResource.SendAsync(new LoadCommand(Name));
                await c.InternalResource.SendAsync(new PlayCommand(0));
            }
        }

        public PlaylistViewModel()
        {
        }

        public string Name
        {
            get => _name;
            set => Set(ref _name, value);
        }
        private string _name;

        public string Artists
        {
            get => _artists;
            private set
            {
                DispatcherHelper.ExecuteOnUIThreadAsync(() => Set(ref _artists, value));
            }
        }
        private string _artists;

        public string PlaylistInfo
        {
            get => _info;
            private set
            {
                DispatcherHelper.ExecuteOnUIThreadAsync(() => Set(ref _info, value));
            }
        }
        private string _info;

        public bool ArtLoaded
        {
            get => _artLoaded;
            set => Set(ref _artLoaded, value);
        }
        private bool _artLoaded;

        public BitmapImage PlaylistArt
        {
            get => _playlistArt;
            private set
            {
                DispatcherHelper.ExecuteOnUIThreadAsync(() => Set(ref _playlistArt, value));
            }
        }

        private BitmapImage _playlistArt;

        public BitmapImage PlaylistArt2
        {
            get => _playlistArt2;
            private set
            {
                DispatcherHelper.ExecuteOnUIThreadAsync(() => Set(ref _playlistArt2, value));
            }
        }

        private BitmapImage _playlistArt2;

        public BitmapImage PlaylistArt3
        {
            get => _playlistArt3;
            private set
            {
                DispatcherHelper.ExecuteOnUIThreadAsync(() => Set(ref _playlistArt3, value));
            }
        }

        private BitmapImage _playlistArt3;

        public async Task LoadDataAsync(string playlistName)
        {
            Name = playlistName;
            Source.Clear();

            using (var c = await MPDConnectionService.GetConnectionAsync())
            {
                var findReq = await c.InternalResource.SendAsync(new ListPlaylistInfoCommand(playlistName));
                if (!findReq.IsResponseValid) return;

                foreach (var item in findReq.Response.Content)
                {
                    Source.Add(new TrackViewModel(item));
                }

                Artists = findReq.Response.Content.
                            Select(f => f.Artist).Distinct().Aggregate((f1, f2) => $"{f1}, {f2}");

                var totalTime = Source.Select(t => t.File.Time).Aggregate((t1, t2) => t1 + t2);
                TimeSpan t = TimeSpan.FromSeconds(totalTime);

                PlaylistInfo = $"{Source.Count} Tracks, Total Time: {MiscHelpers.ToReadableString(t)}";
            }

            await Task.Run(async () =>
            {
                // Get album art for three albums to display in the playlist view
                Random r = new Random();
                var distinctAlbums = Source.GroupBy(t => t.File.Album).Select(t => t.First()).OrderBy((item) => r.Next()).ToList();

                if (distinctAlbums.Count > 1)
                {
                    var albumart = await AlbumArtHelpers.GetAlbumArtAsync(distinctAlbums[0].File);
                    if (albumart != null)
                        PlaylistArt = await AlbumArtHelpers.WriteableBitmapToBitmapImageAsync(albumart, 150);
                }

                if (distinctAlbums.Count > 2)
                {
                    var albumart = await AlbumArtHelpers.GetAlbumArtAsync(distinctAlbums[1].File);
                    if (albumart != null)
                        PlaylistArt2 = await AlbumArtHelpers.WriteableBitmapToBitmapImageAsync(albumart, 150);
                }
                else PlaylistArt2 = PlaylistArt;

                if (distinctAlbums.Count > 3)
                {
                    var albumart = await AlbumArtHelpers.GetAlbumArtAsync(distinctAlbums[2].File);
                    if (albumart != null)
                        PlaylistArt3 = await AlbumArtHelpers.WriteableBitmapToBitmapImageAsync(albumart, 150);
                }
                else PlaylistArt3 = PlaylistArt2;

                await DispatcherHelper.ExecuteOnUIThreadAsync(() => ArtLoaded = true);
            });
        }
    }
}
