using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
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

        public string PlaylistInfo
        {
            get => _info;
            private set
            {
                DispatcherHelper.ExecuteOnUIThreadAsync(() => Set(ref _info, value));
            }
        }
        private string _info;

        public ObservableCollection<TrackViewModel> Source { get; } = new ObservableCollection<TrackViewModel>();

        private ICommand _addToPlaylistCommand;
        public ICommand AddToPlaylistCommand => _addToPlaylistCommand ?? (_addToPlaylistCommand = new RelayCommand(AddToPlaylist));
        private async void AddToPlaylist()
        {
            var playlistName = await DialogService.ShowAddToPlaylistDialog();
            if (playlistName == null) return;

            try
            {
                using (var c = await MPDConnectionService.GetConnectionAsync())
                {
                    foreach (var f in Item.Files)
                    {
                        var req = await c.InternalResource.SendAsync(new PlaylistAddCommand(playlistName, f.Path));

                        if (!req.IsResponseValid)
                        {
                            NotificationService.ShowInAppNotification($"Couldn't add Album: Invalid MPD Response.", 0);
                            break;
                        }
                    }
                    NotificationService.ShowInAppNotification($"Album added to Playlist!");
                }
            }
            catch (Exception e)
            {
                NotificationService.ShowInAppNotification($"Couldn't add album: {e}", 0);
            }
        }

        private ICommand _addToQueueCommand;
        public ICommand AddAlbumCommand => _addToQueueCommand ?? (_addToQueueCommand = new RelayCommand(AddToQueue));
        private async void AddToQueue()
        {
            try
            {
                using (var c = await MPDConnectionService.GetConnectionAsync())
                {
                    foreach (var f in Item.Files)
                    {
                        var req = await c.InternalResource.SendAsync(new AddCommand(f.Path));

                        if (!req.IsResponseValid)
                        {
                            NotificationService.ShowInAppNotification($"Couldn't add Album: Invalid MPD Response.", 0);
                            break;
                        }
                    }
                    NotificationService.ShowInAppNotification($"Album added to Queue!");
                }
            }
            catch (Exception e)
            {
                NotificationService.ShowInAppNotification($"Couldn't add album: {e}", 0);
            }
        }

        private ICommand _playCommand;
        public ICommand PlayAlbumCommand => _playCommand ?? (_playCommand = new RelayCommand(PlayAlbum));
        private async void PlayAlbum()
        {
            // Clear queue, add album and play
            try
            {
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
                NotificationService.ShowInAppNotification($"Now Playing {Item.Name}");
            }
            catch (Exception e)
            {
                NotificationService.ShowInAppNotification($"Couldn't play album: {e}", 0);
            }
        }

        public AlbumDetailViewModel()
        {
        }

        public async Task InitializeAsync(AlbumViewModel album)
        {
            Item = album;

            //TODO recheck in case loading failed or something

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

            var totalTime = Source.Select(t => t.File.Time).Aggregate((t1, t2) => t1 + t2);
            TimeSpan t = TimeSpan.FromSeconds(totalTime);

            PlaylistInfo = $"{Source.Count} Tracks, Total Time: {MiscHelpers.ToReadableString(t)}";
        }
    }
}
