using System;
using System.Collections.ObjectModel;
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

namespace FluentMPC.ViewModels
{
    public class QueueViewModel : Observable
    {
        public QueueViewModel()
        {
            MPDConnectionService.QueueChanged += MPDConnectionService_QueueChanged;
            MPDConnectionService.ConnectionChanged += MPDConnectionService_ConnectionChanged;

            Source.CollectionChanged += (s, e) => OnPropertyChanged(nameof(IsSourceEmpty));
        }

        private void MPDConnectionService_ConnectionChanged(object sender, EventArgs e)
        {
            if (MPDConnectionService.IsConnected)
                Task.Run(async () => await LoadDataAsync());
        }

        private async void MPDConnectionService_QueueChanged(object sender, EventArgs e)
        {
            // scrolling is handled in code-behind
            await DispatcherHelper.ExecuteOnUIThreadAsync(async () => await LoadDataAsync());
        }

        public ObservableCollection<TrackViewModel> Source { get; } = new ObservableCollection<TrackViewModel>();

        public bool IsSourceEmpty => Source.Count == 0;

        public async Task LoadDataAsync()
        {
            // TODO - Don't clear, update collection instead based on new data
            Source.Clear();
            var response = await MPDConnectionService.SafelySendCommandAsync(new PlaylistInfoCommand());

            if (response != null)
                foreach (var item in response)
                {
                    Source.Add(new TrackViewModel(item, false));
                }

            await DispatcherHelper.ExecuteOnUIThreadAsync(() => OnPropertyChanged(nameof(Source)));
        }
    }
}
