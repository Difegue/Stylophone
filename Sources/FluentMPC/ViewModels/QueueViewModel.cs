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
using MpcNET.Commands.Queue;
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

        public QueueViewModel()
        {
            MPDConnectionService.QueueChanged += MPDConnectionService_QueueChanged;
            MPDConnectionService.ConnectionChanged += MPDConnectionService_ConnectionChanged;

            Source.CollectionChanged += (s, e) => OnPropertyChanged(nameof(IsSourceEmpty));
        }

        private async void Source_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add && _previousAction == NotifyCollectionChangedAction.Remove)
            {
                // User reordered tracks, send matching command
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

                            // PlChanges gives the full list of files starting from the change, so we delete all existing tracks from the source after that change, and swap the new ones in.
                            var initialPosition = response.First().Position;

                            while (Source.Count != initialPosition)
                            {
                                Source.RemoveAt(initialPosition);
                            }

                            foreach (var item in response)
                            {
                                Source.Add(new TrackViewModel(item, false));
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
            Source.CollectionChanged -= Source_CollectionChanged;

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
