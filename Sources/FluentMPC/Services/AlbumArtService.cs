using ColorThiefDotNet;
using FluentMPC.Helpers;
using FluentMPC.ViewModels.Items;
using Microsoft.Toolkit.Uwp;
using Microsoft.Toolkit.Uwp.Helpers;
using MpcNET.Commands.Database;
using MpcNET.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml.Media.Imaging;

namespace FluentMPC.Services
{
    public class AlbumArt
    {
        public BitmapImage ArtBitmap { get; set; }
        public QuantizedColor DominantColor { get; set; }
    }

    public static class AlbumArtService
    {
        private static Stack<AlbumViewModel> _albumArtQueue;
        private static CancellationTokenSource _queueCanceller;

        public static void Initialize()
        {
            _queueCanceller?.Cancel();
            _queueCanceller = new CancellationTokenSource();

            var token = _queueCanceller.Token;

            _albumArtQueue = new Stack<AlbumViewModel>();

            // Run an idle loop in a spare thread to process album art sequentially on a LIFO basis
            _ = Task.Run(async () =>
            {
                while (true)
                {
                    if (token.IsCancellationRequested)
                        break;

                    if (_albumArtQueue.Count == 0)
                    {
                        Thread.Sleep(600);
                        continue;
                    }

                    var vm = _albumArtQueue.Pop();

                    try
                    {
                        if (vm.AlbumArtLoaded)
                            continue;

                        if (vm.Files.Count > 0)
                        {
                            var art = await GetAlbumArtAsync(vm.Files[0], true, 180);
                            vm.SetAlbumArt(art);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine("Exception while processing albumart queue: " + e);
                    }
                }
            });
        }

        /// <summary>
        /// Check if this file's album art is already stored in the internal Album Art cache.
        /// </summary>
        /// <param name="f">MpdFile to check for art</param>
        /// <returns>True if the art is cached, false otherwise.</returns>
        public static async Task<bool> IsAlbumArtCachedAsync(IMpdFile f)
        {
            StorageFolder pictureFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("AlbumArt", CreationCollisionOption.OpenIfExists);
            return (await pictureFolder.FileExistsAsync(GetFileIdentifier(f)));
        }

        /// <summary>
        /// Queue up an AlbumViewModel for the service to grab its album art.
        /// This method is on a LIFO basis, but if the service is already busy on downloading large art it won't reply straight away!
        /// Consider checking beforehand if the art is cached, and if it is, to use GetAlbumArtAsync directly instead of this.
        /// </summary>
        /// <param name="vm"></param>
        public static void QueueAlbumArt(AlbumViewModel vm) => _albumArtQueue.Push(vm);

        /// <summary>
        /// Get the Album Art for the given MpdFile. If desired, this function can also calculate the dominant color of the art.
        /// </summary>
        /// <param name="f">The MpdFile</param>
        /// <param name="calculateDominantColor">Whether to calculate dominant color through ColorThief</param>
        /// <param name="albumArtWidth">Width of the final BitmapImage</param>
        /// <param name="dispatcher">Dispatcher to use for the UI-bound options. Defaults to MainWindow.CoreWindow.Dispatcher.</param>
        /// <returns>An AlbumArt object containing bitmap and color. Returns null if there was no albumart on the MPD server.</returns>
        public async static Task<AlbumArt> GetAlbumArtAsync(IMpdFile f, bool calculateDominantColor, int albumArtWidth, DispatcherQueue dispatcherQueue = null, CancellationToken token = default)
        {
            if (dispatcherQueue == null)
                dispatcherQueue = DispatcherService.DispatcherQueue;

            var result = new AlbumArt();

            var bitmap = await GetAlbumBitmap(f, dispatcherQueue, token);
            if (bitmap != null)
            {
                result.ArtBitmap = await WriteableBitmapToBitmapImageAsync(bitmap, albumArtWidth, dispatcherQueue);

                if (calculateDominantColor)
                {
                    result.DominantColor = await GetDominantColor(bitmap, dispatcherQueue);
                }

                return result;
            }
            else
                return null;
        }

        private static string GetFileIdentifier(IMpdFile f)
        {
            // This allows to cache per album, avoiding saving the same album art a ton of times.
            // Doesn't work if files in an album have different albumarts, but that happens so rarely it's fine to ignore it.
            var uniqueIdentifier = f.HasAlbum ? f.Album : f.HasTitle ? f.Title : f.Path;

            return MiscHelpers.EscapeFilename(uniqueIdentifier);
        }

        private async static Task<WriteableBitmap> GetAlbumBitmap(IMpdFile f, DispatcherQueue dispatcher, CancellationToken token = default)
        {
            WriteableBitmap result;
            var foundUsableArt = false;
            var fileName = GetFileIdentifier(f);

            // Try loading from art cache first
            if (await IsAlbumArtCachedAsync(f))
                return await LoadImageFromFile(fileName, dispatcher);

            // Get albumart from MPD
            List<byte> data = new List<byte>();
            try
            {
                using (var c = await MPDConnectionService.GetConnectionAsync(token))
                {
                    if (c == null) // We got cancelled
                        return null;

                    long totalBinarySize = 9999;
                    long currentSize = 0;

                    do
                    {
                        var albumReq = await c.InternalResource.SendAsync(new AlbumArtCommand(f.Path, currentSize));
                        if (!albumReq.IsResponseValid) break;

                        var response = albumReq.Response.Content;
                        if (response.Binary == 0) break; // MPD isn't giving us any more data, let's roll with what we have.

                        totalBinarySize = response.Size;
                        currentSize += response.Binary;
                        data.AddRange(response.Data);
                        foundUsableArt = true;
                        Debug.WriteLine($"Downloading albumart: {currentSize}/{totalBinarySize}");
                    } while (currentSize < totalBinarySize && !token.IsCancellationRequested);

                    // Fallback to readpicture if albumart didn't work
                    if (!foundUsableArt) do
                        {
                            var albumReq = await c.InternalResource.SendAsync(new ReadPictureCommand(f.Path, currentSize));
                            if (!albumReq.IsResponseValid) break;

                            var response = albumReq.Response.Content;
                            if (response == null || response.Binary == 0) break; // MPD isn't giving us any more data, let's roll with what we have.

                            totalBinarySize = response.Size;
                            currentSize += response.Binary;
                            data.AddRange(response.Data);
                            foundUsableArt = true;
                            Debug.WriteLine($"Downloading albumart: {currentSize}/{totalBinarySize}");
                        } while (currentSize < totalBinarySize && !token.IsCancellationRequested);

                    if (token.IsCancellationRequested)
                        return null;

                    // Create the BitmapImage on the UI Thread.
                    if (foundUsableArt)
                        result = await ImageFromBytes(data.ToArray(), dispatcher);
                    else
                        return null;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception caught while getting albumart: " + e);
                return null;
            }

            if (foundUsableArt)
                await SaveArtToFileAsync(fileName, data);

            return result;
        }

        private static async Task<QuantizedColor> GetDominantColor(WriteableBitmap art, DispatcherQueue dispatcherQueue)
        {
            // Get dominant color of albumart
            using (InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream())
            {
                await dispatcherQueue.EnqueueAsync(async () =>
                {
                    await art.Resize(50, 50, WriteableBitmapExtensions.Interpolation.NearestNeighbor).ToStreamAsJpeg(stream);
                });
                stream.Seek(0);

                var colorThief = new ColorThief();
                var quantizedColor = await colorThief.GetColor(await BitmapDecoder.CreateAsync(stream));

                return quantizedColor;
            }
        }

        private static async Task SaveArtToFileAsync(string fileName, List<byte> data)
        {
            StorageFolder pictureFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("AlbumArt", CreationCollisionOption.OpenIfExists);
            await pictureFolder.SaveFileAsync(data.ToArray(), fileName, CreationCollisionOption.ReplaceExisting);
        }

        private static async Task<BitmapImage> WriteableBitmapToBitmapImageAsync(WriteableBitmap art, int decodedPixelWidth, DispatcherQueue dispatcherQueue)
        {
            BitmapImage image = null;
            await dispatcherQueue.EnqueueAsync(async () =>
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

        private static async Task<WriteableBitmap> LoadImageFromFile(string fileName, DispatcherQueue dispatcherQueue)
        {
            try
            {
                StorageFolder pictureFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("AlbumArt", CreationCollisionOption.OpenIfExists);

                var file = await pictureFolder.GetFileAsync(fileName);
                var readStream = await file.OpenReadAsync();

                WriteableBitmap image = null;
                await dispatcherQueue.EnqueueAsync(async () => image = await BitmapFactory.FromStream(readStream));

                readStream.Dispose();
                return image;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private async static Task<WriteableBitmap> ImageFromBytes(byte[] bytes, DispatcherQueue dispatcherQueue)
        {
            WriteableBitmap image = null;
            using (var stream = new MemoryStream(bytes))
            {
                await dispatcherQueue.EnqueueAsync(async () =>
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
