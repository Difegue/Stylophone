using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

using FluentMPC.Core.Models;
using FluentMPC.Core.Services;
using FluentMPC.Helpers;
using FluentMPC.Services;
using FluentMPC.ViewModels.Items;
using Microsoft.Toolkit.Uwp.Helpers;
using Microsoft.Toolkit.Uwp.UI.Controls;
using MpcNET.Commands.Playlist;
using MpcNET.Types;
using Windows.UI.Xaml.Media.Imaging;

namespace FluentMPC.ViewModels
{
    public class PlaylistViewModel : Observable
    {
        public ObservableCollection<TrackViewModel> Source { get; private set; } = new ObservableCollection<TrackViewModel>();

        public MpdPlaylist Item { get; private set; }

        public PlaylistViewModel()
        {
        }

        public String Name
        {
            get => _name;
            set => Set(ref _name, value);
        }
        private String _name;

        public String Artists
        {
            get => _artists;
            private set
            {
                DispatcherHelper.ExecuteOnUIThreadAsync(() => Set(ref _artists, value));
            }
        }
        private String _artists;

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
            }
        }
    }
}
