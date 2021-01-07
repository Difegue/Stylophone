using System;
using System.Collections.Generic;
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
using MpcNET;
using MpcNET.Commands.Playback;
using MpcNET.Commands.Playlist;
using MpcNET.Commands.Queue;
using MpcNET.Commands.Reflection;
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

        #region Commands

        private bool IsSingleTrackSelected(object list)
        {
            // Cast the received __ComObject
            var selectedTracks = (IList<object>)list;

            return (selectedTracks?.Count == 1);
        }

        private ICommand _deletePlaylistCommand;
        public ICommand RemovePlaylistCommand => _deletePlaylistCommand ?? (_deletePlaylistCommand = new RelayCommand(DeletePlaylist));
        private async void DeletePlaylist()
        {
            ContentDialog confirmDialog = new ContentDialog
            {
                Title = "DeletePlaylistContentDialog".GetLocalized(),
                PrimaryButtonText = "OKButtonText".GetLocalized(),
                CloseButtonText = "CancelButtonText".GetLocalized()
            };

            ContentDialogResult result = await confirmDialog.ShowAsync();
            if (result != ContentDialogResult.Primary)
                return;

            var res = await MPDConnectionService.SafelySendCommandAsync(new RmCommand(Name));

            if (res != null)
            {
                NotificationService.ShowInAppNotification("PlaylistRemovedText".GetLocalized());
                NavigationService.GoBack();
            }
        }

        private ICommand _loadPlaylistCommand;
        public ICommand LoadPlaylistCommand => _loadPlaylistCommand ?? (_loadPlaylistCommand = new RelayCommand(LoadPlaylist));
        private async void LoadPlaylist()
        {
            var res = await MPDConnectionService.SafelySendCommandAsync(new LoadCommand(Name));

            if (res != null)
                NotificationService.ShowInAppNotification("AddedToQueueText".GetLocalized());
        }
        private ICommand _playCommand;
        public ICommand PlayPlaylistCommand => _playCommand ?? (_playCommand = new RelayCommand(PlayPlaylist));
        private async void PlayPlaylist()
        {
            // Clear queue, add playlist and play
            var commandList = new CommandList(new IMpcCommand<object>[] { new ClearCommand() , new LoadCommand(Name), new PlayCommand(0) });

            if (await MPDConnectionService.SafelySendCommandAsync(commandList) != null)
            {
                // Auto-navigate to the queue
                NavigationService.Navigate(typeof(Views.ServerQueuePage));
            }
        }

        private ICommand _addToQueueCommand;
        public ICommand AddToQueueCommand => _addToQueueCommand ?? (_addToQueueCommand = new RelayCommand<IList<object>>(QueueTrack));

        private async void QueueTrack(object list)
        {
            var selectedTracks = (IList<object>)list;

            if (selectedTracks?.Count > 0)
            {
                var commandList = new CommandList();

                foreach (var f in selectedTracks)
                {
                    var trackVM = f as TrackViewModel;
                    commandList.Add(new AddIdCommand(trackVM.File.Path));
                }

                var r = await MPDConnectionService.SafelySendCommandAsync(commandList);
                if (r != null) 
                    NotificationService.ShowInAppNotification("AddedToQueueText".GetLocalized());
            }
        }

        private ICommand _viewAlbumCommand;
        public ICommand ViewAlbumCommand => _viewAlbumCommand ?? (_viewAlbumCommand = new RelayCommand<IList<object>>(ViewAlbum, IsSingleTrackSelected));

        private void ViewAlbum(object list)
        {
            // Cast the received __ComObject
            var selectedTracks = (IList<object>)list;

            if (selectedTracks?.Count > 0)
            {
                var trackVM = selectedTracks.First() as TrackViewModel;
                trackVM.ViewAlbumCommand.Execute(trackVM.File);
            }
        }

        private ICommand _removeTrackCommand;
        public ICommand RemoveTrackFromPlaylistCommand => _removeTrackCommand ?? (_removeTrackCommand = new RelayCommand<IList<object>> (RemoveTrack));

        private async void RemoveTrack(object list)
        {
            var selectedTracks = (IList<object>)list;

            if (selectedTracks?.Count > 0)
            {
                var commandList = new CommandList();

                // We can't batch PlaylistDeleteCommands cleanly, since they're index-based and logically, said indexes will shift as we remove stuff from the playlist.
                // To simulate this behavior, we copy our Source list and incrementally remove the affected tracks from it to get the valid indexes as we move down the commandList.
                IList<TrackViewModel> copy = Source.ToList();

                foreach (var f in selectedTracks)
                {
                    var trackVM = f as TrackViewModel;
                    commandList.Add(new PlaylistDeleteCommand(Name, copy.IndexOf(trackVM)));
                    copy.Remove(trackVM);
                }

                var r = await MPDConnectionService.SafelySendCommandAsync(commandList);
                if (r != null) // Reload playlist
                    await LoadDataAsync(Name);
            }
        }
        #endregion

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
                    var art = await AlbumArtService.GetAlbumArtAsync(distinctAlbums[0].File, false, 150);
                    PlaylistArt = art != null ? art.ArtBitmap : PlaylistArt;
                }

                if (distinctAlbums.Count > 2)
                {
                    var art = await AlbumArtService.GetAlbumArtAsync(distinctAlbums[1].File, false, 150);
                    PlaylistArt2 = art != null ? art.ArtBitmap : PlaylistArt2;
                }
                else PlaylistArt2 = PlaylistArt;

                if (distinctAlbums.Count > 3)
                {
                    var art = await AlbumArtService.GetAlbumArtAsync(distinctAlbums[2].File, false, 150);
                    PlaylistArt3 = art != null ? art.ArtBitmap : PlaylistArt3;
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
