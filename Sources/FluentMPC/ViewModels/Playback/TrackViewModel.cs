using FluentMPC.Services;
using Microsoft.Toolkit.Uwp.UI.Controls.TextToolbarSymbols;
using MpcNET.Commands.Database;
using MpcNET.Types;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace FluentMPC.ViewModels.Playback
{
    public class TrackViewModel
    {
        public IMpdFile File { get; }

        public BitmapImage AlbumArt { get; private set; }

        public static async Task<TrackViewModel> FromMpdFile(IMpdFile f)
        {
            var image = await GetAlbumArt(f);
            return new TrackViewModel(f, image);
        }

        private TrackViewModel(IMpdFile file, BitmapImage image)
        {
            File = file;
            AlbumArt = image;
        }

        public static async Task<BitmapImage> GetAlbumArt(IMpdFile f)
        {
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

                        var response = albumReq.Response.Content;
                        totalBinarySize = response.Size;
                        currentSize += response.Binary;
                        data.AddRange(response.Data);

                    } while (currentSize < totalBinarySize);

                    return await ImageFromBytes(data.ToArray());
                }
            }
            catch (Exception e)
            {
                // TODO fallback
                return new BitmapImage();
            }
            
        }
        public async static Task<BitmapImage> ImageFromBytes(Byte[] bytes)
        {
            BitmapImage image = new BitmapImage();
            using (InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream())
            {
                await stream.WriteAsync(bytes.AsBuffer());
                stream.Seek(0);
                await image.SetSourceAsync(stream);
            }
            return image;
        }
    }
}
