using System;
using System.Collections.ObjectModel;
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
        }

        private async void MPDConnectionService_SongChanged(object sender, SongChangedEventArgs e)
        {
            await DispatcherHelper.ExecuteOnUIThreadAsync(async () => await LoadDataAsync());
            // TODO focus played track
            //e.NewSongId
        }

        private ICommand _itemClickCommand;

        public ICommand ItemClickCommand => _itemClickCommand ?? (_itemClickCommand = new RelayCommand<TrackViewModel>(OnItemClick));

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
            OnPropertyChanged(nameof(Source));
        }

        private void OnItemClick(TrackViewModel clickedItem)
        {
            if (clickedItem != null)
            {
                //NavigationService.Frame.SetListDataItemForNextConnectedAnimation(clickedItem);
                //NavigationService.Navigate<LibraryDetailPage>(clickedItem.OrderID);
            }
        }
    }
}
