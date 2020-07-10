using ColorThiefDotNet;
using FluentMPC.Helpers;
using FluentMPC.Services;
using Microsoft.Toolkit.Uwp.Helpers;
using Microsoft.Toolkit.Uwp.UI.Controls.TextToolbarSymbols;
using MpcNET.Commands.Database;
using MpcNET.Types;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml.Media.Imaging;

namespace FluentMPC.ViewModels.Playback
{
    public class TrackViewModel : Observable
    {
        public IMpdFile File { get; }

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

        public TrackViewModel(IMpdFile file, BitmapImage image)
        {
            File = file;
            AlbumArt = image;
        }

        public TrackViewModel(IMpdFile file)
        {
            File = file;
            AlbumArt = new BitmapImage();

            // Fire off an async request to get the album art from MPD.
            Task.Run(async () => await GetAlbumArtAsync(File));

        }

        public async Task GetAlbumArtAsync(IMpdFile f)
        {
            // TODO: Add some cache 

            // Get albumart from MPD
            try
            {
                using (var c = await MPDConnectionService.GetConnectionAsync())
                {
                    int totalBinarySize = 9999;
                    int currentSize = 0;
                    List<byte> data = new List<byte>();

                    do
                    {
                        var albumReq = await c.SendAsync(new AlbumArtCommand(f.Path, currentSize));
                        if (!albumReq.IsResponseValid) throw new Exception();
                        // TODO: Fallback to readpicture {URI} {OFFSET}

                        var response = albumReq.Response.Content;
                        totalBinarySize = response.Size;
                        currentSize += response.Binary;
                        data.AddRange(response.Data);

                    } while (currentSize < totalBinarySize);

                    // Create the BitmapImage on the UI Thread. 
                    await DispatcherHelper.ExecuteOnUIThreadAsync(
                        async () => AlbumArt = await ImageFromBytes(data.ToArray()));
                }
            }
            catch (Exception e)
            {
                // TODO fallback
                AlbumArt = new BitmapImage();
            }
            
        }
        public async static Task<BitmapImage> ImageFromBytes(byte[] bytes)
        {
            BitmapImage image = new BitmapImage();
            using (InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream())
            {
                await stream.WriteAsync(bytes.AsBuffer());
                stream.Seek(0);
                await image.SetSourceAsync(stream);
            }
            // TODO: Move this elsewhere to retain access to the HQ bitmapimage
            //image.DecodePixelWidth = 70;
            return image;
        }
    }
}
