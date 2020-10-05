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
using MpcNET.Types;
using Sundew.Base.Collections;

namespace FluentMPC.ViewModels
{
    public class QueueViewModel : Observable
    {
        private NotifyCollectionChangedAction _previousAction;
        private int _oldId;

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
                Task.Run(async () => await LoadDataAsync());
        }

        private void MPDConnectionService_QueueChanged(object sender, EventArgs e)
        {
            // scrolling is handled in code-behind
            Task.Run(async () => await LoadDataAsync());
        }

        public ObservableCollection<TrackViewModel> Source { get; } = new ObservableCollection<TrackViewModel>();

        public bool IsSourceEmpty => Source.Count == 0;

        public async Task LoadDataAsync()
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
                // TODO - Don't clear, update collection instead based on new data
                Source.Clear();
                Source.AddRange(tracks);
            });

            Source.CollectionChanged += Source_CollectionChanged;
        }
    }
}
