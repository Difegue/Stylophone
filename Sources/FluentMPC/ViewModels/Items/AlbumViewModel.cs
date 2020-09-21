using FluentMPC.Helpers;
using FluentMPC.Services;
using Microsoft.Toolkit.Uwp.Helpers;
using MpcNET;
using MpcNET.Commands.Database;
using MpcNET.Commands.Playback;
using MpcNET.Commands.Playlist;
using MpcNET.Commands.Queue;
using MpcNET.Tags;
using MpcNET.Types;
using Sundew.Base.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.System.Threading;
using Windows.UI;
using Windows.UI.Xaml.Media.Imaging;

namespace FluentMPC.ViewModels.Items
{
    public class AlbumViewModel : Observable
    {
        public String Name
        {
            get => _name;
            set => Set(ref _name, value);
        }
        private String _name;

        public String Artist
        {
            get => _artist;
            private set
            {
                DispatcherHelper.ExecuteOnUIThreadAsync(() => Set(ref _artist, value));
            }
        }
        private String _artist;

        public IList<IMpdFile> Files
        {
            get => _files;
            set => Set(ref _files, value);
        }
        private IList<IMpdFile> _files;

        private bool _detailLoading;
        public bool IsDetailLoading
        {
            get => _detailLoading;
            internal set
            {
                DispatcherHelper.ExecuteOnUIThreadAsync(() => Set(ref _detailLoading, value));
            }
        }

        private bool _artLoaded;
        public bool AlbumArtLoaded
        {
            get => _artLoaded;
            private set
            {
                DispatcherHelper.ExecuteOnUIThreadAsync(() => Set(ref _artLoaded, value));
            }
        }

        public BitmapImage AlbumArt
        {
            get => _albumArt;
            private set
            {
                DispatcherHelper.ExecuteOnUIThreadAsync(() => Set(ref _albumArt, value));
            }
        }

        internal void SetAlbumArt(AlbumArt art)
        {
            if (art != null)
            {
                AlbumArt = art.ArtBitmap;
                IsLight = !art.DominantColor.IsDark;
                DominantColor = art.DominantColor.ToWindowsColor();
            }

           AlbumArtLoaded = true;
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

        private bool _isLight;
        /// <summary>
        /// If the dominant color of the album is too light to show white text on top of, this boolean will be true.
        /// </summary>
        public bool IsLight
        {
            get => _isLight;
            private set
            {
                DispatcherHelper.ExecuteOnUIThreadAsync(() => Set(ref _isLight, value));
            }
        }

        private ICommand _addToPlaylistCommand;
        public ICommand AddToPlaylistCommand => _addToPlaylistCommand ?? (_addToPlaylistCommand = new RelayCommand(AddToPlaylist));
        private async void AddToPlaylist()
        {
            var playlistName = await DialogService.ShowAddToPlaylistDialog();
            if (playlistName == null) return;

            try
            {
                if (Files.Count == 0) throw new Exception("No tracks loaded yet.");

                using (var c = await MPDConnectionService.GetConnectionAsync())
                {
                    foreach (var f in Files)
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
                NotificationService.ShowInAppNotification($"Couldn't add album: {e.Message}", 0);
            }
        }

        private ICommand _addToQueueCommand;
        public ICommand AddAlbumCommand => _addToQueueCommand ?? (_addToQueueCommand = new RelayCommand(AddToQueue));
        private async void AddToQueue()
        {
            // Temporarily disable ConnectionService Queue updates, as rapid-firing "add" commands will force the Queue to refresh a ton of times.
            // TODO: check command lists to avoid this issue
            MPDConnectionService.DisableQueueEvents = true;

            try
            {
                if (Files.Count == 0) throw new Exception("No tracks loaded yet.");

                using (var c = await MPDConnectionService.GetConnectionAsync())
                {
                    foreach (var f in Files)
                    {
                        var req = await c.InternalResource.SendAsync(new AddCommand(f.Path));

                        if (!req.IsResponseValid)
                        {
                            NotificationService.ShowInAppNotification($"Couldn't add Album: Invalid MPD Response.", 0);
                            break;
                        }

                        // Reenable queue events for the last file so that the update is processed cleanly by the client
                        if (Files.IndexOf(f) == Files.Count - 2)
                            MPDConnectionService.DisableQueueEvents = false;

                    }
                    NotificationService.ShowInAppNotification($"Album added to Queue!");
                }
            }
            catch (Exception e)
            {
                NotificationService.ShowInAppNotification($"Couldn't add album: {e.Message}", 0);
                MPDConnectionService.DisableQueueEvents = false;
            }
        }

        private ICommand _playCommand;
        public ICommand PlayAlbumCommand => _playCommand ?? (_playCommand = new RelayCommand(PlayAlbum));
        private async void PlayAlbum()
        {
            // Temporarily disable ConnectionService Queue updates, as rapid-firing "add" commands will force the Queue to refresh a ton of times.
            // TODO: check command lists to avoid this issue
            MPDConnectionService.DisableQueueEvents = true;

            // Clear queue, add album and play
            try
            {
                if (Files.Count == 0) throw new Exception("No tracks loaded yet.");

                using (var c = await MPDConnectionService.GetConnectionAsync())
                {
                    var req = await c.InternalResource.SendAsync(new ClearCommand());
                    if (!req.IsResponseValid) throw new Exception($"Couldn't clear queue!");

                    foreach (var f in Files)
                    {
                        req = await c.InternalResource.SendAsync(new AddCommand(f.Path));
                    }
                    req = await c.InternalResource.SendAsync(new PlayCommand(0));
                }
                NotificationService.ShowInAppNotification($"Now Playing {Name}");
            }
            catch (Exception e)
            {
                NotificationService.ShowInAppNotification($"Couldn't play album: {e.Message}", 0);
            }
            MPDConnectionService.DisableQueueEvents = false;
        }

        public AlbumViewModel(string albumName)
        {
            Name = albumName;
            DominantColor = Colors.Black;
            Files = new List<IMpdFile>();
            IsDetailLoading = false;
        }

        public async Task LoadAlbumDataAsync(MpcConnection c)
        {
            IsDetailLoading = true;
            try
            {
                var findReq = await c.SendAsync(new FindCommand(MpdTags.Album, Name));
                if (!findReq.IsResponseValid)
                    return;

                Files.AddRange(findReq.Response.Content);
                Artist = Files.Select(f => f.Artist).Distinct().Aggregate((f1, f2) => $"{f1}, {f2}");

                // If we've already generated album art, don't use the queue and directly grab it
                if (await AlbumArtService.IsAlbumArtCachedAsync(Files[0]))
                {
                    var art = await AlbumArtService.GetAlbumArtAsync(Files[0], true, 180);
                    SetAlbumArt(art);
                }
                else
                {
                    // Queue this VM in the AlbumArtService so its album art is eventually recovered from the server
                    AlbumArtService.QueueAlbumArt(this);
                }
            }
            finally
            {
                IsDetailLoading = false;
            }
        }
    }
}
