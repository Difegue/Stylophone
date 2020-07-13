using FluentMPC.Services;
using Microsoft.Toolkit.Uwp.Helpers;
using MpcNET.Commands.Database;
using MpcNET.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace FluentMPC.Helpers
{
    public static class MiscHelpers
    {
        /// <summary>
        ///     Formats a timespan into a human
        ///     readable form.
        /// </summary>
        /// <param name="inputMilliseconds">Time to format</param>
        /// <returns>Formatted time string</returns>
        public static string FormatTimeString(double inputMilliseconds)
        {
            // Convert the milliseconds into a usable timespan
            var timeSpan = TimeSpan.FromMilliseconds(inputMilliseconds);

            // Check if the length is less than one minute
            if (timeSpan.TotalMinutes < 1.0)
                return string.Format("{0:D2}:{1:D2}", 0, timeSpan.Seconds);

            return timeSpan.TotalHours < 1.0
                ? string.Format("{0:D2}:{1:D2}", timeSpan.Minutes, timeSpan.Seconds)
                : string.Format("{0:D2}:{1:D2}:{2:D2}", (int)timeSpan.TotalHours, timeSpan.Minutes, timeSpan.Seconds);
        }

        public async static Task<BitmapImage> GetAlbumArtAsync(IMpdFile f, int albumArtWidth)
        {
            // TODO: Add some cache 
            BitmapImage result = null;

            // Get albumart from MPD
            try
            {
                using (var c = await MPDConnectionService.GetConnectionAsync())
                {
                    int totalBinarySize = 9999;
                    int currentSize = 0;
                    List<byte> data = new List<byte>();

                    var foundUsableArt = false;

                    do
                    {
                        var albumReq = await c.SendAsync(new AlbumArtCommand(f.Path, currentSize));
                        if (!albumReq.IsResponseValid) break;

                        var response = albumReq.Response.Content;
                        totalBinarySize = response.Size;
                        currentSize += response.Binary;
                        data.AddRange(response.Data);
                        foundUsableArt = true;
                    } while (currentSize < totalBinarySize);

                    // Fallback to readpicture if albumart didn't work
                    if (!foundUsableArt) do
                        {
                            var albumReq = await c.SendAsync(new ReadPictureCommand(f.Path, currentSize));
                            if (!albumReq.IsResponseValid) break;

                            var response = albumReq.Response.Content;
                            totalBinarySize = response.Size;
                            currentSize += response.Binary;
                            data.AddRange(response.Data);
                            foundUsableArt = true;
                        } while (currentSize < totalBinarySize);

                    // Create the BitmapImage on the UI Thread.
                    if (foundUsableArt)
                        await DispatcherHelper.ExecuteOnUIThreadAsync(
                            async () => result = await ImageFromBytes(data.ToArray()));
                    else
                        throw new Exception();
                }
            }
            catch (Exception e)
            {
                // Fallback
                await DispatcherHelper.ExecuteOnUIThreadAsync(() =>
                    result = new BitmapImage(new Uri("ms-appx:///Assets/AlbumPlaceholder.png")));
            }

            if (albumArtWidth > 0)
                await DispatcherHelper.ExecuteOnUIThreadAsync(() => result.DecodePixelWidth = albumArtWidth);

            return result;
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
            return image;
        }
    }
}
