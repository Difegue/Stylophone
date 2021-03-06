﻿using ColorThiefDotNet;
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
using Windows.UI.Core;
using FluentMPC.Views;
using Windows.UI;
using MpcNET.Commands.Queue;
using System.Threading;
using FluentMPC.ViewModels.Playback;
using Windows.System;
using Microsoft.Toolkit.Uwp;

namespace FluentMPC.ViewModels.Items
{
    public class TrackViewModel : Observable
    {
        private readonly DispatcherQueue _currentDispatcherQueue;

        public IMpdFile File { get; }

        public string Name => File.HasTitle ? File.Title : File.Path.Split('/').Last();

        public bool IsPlaying => MPDConnectionService.CurrentStatus.SongId == File.Id;

        internal void UpdatePlayingStatus() => DispatcherService.ExecuteOnUIThreadAsync(() => OnPropertyChanged(nameof(IsPlaying)));

        public BitmapImage AlbumArt
        {
            get => _albumArt;
            private set
            {
                _currentDispatcherQueue.EnqueueAsync(() => Set(ref _albumArt, value));
            }
        }

        private BitmapImage _albumArt;

        public Color DominantColor
        {
            get => _albumColor;
            private set
            {
                _currentDispatcherQueue.EnqueueAsync(() => Set(ref _albumColor, value));
            }
        }

        private Color _albumColor;

        private bool _isLight;
        public bool IsLight
        {
            get => _isLight;
            private set
            {
                _currentDispatcherQueue.EnqueueAsync(() => Set(ref _isLight, value));
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
            var playlistName = await DialogService.ShowAddToPlaylistDialog();
            if (playlistName == null) return;

            var req = await MPDConnectionService.SafelySendCommandAsync(new PlaylistAddCommand(playlistName, file.Path));

            if (req != null)
                NotificationService.ShowInAppNotification(string.Format("AddedToPlaylistText".GetLocalized(), playlistName));
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

        public TrackViewModel(IMpdFile file, bool getAlbumArt = false, VisualizationType hostType = VisualizationType.None, DispatcherQueue dispatcherQueue = null, CancellationToken albumArtCancellationToken = default)
        {
            MPDConnectionService.SongChanged += (s, e) => UpdatePlayingStatus();

            // Use specific UI dispatcher if given
            // (Used for the compact view scenario, which rolls its own dispatcher..)
            _currentDispatcherQueue = dispatcherQueue ?? DispatcherService.DispatcherQueue;

            File = file;
            DominantColor = Colors.Black;

            // Fire off an async request to get the album art from MPD.
            if (getAlbumArt)
            {

                Task.Run(async () =>
                {
                    await _currentDispatcherQueue.EnqueueAsync(() =>
                    {
                        var placeholder = new BitmapImage(new Uri("ms-appx:///Assets/AlbumPlaceholder.png"));
                        placeholder.DecodePixelWidth = (int)hostType;
                        AlbumArt = placeholder;
                    });

                    // This is RAM-intensive as it has to convert the image, so we only do it if needed (aka now playing bar and full playback only)
                    var calculateDominantColor = hostType.IsOneOf(VisualizationType.NowPlayingBar, VisualizationType.FullScreenPlayback);

                    // Use the int value of the VisualizationType to know how large the decoded bitmap has to be.
                    var art = await AlbumArtService.GetAlbumArtAsync(File, calculateDominantColor, (int)hostType, _currentDispatcherQueue, albumArtCancellationToken);

                    if (art != null)
                    {
                        if (calculateDominantColor)
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
}
