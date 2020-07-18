using ColorThiefDotNet;
using FluentMPC.Services;
using Microsoft.Toolkit.Uwp.Helpers;
using MpcNET.Commands.Database;
using MpcNET.Types;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using Color = Windows.UI.Color;

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

        public async static Task<WriteableBitmap> GetAlbumArtAsync(IMpdFile f)
        {
            WriteableBitmap result;
            var foundUsableArt = false;

            // Try loading from art cache first
            result = await LoadImageFromFile(f.Id);

            if (result != null)
                return result;

            // Get albumart from MPD
            List<byte> data = new List<byte>();
            try
            {
                using (var c = await MPDConnectionService.GetAlbumArtConnectionAsync())
                {
                    int totalBinarySize = 9999;
                    int currentSize = 0;

                    do
                    {
                        var albumReq = await c.InternalResource.SendAsync(new AlbumArtCommand(f.Path, currentSize));
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
                            var albumReq = await c.InternalResource.SendAsync(new ReadPictureCommand(f.Path, currentSize));
                            if (!albumReq.IsResponseValid) break;

                            var response = albumReq.Response.Content;
                            totalBinarySize = response.Size;
                            currentSize += response.Binary;
                            data.AddRange(response.Data);
                            foundUsableArt = true;
                        } while (currentSize < totalBinarySize);

                    // Create the BitmapImage on the UI Thread.
                    if (foundUsableArt)
                        result = await ImageFromBytes(data.ToArray());
                    else
                        throw new Exception();
                }
            }
            catch (Exception e)
            {
                // Fallback
                result = await BitmapFactory.FromContent(new Uri("ms-appx:///Assets/AlbumPlaceholder.png"));
            }

            if (foundUsableArt)
                await SaveArtToFileAsync(f.Id, data); //TODO use hash of data instead of ID to avoid duplicate albumart being saved

            return result;
        }

        internal static async Task<Color> GetDominantColor(WriteableBitmap art)
        {
            //get dominant color of albumart
            using (InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream())
            {
                await DispatcherHelper.ExecuteOnUIThreadAsync(async () => await art.ToStreamAsJpeg(stream));
                stream.Seek(0);

                var colorThief = new ColorThief();
                QuantizedColor quantizedColor = await colorThief.GetColor(await BitmapDecoder.CreateAsync(stream));

                return Color.FromArgb(quantizedColor.Color.A, quantizedColor.Color.R, quantizedColor.Color.G, quantizedColor.Color.B);
            }
        }

        private static async Task SaveArtToFileAsync(int Id, List<byte> data)
        {
            StorageFolder pictureFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("AlbumArt", CreationCollisionOption.OpenIfExists);
            var file = await pictureFolder.CreateFileAsync(Id.ToString(), CreationCollisionOption.ReplaceExisting);

            await FileIO.WriteBytesAsync(file, data.ToArray());
        }

        internal static async Task<BitmapImage> WriteableBitmapToBitmapImageAsync(WriteableBitmap art, int decodedPixelWidth)
        {
            BitmapImage image = null;
            await DispatcherHelper.ExecuteOnUIThreadAsync(async () =>
            {
                image = new BitmapImage();

                using (InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream())
                {
                    await art.ToStreamAsJpeg(stream);
                    stream.Seek(0);

                    await image.SetSourceAsync(stream);
                }

                if (decodedPixelWidth > 0)
                    image.DecodePixelWidth = decodedPixelWidth;
            });
            return image;
        }

        private static async Task<WriteableBitmap> LoadImageFromFile(int Id)
        {
            try
            {
                var fileName = Id.ToString();
                StorageFolder pictureFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("AlbumArt", CreationCollisionOption.OpenIfExists);
                
                if (await pictureFolder.FileExistsAsync(fileName))
                {
                    var file = await pictureFolder.GetFileAsync(fileName);
                    var readStream = await file.OpenReadAsync();

                    WriteableBitmap image = null;
                    await DispatcherHelper.ExecuteOnUIThreadAsync(async () => image = await BitmapFactory.FromStream(readStream));

                    readStream.Dispose();
                    return image;
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async static Task<WriteableBitmap> ImageFromBytes(byte[] bytes)
        {
            WriteableBitmap image = null;
            using (InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream())
            {
                await stream.WriteAsync(bytes.AsBuffer());
                stream.Seek(0);

                await DispatcherHelper.ExecuteOnUIThreadAsync(async () => image = await BitmapFactory.FromStream(stream));
            }
            return image;
        }
    }
}
