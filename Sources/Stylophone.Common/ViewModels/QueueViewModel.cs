using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using MpcNET.Commands.Playback;
using MpcNET.Commands.Playlist;
using MpcNET.Commands.Queue;
using MpcNET.Commands.Reflection;
using MpcNET.Commands.Status;
using Stylophone.Common.Interfaces;
using Stylophone.Common.Services;
using Stylophone.Localization.Strings;

namespace Stylophone.Common.ViewModels
{
    public class QueueViewModel : ViewModelBase
    {
        private NotifyCollectionChangedAction _previousAction;
        private int _oldId;

        private IDialogService _dialogService;
        private INotificationService _notificationService;
        private MPDConnectionService _mpdService;
        private TrackViewModelFactory _trackVmFactory;

        public QueueViewModel(IDialogService dialogService, INotificationService notificationService, IDispatcherService dispatcherService, MPDConnectionService mpdService, TrackViewModelFactory trackViewModelFactory) :
            base(dispatcherService)
        {
            _dialogService = dialogService;
            _notificationService = notificationService;
            _mpdService = mpdService;
            _trackVmFactory = trackViewModelFactory;

            _mpdService.QueueChanged += MPDConnectionService_QueueChanged;
            _mpdService.ConnectionChanged += MPDConnectionService_ConnectionChanged;

            if (_mpdService.IsConnected)
            {
                MPDConnectionService_ConnectionChanged(this, null);
            }

            Source.CollectionChanged += (s, e) => OnPropertyChanged(nameof(IsSourceEmpty));
        }

        public static new string GetHeader() => Resources.QueueHeader;
        private bool IsSingleTrackSelected(object list)
        {
            // Cast the received __ComObject
            var selectedTracks = (IList<object>)list;

            return (selectedTracks?.Count == 1);
        }

        private ICommand _playCommand;
        public ICommand PlayTrackCommand => _playCommand ?? (_playCommand = new AsyncRelayCommand<IList<object>>(PlayTrackAsync, IsSingleTrackSelected));

        private async Task PlayTrackAsync(object list)
        {
            // Cast the received __ComObject
            var selectedTracks = (IList<object>)list;

            if (selectedTracks?.Count > 0)
            {
                var trackVM = selectedTracks.First() as TrackViewModel;
                await _mpdService.SafelySendCommandAsync(new PlayIdCommand(trackVM.File.Id));
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
        public ICommand RemoveFromQueueCommand => _removeCommand ?? (_removeCommand = new AsyncRelayCommand<IList<object>>(RemoveTrackAsync));

        private async Task RemoveTrackAsync(object list)
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
                // The delete command is fired twice -- Make sure the deleted tracks are unselected to avoid sending a second DeleteIdCommand.
                selectedTracks.Clear();

                await _mpdService.SafelySendCommandAsync(commandList);
            }
        }

        private ICommand _addToPlaylistCommand;
        public ICommand AddToPlayListCommand => _addToPlaylistCommand ?? (_addToPlaylistCommand = new AsyncRelayCommand<IList<object>>(AddToPlaylist));

        private async Task AddToPlaylist(object list)
        {
            var playlistName = await _dialogService.ShowAddToPlaylistDialog();
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

                var req = await _mpdService.SafelySendCommandAsync(commandList);

                if (req != null)
                    _notificationService.ShowInAppNotification(string.Format(Resources.NotificationAddedToPlaylist, playlistName));
            }
        }

        private ICommand _saveQueueCommand;
        /// <summary>
        /// Write the current queue to a playlist.
        /// </summary>
        public ICommand SaveQueueCommand => _saveQueueCommand ?? (_saveQueueCommand = new AsyncRelayCommand(SaveQueueAsync));

        private async Task SaveQueueAsync()
        {
            var playlistName = await _dialogService.ShowAddToPlaylistDialog(false);
            if (playlistName == null) return;

            var req = await _mpdService.SafelySendCommandAsync(new SaveCommand(playlistName));

            if (req != null)
                _notificationService.ShowInAppNotification(string.Format(Resources.NotificationAddedToPlaylist, playlistName));
        }

