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
using System.Threading;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Media.Imaging;

namespace FluentMPC.ViewModels.Items
{
    public class AlbumViewModel : Observable
    {
        public String Name
        {
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

        public IList<IMpdFile> Files
        {
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

        private bool _artLoaded;
        public bool AlbumArtLoaded
        {
            get => _artLoaded;
            private set
            {
                DispatcherHelper.ExecuteOnUIThreadAsync(() => Set(ref _artLoaded, value));
            }
        }

        public BitmapImage AlbumArt
        {
            get => _albumArt;
            private set
            {
                DispatcherHelper.ExecuteOnUIThreadAsync(() => Set(ref _albumArt, value));
            }
        }

        private BitmapImage _albumArt;

        public Color DominantColor
        {
            get => _albumColor;
            private set
            {
                DispatcherHelper.ExecuteOnUIThreadAsync(() => Set(ref _albumColor, value));
            }
        }

        private Color _albumColor;

        private bool _isLight;
        public bool IsLight
        {
            get => _isLight;
            private set
            {
                DispatcherHelper.ExecuteOnUIThreadAsync(() => Set(ref _isLight, value));
            }
        }

        public AlbumViewModel(string albumName)
        {
            Name = albumName;
            DominantColor = Colors.Black;
            Files = new List<IMpdFile>();
            IsDetailLoading = false;
        }

        public async Task LoadAlbumDataAsync(CancellationToken token = default)
        {
            IsDetailLoading = true;

            try
            {
                using (var c = await MPDConnectionService.GetConnectionAsync(token))
                {
                    var findReq = await c.InternalResource.SendAsync(new FindCommand(MpdTags.Album, Name));
                    if (!findReq.IsResponseValid) return;

                    Files.AddRange(findReq.Response.Content);
                    Artist = Files.Select(f => f.Artist).Distinct().Aggregate((f1, f2) => $"{f1}, {f2}");
                }

                // Fire off an async request to get the album art from MPD.
                Task.Run(async () =>
                {
                    if (Files.Count > 0)
                    {
                        var art = await AlbumArtHelpers.GetAlbumArtAsync(Files[0], token);

                        if (art != null)
                        {
                            AlbumArt = await AlbumArtHelpers.WriteableBitmapToBitmapImageAsync(art, 180);

                            var color = await AlbumArtHelpers.GetDominantColor(art);
                            IsLight = !color.IsDark;
                            DominantColor = color.ToWindowsColor();
                        }
                        
                        AlbumArtLoaded = true;
                    }
                });
            }
            finally
            {
                IsDetailLoading = false;
            }
        }
    }
}
