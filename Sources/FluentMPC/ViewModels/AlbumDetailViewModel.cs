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
using MpcNET.Commands.Queue;
using MpcNET.Commands.Reflection;
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

        private ICommand _addToPlaylistCommand;
        public ICommand AddToPlayListCommand => _addToPlaylistCommand ?? (_addToPlaylistCommand = new RelayCommand<IList<object>>(AddToPlaylist));

        private async void AddToPlaylist(object list)
        {
            var playlistName = await DialogService.ShowAddToPlaylistDialog();
            if (playlistName == null) return;

            var selectedTracks = (IList<object>)list;

            if (selectedTracks?.Count > 0)
            {
                var commandList = new CommandList();

                foreach (var f in selectedTracks)
                {
                    var trackVM = f as TrackViewModel;
                    commandList.Add(new PlaylistAddCommand(playlistName, trackVM.File.Path));
                }

                var req = await MPDConnectionService.SafelySendCommandAsync(commandList);

                if (req != null)
                    NotificationService.ShowInAppNotification(string.Format("AddedToPlaylistText".GetLocalized(), playlistName));
            }
        }


        public AlbumDetailViewModel()
        {
        }

        public async Task InitializeAsync(AlbumViewModel album)
        {
            Item = album;

            if (album.AlbumArtLoaded)
            {
                // Already loaded, create tracks now
                CreateTrackViewModels();
            }
            else if (album.IsDetailLoading)
            {
                // AlbumVM is currently loading, wait for it to conclude to create tracks
                // We only look at IsDetailLoading since we don't need the album art to be loaded for tracks
                album.PropertyChanged += async (s, e) =>
                {
                    if (album.Files.Count > 0 && Source.Count == 0)
                        await DispatcherHelper.ExecuteOnUIThreadAsync(() => CreateTrackViewModels());
                };
            }
            else // AlbumVM hasn't been loaded at all, load it ourselves
            {
                _ = Task.Run(async () =>
                {
                    using (var c = await MPDConnectionService.GetConnectionAsync())
                    {
                        await album.LoadAlbumDataAsync(c.InternalResource);
                    }
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
