using FluentMPC.Helpers;
using FluentMPC.Services;
using MpcNET.Commands.Database;
using MpcNET.Tags;
using MpcNET.Types;
using Sundew.Base.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;

namespace FluentMPC.ViewModels.Items
{
    public class AlbumViewModel : Observable
    {
        public String Name {
            get => _name;
            set => Set(ref _name, value);
        }
        private String _name;

        public String Artist
        {
            get => _artist;
            set => Set(ref _artist, value);
        }
        private String _artist;

        public IList<IMpdFile> Files {
            get => _files;
            set => Set(ref _files, value);
        }
        private IList<IMpdFile> _files;

        public BitmapImage AlbumArt
        {
            get => _albumArt;
            private set
            {
                Set(ref _albumArt, value);

                //TODO: get dominant color of albumart
                //var colorThief = new ColorThief();
                //QuantizedColor quantizedColor = colorThief.GetColor(decoder);
            }
        }

        private BitmapImage _albumArt;

        public AlbumViewModel(string albumName)
        {
            Name = albumName;
            Files = new List<IMpdFile>();

            Task.Run(() => LoadAlbumDataAsync());
        }

        public async Task LoadAlbumDataAsync()
        {
            using (var c = await MPDConnectionService.GetConnectionAsync())
            {
                var findReq = await c.SendAsync(new FindCommand(MpdTags.Album, Name));
                if (!findReq.IsResponseValid) return;

                Files.AddRange(findReq.Response.Content);
                Artist = Files.Select(f => f.Artist).Distinct().Aggregate((f1, f2) => $"{f1},{f2}");
            }

            // Fire off an async request to get the album art from MPD.
            if (Files.Count > 0)
                _ = Task.Run(async () => AlbumArt = await MiscHelpers.GetAlbumArtAsync(Files[0]));
        }
    }
}
