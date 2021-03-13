using FluentMPC.Helpers;
using FluentMPC.Services;
using Microsoft.Toolkit.Uwp.Helpers;
using MpcNET;
using MpcNET.Commands.Database;
using MpcNET.Commands.Playback;
using MpcNET.Commands.Playlist;
using MpcNET.Commands.Queue;
using MpcNET.Commands.Reflection;
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
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;

namespace FluentMPC.ViewModels.Items
{
    public class AlbumViewModel : Observable
    {
        public string Name
        {
            get => _name;
            set => Set(ref _name, value);
        }
        private string _name;

        public string Artist
        {
            get => _artist;
            private set
            {
                DispatcherService.ExecuteOnUIThreadAsync(() => Set(ref _artist, value));
            }
        }
        private string _artist;

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
                DispatcherService.ExecuteOnUIThreadAsync(() => Set(ref _detailLoading, value));
            }
        }

        private bool _artLoaded;
        public bool AlbumArtLoaded
        {
            get => _artLoaded;
            private set
            {
                DispatcherService.ExecuteOnUIThreadAsync(() => Set(ref _artLoaded, value));
            }
        }

        public BitmapImage AlbumArt
        {
            get => _albumArt;
            private set
            {
                DispatcherService.ExecuteOnUIThreadAsync(() => Set(ref _albumArt, value));
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
                DispatcherService.ExecuteOnUIThreadAsync(() => Set(ref _albumColor, value));
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
                DispatcherService.ExecuteOnUIThreadAsync(() => Set(ref _isLight, value));
            }
        }

        private ICommand _addToPlaylistCommand;
        public ICommand AddToPlaylistCommand => _addToPlaylistCommand ?? (_addToPlaylistCommand = new RelayCommand(AddToPlaylist));
        private async void AddToPlaylist()
        {
            var playlistName = await DialogService.ShowAddToPlaylistDialog();
            if (playlistName == null || Files.Count == 0) return;

            var commandList = new CommandList();

            foreach (var f in Files)
            {
                commandList.Add(new PlaylistAddCommand(playlistName, f.Path));
            }

            if (await MPDConnectionService.SafelySendCommandAsync(commandList) != null)
            {
                NotificationService.ShowInAppNotification(string.Format("AddedToPlaylistText".GetLocalized(), playlistName));
            }
        }

        private ICommand _addToQueueCommand;
        public ICommand AddAlbumCommand => _addToQueueCommand ?? (_addToQueueCommand = new RelayCommand(AddToQueue));
        private async void AddToQueue()
        {
            var commandList = new CommandList();

            if (Files.Count == 0)
            {
                NotificationService.ShowInAppNotification(string.Format("ErrorAddingAlbum".GetLocalized(), "NoTracksLoaded".GetLocalized()), 0);
                return;
            }

            foreach (var f in Files)
            {
                commandList.Add(new AddCommand(f.Path));
            }

            if (await MPDConnectionService.SafelySendCommandAsync(commandList) != null)
                NotificationService.ShowInAppNotification("AddedToQueueText".GetLocalized());
        }

        private ICommand _playCommand;
        public ICommand PlayAlbumCommand => _playCommand ?? (_playCommand = new RelayCommand(PlayAlbum));
        private async void PlayAlbum()
        {
            if (Files.Count == 0)
            {
                NotificationService.ShowInAppNotification(string.Format("ErrorPlayingText".GetLocalized(), "NoTracksLoaded".GetLocalized()), 0);
                return;
            }

            var commandList = new CommandList();

            // Clear queue, add album and play
            commandList.Add(new ClearCommand());

            foreach (var f in Files)
            {
                commandList.Add(new AddCommand(f.Path));
            }

            commandList.Add(new PlayCommand(0));

            if (await MPDConnectionService.SafelySendCommandAsync(commandList) != null)
            {
                // Auto-navigate to the queue
                NavigationService.Navigate(typeof(Views.ServerQueuePage));
                NotificationService.ShowInAppNotification(string.Format("NowPlayingText".GetLocalized(), Name));
            }
        }

        public AlbumViewModel(string albumName)
        {
            Name = albumName;
            DominantColor = (Color)Application.Current.Resources["SystemAccentColor"];
            Files = new List<IMpdFile>();
            IsDetailLoading = false;

            AlbumArt = new BitmapImage(new Uri("ms-appx:///Assets/AlbumPlaceholder.png"));
        }

        public async Task LoadAlbumDataAsync(MpcConnection c)
        {
            IsDetailLoading = true;
            try
            {
                var findReq = await c.SendAsync(new FindCommand(MpdTags.Album, Name));
                if (!findReq.IsResponseValid)
                    return;

                // If files were already added, don't re-add them.
                // This can occasionally happen if the server is a bit overloaded when we look at an album, since AlbumDetailViewModel can call this method a second time.
                if (Files.Count == 0)
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
