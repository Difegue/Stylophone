using System;
using System.Collections.ObjectModel;
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
using MpcNET.Types;
using Windows.UI.Xaml.Media.Imaging;

namespace FluentMPC.ViewModels
{
    public class PlaylistViewModel : Observable
    {
        public ObservableCollection<TrackViewModel> Source { get; private set; } = new ObservableCollection<TrackViewModel>();



        private ICommand _deletePlaylistCommand;
        public ICommand RemovePlaylistCommand => _deletePlaylistCommand ?? (_deletePlaylistCommand = new RelayCommand(DeletePlaylist));
        private async void DeletePlaylist()
        {
            throw new NotImplementedException();

            /* using (var c = await MPDConnectionService.GetConnectionAsync())
             {
                 foreach (var f in Item.Files)
                 {
                     var req = await c.InternalResource.SendAsync(new PlaylistAddCommand(playlistName, f.Path));
                 }
             }*/
        }

        private ICommand _addToQueueCommand;
        public ICommand AddPlaylistCommand => _addToQueueCommand ?? (_addToQueueCommand = new RelayCommand(AddToQueue));
        private async void AddToQueue()
        {
            using (var c = await MPDConnectionService.GetConnectionAsync())
            {
                await c.InternalResource.SendAsync(new LoadCommand(Name));
            }
        }

        private ICommand _playCommand;
        public ICommand PlayPlaylistCommand => _playCommand ?? (_playCommand = new RelayCommand(PlayAlbum));
        private async void PlayAlbum()
        {
            // Clear queue, add playlist and play
            using (var c = await MPDConnectionService.GetConnectionAsync())
            {
                var req = await c.InternalResource.SendAsync(new ClearCommand());
                if (!req.IsResponseValid) throw new Exception($"Couldn't clear queue!");

                await c.InternalResource.SendAsync(new LoadCommand(Name));
                await c.InternalResource.SendAsync(new PlayCommand(0));
            }
        }

        public PlaylistViewModel()
        {
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

        public BitmapImage PlaylistArt
        {
            get => _playlistArt;
            private set
            {
                DispatcherHelper.ExecuteOnUIThreadAsync(() => Set(ref _playlistArt, value));
            }
        }

        private BitmapImage _playlistArt;

        public async Task LoadDataAsync(string playlistName)
        {
            Name = playlistName;
            Source.Clear();

            using (var c = await MPDConnectionService.GetConnectionAsync())
            {
                var findReq = await c.InternalResource.SendAsync(new ListPlaylistInfoCommand(playlistName));
                if (!findReq.IsResponseValid) return;

                foreach (var item in findReq.Response.Content)
                {
                    Source.Add(new TrackViewModel(item));
                }

                Artists = findReq.Response.Content.
                            Select(f => f.Artist).Distinct().Aggregate((f1, f2) => $"{f1}, {f2}");

                var totalTime = Source.Select(t => t.File.Time).Aggregate((t1, t2) => t1 + t2);
                TimeSpan t = TimeSpan.FromSeconds(totalTime);

                PlaylistInfo = $"{Source.Count} Tracks, Total Time: {MiscHelpers.ToReadableString(t)}";

            }
        }
    }
}
