using ColorThiefDotNet;
using FluentMPC.Helpers;
using FluentMPC.Services;
using Microsoft.Toolkit.Uwp.Helpers;
using MpcNET.Commands.Database;
using MpcNET.Tags;
using MpcNET.Types;
using Sundew.Base.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
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
            private set
            {
                DispatcherHelper.ExecuteOnUIThreadAsync(() => Set(ref _artist, value));
            }
        }
        private String _artist;

        public IList<IMpdFile> Files {
            get => _files;
            set => Set(ref _files, value);
        }
        private IList<IMpdFile> _files;

        private bool _detailLoading;
        public bool IsDetailLoading
        {
            get => _detailLoading;
            private set
            {
                DispatcherHelper.ExecuteOnUIThreadAsync(() => Set(ref _detailLoading, value));
            }
        }

        public bool IsFullyLoaded { get; set; }

        public BitmapImage AlbumArt
        {
            get => _albumArt;
            private set
            {
                DispatcherHelper.ExecuteOnUIThreadAsync(() => Set(ref _albumArt, value));
            }
        }

        private BitmapImage _albumArt;

        public AlbumViewModel(string albumName)
        {
            Name = albumName;
            Files = new List<IMpdFile>();
        }

        public async Task LoadAlbumDataAsync()
        {
            IsDetailLoading = true;

            using (var c = await MPDConnectionService.GetConnectionAsync())
            {
                var findReq = await c.InternalResource.SendAsync(new FindCommand(MpdTags.Album, Name));
                if (!findReq.IsResponseValid) return;

                Files.AddRange(findReq.Response.Content);
                Artist = Files.Select(f => f.Artist).Distinct().Aggregate((f1, f2) => $"{f1},{f2}");
            }

            // Fire off an async request to get the album art from MPD.
            if (Files.Count > 0)
                _ = Task.Run(async () =>
                {
                    var art = await MiscHelpers.GetAlbumArtAsync(Files[0]);
                    AlbumArt = await MiscHelpers.WriteableBitmapToBitmapImageAsync(art, 180);

                    IsFullyLoaded = true;
                    IsDetailLoading = false;
                });
        }
    }
}