        private ICommand _clearQueueCommand;
        /// <summary>
        /// Clear the MPD queue.
        /// </summary>
        public ICommand ClearQueueCommand => _clearQueueCommand ?? (_clearQueueCommand = new AsyncRelayCommand(ClearQueueAsync));

        private async Task ClearQueueAsync()
        {
            await _mpdService.SafelySendCommandAsync(new ClearCommand());
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
                await _mpdService.SafelySendCommandAsync(new MoveIdCommand(_oldId, e.NewStartingIndex));
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
            if (_mpdService.IsConnected)
                Task.Run(async () => await LoadInitialDataAsync());
            else
                _dispatcherService.ExecuteOnUIThreadAsync(() => Source.Clear());
        }

        private async void MPDConnectionService_QueueChanged(object sender, EventArgs e)
        {
            // Ask for a new status ourselves as the shared ConnectionService one might not be up to date yet
            var status = await _mpdService.SafelySendCommandAsync(new StatusCommand());
            var newVersion = status?.Playlist;

            // Update the queue only if playlist versions differ
            if (newVersion != null && newVersion != PlaylistVersion)
            {
                _ = Task.Run(async () =>
                {

                    var response = await _mpdService.SafelySendCommandAsync(new PlChangesCommand(PlaylistVersion));

                    if (response != null)
                    {
                        Source.CollectionChanged -= Source_CollectionChanged;

                        // If the queue was cleared, PlaylistLength is 0.
                        if (status.PlaylistLength == 0)
                        {
                            await _dispatcherService.ExecuteOnUIThreadAsync(() => Source.Clear());
                        }
                        else
                        {
                            // PlChanges gives the full list of files starting from the change, so we delete all existing tracks from the source after that change, and swap the new ones in.
                            // If the response is empty, that means the last file in the source was removed.
                            var initialPosition = response.Count() == 0 ? Source.Count - 1 : response.First().Position;

                            while (Source.Count != initialPosition)
                            {
                                await _dispatcherService.ExecuteOnUIThreadAsync(() => Source.RemoveAt(initialPosition));
                                //var toRemove = Source.Skip(initialPosition - 1).Take(Source.Count - initialPosition);
                                //await _dispatcherService.ExecuteOnUIThreadAsync(() => Source.RemoveRange(toRemove));
                            }

                            var toAdd = new List<TrackViewModel>();
                            foreach (var item in response)
                            {
                                var trackVm = _trackVmFactory.GetTrackViewModel(item);
                                toAdd.Add(trackVm);
                            }

                            await _dispatcherService.ExecuteOnUIThreadAsync(() => Source.AddRange(toAdd));
                        }

                        Source.CollectionChanged += Source_CollectionChanged;

                        // Update internal playlist version
                        PlaylistVersion = newVersion.Value;
                    }
                });
            }
        }

        public RangedObservableCollection<TrackViewModel> Source { get; } = new RangedObservableCollection<TrackViewModel>();

        public bool IsSourceEmpty => Source.Count == 0;

        private int _playlistVersion;
        public int PlaylistVersion
        {
            get => _playlistVersion;
            private set => Set(ref _playlistVersion, value);
        }

        public async Task LoadInitialDataAsync()
        {
            var tracks = new List<TrackViewModel>();
            var response = await _mpdService.SafelySendCommandAsync(new PlaylistInfoCommand());

            if (response != null)
                foreach (var item in response)
                {
                    var trackVm = _trackVmFactory.GetTrackViewModel(item);
                    tracks.Add(trackVm);
                }

            await _dispatcherService.ExecuteOnUIThreadAsync(() =>
            {
                Source.Clear();
                Source.AddRange(tracks);
            });

            // Set our internal playlist version
            var status = await _mpdService.SafelySendCommandAsync(new StatusCommand());

            if (status != null)
                PlaylistVersion = status.Playlist;

            Source.CollectionChanged += Source_CollectionChanged;
        }
    }
}
