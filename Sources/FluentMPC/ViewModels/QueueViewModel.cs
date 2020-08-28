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
using MpcNET.Commands.Playlist;
using MpcNET.Types;

namespace FluentMPC.ViewModels
{
    public class QueueViewModel : Observable
    {
        public QueueViewModel()
        {
            MPDConnectionService.SongChanged += MPDConnectionService_SongChanged;
            MPDConnectionService.ConnectionChanged += MPDConnectionService_ConnectionChanged;
        }

        private void MPDConnectionService_ConnectionChanged(object sender, EventArgs e)
        {
            if (MPDConnectionService.IsConnected)
                Task.Run(async () => await LoadDataAsync());
        }

        private async void MPDConnectionService_SongChanged(object sender, SongChangedEventArgs e)
        {
            // TODO handle queue updates with QueueModified Event
            //await LoadDataAsync();

            // scrolling is handled in code-behind

        }

        public ObservableCollection<TrackViewModel> Source { get; } = new ObservableCollection<TrackViewModel>();

        public async Task LoadDataAsync()
        {
            // TODO - Don't clear, update collection instead based on new data
            Source.Clear();

            using (var c = await MPDConnectionService.GetConnectionAsync())
            {
                var response = await c.InternalResource.SendAsync(new PlaylistInfoCommand());

                if (response.IsResponseValid)
                    foreach (var item in response.Response.Content)
                    {
                        Source.Add(new TrackViewModel(item, false));
                    }
            }
            await DispatcherHelper.ExecuteOnUIThreadAsync(() => OnPropertyChanged(nameof(Source)));
        }
    }
}
