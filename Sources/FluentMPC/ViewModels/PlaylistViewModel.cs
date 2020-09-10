using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace FluentMPC.ViewModels
{
    public class PlaylistViewModel : Observable
    {
        public ObservableCollection<TrackViewModel> Source { get; private set; } = new ObservableCollection<TrackViewModel>();

        private NotifyCollectionChangedAction _previousAction;
        private int _oldId;

        public bool IsSourceEmpty => Source.Count == 0;

        private ICommand _deletePlaylistCommand;
        public ICommand RemovePlaylistCommand => _deletePlaylistCommand ?? (_deletePlaylistCommand = new RelayCommand(DeletePlaylist));
        private async void DeletePlaylist()
        {
            ContentDialog confirmDialog = new ContentDialog
            {
                Title = "Delete Playlist?",
                PrimaryButtonText = "OK",
                CloseButtonText = "Cancel"
            };

            ContentDialogResult result = await confirmDialog.ShowAsync();
            if (result != ContentDialogResult.Primary)
                return;

            var res = await MPDConnectionService.SafelySendCommandAsync(new RmCommand(Name));

            if (res != null)
            {
                NotificationService.ShowInAppNotification("Playlist removed!");
                NavigationService.GoBack();
            }
        }

        private ICommand _addToQueueCommand;
        public ICommand AddPlaylistCommand => _addToQueueCommand ?? (_addToQueueCommand = new RelayCommand(AddToQueue));
        private async void AddToQueue()
        {
            var res = await MPDConnectionService.SafelySendCommandAsync(new LoadCommand(Name));

            if (res != null)
            {
                NotificationService.ShowInAppNotification("Added to queue!");
            }
        }
        private ICommand _playCommand;
        public ICommand PlayPlaylistCommand => _playCommand ?? (_playCommand = new RelayCommand(PlayAlbum));
        private async void PlayAlbum()
        {
            // Clear queue, add playlist and play
            try
            {
                using (var c = await MPDConnectionService.GetConnectionAsync())
                {
                    var req = await c.InternalResource.SendAsync(new ClearCommand());
                    if (!req.IsResponseValid) throw new Exception($"Couldn't clear queue!");

                    await c.InternalResource.SendAsync(new LoadCommand(Name));
                    await c.InternalResource.SendAsync(new PlayCommand(0));
                }
            }
            catch (Exception e)
            {
                NotificationService.ShowInAppNotification($"Couldn't play content: {e}", 0);
            }
        }

        private ICommand _removeTrackCommand;
        public ICommand RemoveTrackFromPlaylistCommand => _removeTrackCommand ?? (_removeTrackCommand = new RelayCommand<TrackViewModel>(RemoveTrack));
        private async void RemoveTrack(TrackViewModel track)
        {
            var trackPos = Source.IndexOf(track);
            var r = await MPDConnectionService.SafelySendCommandAsync(new PlaylistDeleteCommand(Name, trackPos));

            if (r != null) // Reload playlist
                await LoadDataAsync(Name);
        }

        public PlaylistViewModel()
        {
            Source.CollectionChanged += (s, e) => OnPropertyChanged(nameof(IsSourceEmpty));
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
            Source.CollectionChanged -= Source_CollectionChanged;
            Source.Clear();

            var findReq = await MPDConnectionService.SafelySendCommandAsync(new ListPlaylistInfoCommand(playlistName));
            if (findReq == null) return;

            foreach (var item in findReq)
            {
                Source.Add(new TrackViewModel(item));
            }

            Source.CollectionChanged += Source_CollectionChanged;

            Artists = findReq.Count() > 0 ? findReq.
                        Select(f => f.Artist).Distinct().Aggregate((f1, f2) => $"{f1}, {f2}") : "";

            var totalTime = Source.Count > 0 ? Source.Select(t => t.File.Time).Aggregate((t1, t2) => t1 + t2) : 0;
            TimeSpan t = TimeSpan.FromSeconds(totalTime);

            PlaylistInfo = $"{Source.Count} Tracks, Total Time: {MiscHelpers.ToReadableString(t)}";


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

        private async void Source_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add && _previousAction == NotifyCollectionChangedAction.Remove)
            {
                // User reordered tracks, send matching command
                await MPDConnectionService.SafelySendCommandAsync(new PlaylistMoveCommand(Name, _oldId, e.NewStartingIndex));
                _previousAction = NotifyCollectionChangedAction.Move;
            }
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                _previousAction = e.Action;
                _oldId = e.OldStartingIndex;
            }
        }
    }
}
