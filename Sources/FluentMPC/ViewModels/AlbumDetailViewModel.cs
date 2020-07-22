using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using FluentMPC.Helpers;
using FluentMPC.Services;
using FluentMPC.ViewModels.Items;
using Microsoft.Toolkit.Uwp.Helpers;
using MpcNET.Commands.Playback;
using MpcNET.Commands.Playlist;
using MpcNET.Types;

namespace FluentMPC.ViewModels
{
    public class AlbumDetailViewModel : Observable
    {
        private AlbumViewModel _item;

        public AlbumViewModel Item
        {
            get { return _item; }
            set { Set(ref _item, value); }
        }

        public ObservableCollection<TrackViewModel> Source { get; } = new ObservableCollection<TrackViewModel>();
        public IList<MpdPlaylist> Playlists => MPDConnectionService.Playlists;

        private ICommand _addToPlaylistCommand;
        public ICommand AddToPlaylistCommand => _addToPlaylistCommand ?? (_addToPlaylistCommand = new RelayCommand(AddToPlaylist));
        private async void AddToPlaylist()
        {
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
        public ICommand AddAlbumCommand => _addToQueueCommand ?? (_addToQueueCommand = new RelayCommand(AddToQueue));
        private async void AddToQueue()
        {
            using (var c = await MPDConnectionService.GetConnectionAsync())
            {
                foreach (var f in Item.Files)
                {
                    var req = await c.InternalResource.SendAsync(new AddCommand(f.Path));
                }
            }
        }

        private ICommand _playCommand;
        public ICommand PlayAlbumCommand => _playCommand ?? (_playCommand = new RelayCommand<string>(PlayAlbum));
        private async void PlayAlbum(string playlistName)
        {
            // Clear queue, add album and play
            using (var c = await MPDConnectionService.GetConnectionAsync())
            {
                var req = await c.InternalResource.SendAsync(new ClearCommand());
                if (!req.IsResponseValid) throw new Exception($"Couldn't clear queue!");

                foreach (var f in Item.Files)
                {
                    req = await c.InternalResource.SendAsync(new AddCommand(f.Path));
                }

                req = await c.InternalResource.SendAsync(new PlayCommand(0));
            }
        }

        public AlbumDetailViewModel()
        {
        }

        public async Task InitializeAsync(AlbumViewModel album)
        {
            Item = album;

            if (album.IsFullyLoaded)
            {
                // Already loaded, create tracks now
                CreateTrackViewModels();
            }
            else if (album.IsDetailLoading)
            {
                // AlbumVM is currently loading, wait for it to conclude to create tracks
                album.PropertyChanged += async (s, e) =>
                {
                    if (album.IsFullyLoaded && Source.Count == 0)
                        await DispatcherHelper.ExecuteOnUIThreadAsync(() => CreateTrackViewModels());
                };
            }
            else // AlbumVM hasn't been loaded at all, load it ourselves
            {
                _ = Task.Run(async () =>
                {
                    await album.LoadAlbumDataAsync();
                    await DispatcherHelper.ExecuteOnUIThreadAsync(() => CreateTrackViewModels());
                });
            }

        }

        private void CreateTrackViewModels()
        {
            foreach (IMpdFile file in Item.Files)
            {
                Source.Add(new TrackViewModel(file));
            }
        }
    }
}
