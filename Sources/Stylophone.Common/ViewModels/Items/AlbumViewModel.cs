using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MpcNET;
using MpcNET.Commands.Database;
using MpcNET.Commands.Playback;
using MpcNET.Commands.Playlist;
using MpcNET.Commands.Queue;
using MpcNET.Commands.Reflection;
using MpcNET.Tags;
using MpcNET.Types;
using SkiaSharp;
using Stylophone.Common.Interfaces;
using Stylophone.Common.Services;
using Stylophone.Localization.Strings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Stylophone.Common.ViewModels
{
    public class AlbumViewModelFactory
    {
        public INotificationService NotificationService;
        public IInteropService Interop;
        public INavigationService NavigationService;
        public IDispatcherService DispatcherService;
        public IDialogService DialogService;
        public AlbumArtService AlbumArtService;
        public MPDConnectionService MPDService;

        public AlbumViewModelFactory(INotificationService notificationService, IInteropService interop, INavigationService navigationService, IDispatcherService dispatcherService, IDialogService dialogService, 
            AlbumArtService albumArtService, MPDConnectionService mpdService)
        {
            Interop = interop;
            NotificationService = notificationService;
            NavigationService = navigationService;
            DispatcherService = dispatcherService;
            DialogService = dialogService;
            AlbumArtService = albumArtService;
            MPDService = mpdService;
        }

        public AlbumViewModel GetAlbumViewModel(string albumName)
        {
            return new AlbumViewModel(this, albumName);
        }
    }

    public partial class AlbumViewModel : ViewModelBase, IDisposable
    {
        private INotificationService _notificationService;
        private IInteropService _interop;
        private IDialogService _dialogService;
        private INavigationService _navigationService;
        private AlbumArtService _albumArtService;
        private MPDConnectionService _mpdService;

        internal AlbumViewModel(AlbumViewModelFactory factory, string albumName) : base(factory.DispatcherService)
        {
            _interop = factory.Interop;
            _notificationService = factory.NotificationService;
            _navigationService = factory.NavigationService;
            _dialogService = factory.DialogService;
            _albumArtService = factory.AlbumArtService;
            _mpdService = factory.MPDService;

            Name = albumName;
            Files = new List<IMpdFile>();
            IsDetailLoading = false;

            DominantColor = _interop.GetAccentColor();
        }

        [ObservableProperty]
        private string _name;

        [ObservableProperty]
        private string _artist;

        [ObservableProperty]
        private List<IMpdFile> _files;

        [ObservableProperty]
        private bool _isDetailLoading;

        [ObservableProperty]
        private bool _albumArtLoaded;

        [ObservableProperty]
        private SKImage _albumArt;

        [ObservableProperty]
        private SKColor _dominantColor;

        /// <summary>
        /// If the dominant color of the album is too light to show white text on top of, this boolean will be true.
        /// </summary>
        [ObservableProperty]
        private bool _isLight;

        internal void SetAlbumArt(AlbumArt art)
        {
            if (art != null)
            {
                AlbumArt = art.ArtBitmap;
                IsLight = !art.DominantColor.IsDark;
                DominantColor = art.DominantColor.Color;
            }

           AlbumArtLoaded = true;
        }

        [RelayCommand]
        private async Task AddToPlaylist()
        {
            var playlistName = await _dialogService.ShowAddToPlaylistDialog();
            if (playlistName == null || Files.Count == 0) return;

            var commandList = new CommandList();

            foreach (var f in Files)
            {
                commandList.Add(new PlaylistAddCommand(playlistName, f.Path));
            }

            if (await _mpdService.SafelySendCommandAsync(commandList) != null)
            {
                _notificationService.ShowInAppNotification(string.Format(Resources.NotificationAddedToPlaylist, playlistName));
            }
        }

        [RelayCommand]
        private async Task AddAlbum()
        {
            var commandList = new CommandList();

            if (Files.Count == 0)
            {
                _notificationService.ShowInAppNotification(Resources.NotificationNoTracksLoaded, "", NotificationType.Warning);
                return;
            }

            foreach (var f in Files)
            {
                commandList.Add(new AddCommand(f.Path));
            }

            if (await _mpdService.SafelySendCommandAsync(commandList) != null)
                _notificationService.ShowInAppNotification(Resources.NotificationAddedToQueue);
        }

        [RelayCommand]
        private async Task PlayAlbum()
        {
            if (Files.Count == 0)
            {
                _notificationService.ShowInAppNotification(Resources.NotificationNoTracksLoaded, "", NotificationType.Warning);
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

            if (await _mpdService.SafelySendCommandAsync(commandList) != null)
            {
                // Auto-navigate to the queue
                _navigationService.Navigate<QueueViewModel>();
                _notificationService.ShowInAppNotification(string.Format(Resources.NotificationNowPlayingTrack, Name));
            }
        }

        /// <summary>
        /// Load Album Data. You can either provide a MpcConnection object (for batch loading)
        /// or leave as empty to automatically pick up a connection from the datasource.
        /// </summary>
        /// <returns></returns>
        public async Task LoadAlbumDataAsync()
        {
            using (var c = await _mpdService.GetConnectionAsync())
            {
                await LoadAlbumDataAsync(c.InternalResource);
            }
        }

        public async Task LoadAlbumDataAsync(MpcConnection c)
        {
            IsDetailLoading = true;
            AlbumArt = await _interop.GetPlaceholderImageAsync();
            try
            {
                var findReq = await c.SendAsync(new FindCommand(MpdTags.Album, Name));
                if (!findReq.IsResponseValid)
                    return;

                // If files were already added, don't re-add them.
                // This can occasionally happen if the server is a bit overloaded when we look at an album, since AlbumDetailViewModel can call this method a second time.
                if (Files.Count == 0)
                    Files.AddRange(findReq.Response.Content);

                Artist = Files.Any(f => f.HasAlbumArtist) ? Files.First(f => f.HasAlbumArtist).AlbumArtist : 
                         Files.Select(f => f.Artist).Distinct().Where(f => f != "").Aggregate((f1, f2) => $"{f1}, {f2}");

                // If we've already generated album art, don't use the queue and directly grab it
                if (await _albumArtService.IsAlbumArtCachedAsync(Files[0]))
                {
                    var art = await _albumArtService.GetAlbumArtAsync(Files[0], true);
                    SetAlbumArt(art);
                }
                else
                {
                    // Queue this VM in the AlbumArtService so its album art is eventually recovered from the server
                    _albumArtService.QueueAlbumArt(this);
                }
            }
            finally
            {
                IsDetailLoading = false;
            }
        }

        public void Dispose()
        {
            AlbumArt?.Dispose();
            AlbumArt = null;
            AlbumArtLoaded = false;
        }
    }
}
