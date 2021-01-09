using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Input;
using FluentMPC.Helpers;
using FluentMPC.Services;
using FluentMPC.ViewModels.Items;
using Microsoft.Toolkit.Uwp.Helpers;
using MpcNET;
using MpcNET.Commands.Playback;
using MpcNET.Commands.Playlist;
using MpcNET.Commands.Queue;
using MpcNET.Commands.Reflection;
using MpcNET.Commands.Status;
using MpcNET.Types;
using Sundew.Base.Collections;

namespace FluentMPC.ViewModels
{
    public class QueueViewModel : Observable
    {
        private NotifyCollectionChangedAction _previousAction;
        private int _oldId;
        private int _playlistVersion;

        private bool IsSingleTrackSelected(object list)
        {
            // Cast the received __ComObject
            var selectedTracks = (IList<object>)list;

            return (selectedTracks?.Count == 1);
        }

        private ICommand _playCommand;
        public ICommand PlayTrackCommand => _playCommand ?? (_playCommand = new RelayCommand<IList<object>>(PlayTrack, IsSingleTrackSelected));

        private async void PlayTrack(object list)
        {
            // Cast the received __ComObject
            var selectedTracks = (IList<object>)list;

            if (selectedTracks?.Count > 0)
            {
                var trackVM = selectedTracks.First() as TrackViewModel;
                await MPDConnectionService.SafelySendCommandAsync(new PlayIdCommand(trackVM.File.Id));
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

        private ICommand _removeCommand;
        public ICommand RemoveFromQueueCommand => _removeCommand ?? (_removeCommand = new RelayCommand<IList<object>>(RemoveTrack));

        private async void RemoveTrack(object list)
        {
            var selectedTracks = (IList<object>)list;

            if (selectedTracks?.Count > 0)
            {
                var commandList = new CommandList();

                foreach (var f in selectedTracks)
                {
                    var trackVM = f as TrackViewModel;
                    commandList.Add(new DeleteIdCommand(trackVM.File.Id));
                }

                await MPDConnectionService.SafelySendCommandAsync(commandList);
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

        public QueueViewModel()
        {
            MPDConnectionService.QueueChanged += MPDConnectionService_QueueChanged;
            MPDConnectionService.ConnectionChanged += MPDConnectionService_ConnectionChanged;

            Source.CollectionChanged += (s, e) => OnPropertyChanged(nameof(IsSourceEmpty));
        }

        /// <summary>
        /// This method is only fired if the source is changed outside of MPD Events.
        /// So basically, only for reordering right now!
        /// When reordering multiple items, the ListView sends events in a remove->add->remove->add fashion.
        /// </summary>
        private async void Source_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add && _previousAction == NotifyCollectionChangedAction.Remove)
            {
                // One Remove->Add Pair means one MoveIdCommand
                await MPDConnectionService.SafelySendCommandAsync(new MoveIdCommand(_oldId, e.NewStartingIndex));
                _previousAction = NotifyCollectionChangedAction.Move;
            }
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                _previousAction = e.Action;
                _oldId = e.OldItems.Count > 0 ? (e.OldItems[0] as TrackViewModel).File.Id : -1;
            }
        }

        private void MPDConnectionService_ConnectionChanged(object sender, EventArgs e)
        {
            if (MPDConnectionService.IsConnected)
                Task.Run(async () => await LoadInitialDataAsync());
        }

        private async void MPDConnectionService_QueueChanged(object sender, EventArgs e)
        {
            // Ask for a new status ourselves as the shared ConnectionService one might not be up to date yet
            var status = await MPDConnectionService.SafelySendCommandAsync(new StatusCommand());
            var newVersion = status.Playlist;

            // Update the queue only if playlist versions differ
            if (newVersion != _playlistVersion)
            {
                _ = Task.Run(async () => {

                    var response = await MPDConnectionService.SafelySendCommandAsync(new PlChangesCommand(_playlistVersion));

                    if (response != null)
                    {
                        Source.CollectionChanged -= Source_CollectionChanged;
                        await DispatcherHelper.ExecuteOnUIThreadAsync(() => {

                            // If the queue was cleared, PlaylistLength is 0.
                            if (status.PlaylistLength == 0)
                            {
                                Source.Clear();
                            }
                            else
                            {
                                // PlChanges gives the full list of files starting from the change, so we delete all existing tracks from the source after that change, and swap the new ones in.
                                // If the response is empty, that means the last file in the source was removed.
                                var initialPosition = response.Count() == 0 ? Source.Count - 1 : response.First().Position;


                                while (Source.Count != initialPosition)
                                {
                                    Source.RemoveAt(initialPosition);
                                }

                                foreach (var item in response)
                                {
                                    Source.Add(new TrackViewModel(item, false));
                                }
                            }

                        });
                        Source.CollectionChanged += Source_CollectionChanged;

                        // Update internal playlist version
                        _playlistVersion = newVersion;
                    }
                });
            }
        }

        public ObservableCollection<TrackViewModel> Source { get; } = new ObservableCollection<TrackViewModel>();

        public bool IsSourceEmpty => Source.Count == 0;

        public async Task LoadInitialDataAsync()
        {
            var tracks = new List<TrackViewModel>();
            var response = await MPDConnectionService.SafelySendCommandAsync(new PlaylistInfoCommand());

            if (response != null)
                foreach (var item in response)
                {
                    tracks.Add(new TrackViewModel(item, false));
                }

            await DispatcherHelper.ExecuteOnUIThreadAsync(() => {
                Source.Clear();
                Source.AddRange(tracks);
            });

            // Set our internal playlist version
            var status = await MPDConnectionService.SafelySendCommandAsync(new StatusCommand());
            _playlistVersion = status.Playlist;

            Source.CollectionChanged += Source_CollectionChanged;
        }
    }
}
