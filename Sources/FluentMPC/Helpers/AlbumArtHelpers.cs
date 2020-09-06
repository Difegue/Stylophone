using ColorThiefDotNet;
using FluentMPC.Services;
using Microsoft.Toolkit.Uwp.Helpers;
using MpcNET.Commands.Database;
using MpcNET.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml.Media.Imaging;
using Color = Windows.UI.Color;

namespace FluentMPC.Helpers
{
    public static class AlbumArtHelpers
    {

        public async static Task<WriteableBitmap> GetAlbumArtAsync(IMpdFile f, CancellationToken token = default, CoreDispatcher dispatcher = null)
        {
            if (dispatcher == null)
                dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;

            WriteableBitmap result;
            var foundUsableArt = false;

            // This allows to cache per album, avoiding saving the same album art a ton of times.
            // Doesn't work if files in an album have different albumarts, but that happens so rarely it's fine to ignore it.
            var uniqueIdentifier = f.HasAlbum ? f.Album : f.HasTitle ? f.Title : f.Path;

            // Try loading from art cache first
            result = await LoadImageFromFile(uniqueIdentifier, dispatcher);

            if (result != null)
                return result;

            // Get albumart from MPD
            List<byte> data = new List<byte>();
            try
            {
                using (var c = await MPDConnectionService.GetAlbumArtConnectionAsync(token))
                {
                    if (c == null) // We got cancelled
                        throw new Exception();

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
                        result = await ImageFromBytes(data.ToArray(), dispatcher);
                    else
                        throw new Exception();
                }
            }
            catch
            {
                return null;
            }

            if (foundUsableArt)
                await SaveArtToFileAsync(uniqueIdentifier, data);

            return result;
        }

        internal static async Task<QuantizedColor> GetDominantColor(WriteableBitmap art, CoreDispatcher dispatcher = null)
        {
            if (dispatcher == null)
                dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;

            //get dominant color of albumart
            using (InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream())
            {
                await DispatcherHelper.AwaitableRunAsync(dispatcher, async () =>
                {
                    await art.Resize(50, 50, WriteableBitmapExtensions.Interpolation.NearestNeighbor).ToStreamAsJpeg(stream);
                });
                stream.Seek(0);

                var colorThief = new ColorThief();
                var quantizedColor = await colorThief.GetColor(await BitmapDecoder.CreateAsync(stream));

                return quantizedColor;
            }
        }

        private static async Task SaveArtToFileAsync(string Id, List<byte> data)
        {
            var fileName = MiscHelpers.EscapeFilename(Id);
            StorageFolder pictureFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("AlbumArt", CreationCollisionOption.OpenIfExists);
            await pictureFolder.SaveFileAsync(data.ToArray(), fileName, CreationCollisionOption.ReplaceExisting);
        }

        internal static async Task<BitmapImage> WriteableBitmapToBitmapImageAsync(WriteableBitmap art, int decodedPixelWidth, CoreDispatcher dispatcher = null)
        {
            if (dispatcher == null)
                dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;

            BitmapImage image = null;
            await DispatcherHelper.AwaitableRunAsync(dispatcher, async () =>
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

        private static async Task<WriteableBitmap> LoadImageFromFile(string Id, CoreDispatcher dispatcher)
        {
            try
            {
                var fileName = MiscHelpers.EscapeFilename(Id);
                StorageFolder pictureFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("AlbumArt", CreationCollisionOption.OpenIfExists);

                if (await pictureFolder.FileExistsAsync(fileName))
                {
                    var file = await pictureFolder.GetFileAsync(fileName);
                    var readStream = await file.OpenReadAsync();

                    WriteableBitmap image = null;
                    await DispatcherHelper.AwaitableRunAsync(dispatcher, async () => image = await BitmapFactory.FromStream(readStream));

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

        public async static Task<WriteableBitmap> ImageFromBytes(byte[] bytes, CoreDispatcher dispatcher)
        {
            WriteableBitmap image = null;
            using (var stream = new MemoryStream(bytes))
            {
                await DispatcherHelper.AwaitableRunAsync(dispatcher, async () =>
                {
                    image = await BitmapFactory.FromStream(stream);

                    // Resize overly large images to reduce OOM risk. Is 2048 too small ?
                    if (image.PixelWidth > 2048)
                    {
                        image = image.Resize(2048, 2048 * image.PixelHeight / image.PixelWidth, WriteableBitmapExtensions.Interpolation.Bilinear);
                    }
                });
            }
            return image;
        }

    }
}
