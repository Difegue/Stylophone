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

            // This allows to cache per album, avoiding saving the same album art a ton of times.
            // Doesn't work if files in an album have different albumarts, but that happens so rarely it's fine to ignore it.
            var uniqueIdentifier = f.HasAlbum ? f.Album : f.HasTitle ? f.Title : f.Path;

            // Try loading from art cache first
            result = await LoadImageFromFile(uniqueIdentifier);

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
                await DispatcherHelper.ExecuteOnUIThreadAsync(async () =>
                    result = await BitmapFactory.FromContent(new Uri("ms-appx:///Assets/AlbumPlaceholder.png")));
            }

            if (foundUsableArt)
                await SaveArtToFileAsync(uniqueIdentifier, data); 

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

        private static async Task SaveArtToFileAsync(string Id, List<byte> data)
        {
            var fileName = EscapeFilename(Id);
            StorageFolder pictureFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("AlbumArt", CreationCollisionOption.OpenIfExists);
            var file = await pictureFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);

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

        private static async Task<WriteableBitmap> LoadImageFromFile(string Id)
        {
            try
            {
                var fileName = EscapeFilename(Id);
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

        /// <summary>
        /// Escapes an object name so that it is a valid filename.
        /// </summary>
        /// <param name="fileName">Original object name.</param>
        /// <returns>Escaped name.</returns>
        /// <remarks>
        /// All characters that are not valid for a filename, plus "%" and ".", are converted into "%uuuu", where uuuu is the hexadecimal
        /// unicode representation of the character.
        /// </remarks>
        public static string EscapeFilename(string fileName)
        {
            char[] invalidChars = Path.GetInvalidFileNameChars();

            // Replace "%", then replace all other characters, then replace "."

            fileName = fileName.Replace("%", "%0025");
            foreach (char invalidChar in invalidChars)
            {
                fileName = fileName.Replace(invalidChar.ToString(), string.Format("%{0,4:X}", Convert.ToInt16(invalidChar)).Replace(' ', '0'));
            }
            return fileName.Replace(".", "%002E");
        }
    }
}
