using ColorThiefDotNet;
using MpcNET.Commands.Database;
using MpcNET.Types;
using SkiaSharp;
using Stylophone.Common.Helpers;
using Stylophone.Common.Interfaces;
using Stylophone.Common.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Stylophone.Common.Services
{
    public class AlbumArt
    {
        public SKImage ArtBitmap { get; set; }
        public QuantizedColor DominantColor { get; set; }
    }

    public class AlbumArtService
    {
        private Stack<AlbumViewModel> _albumArtQueue;
        private CancellationTokenSource _queueCanceller;

        private IApplicationStorageService _applicationStorageService;
        private MPDConnectionService _mpdService;

        public AlbumArtService(MPDConnectionService mpdService, IApplicationStorageService appStorage)
        {
            _mpdService = mpdService;
            _applicationStorageService = appStorage;
        }

        public void Initialize()
        {
            _queueCanceller?.Cancel();
            _queueCanceller = new CancellationTokenSource();

            var token = _queueCanceller.Token;

            _albumArtQueue = new Stack<AlbumViewModel>();

            // Run an idle loop in a spare thread to process album art sequentially on a LIFO basis
            Task.Run(async () =>
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
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Check if this file's album art is already stored in the internal Album Art cache.
        /// </summary>
        /// <param name="f">MpdFile to check for art</param>
        /// <returns>True if the art is cached, false otherwise.</returns>
        public async Task<bool> IsAlbumArtCachedAsync(IMpdFile f) => await _applicationStorageService.DoesFileExistAsync(GetFileIdentifier(f), "AlbumArt");

        /// <summary>
        /// Queue up an AlbumViewModel for the service to grab its album art.
        /// This method is on a LIFO basis, but if the service is already busy on downloading large art it won't reply straight away!
        /// Consider checking beforehand if the art is cached, and if it is, to use GetAlbumArtAsync directly instead of this.
        /// </summary>
        /// <param name="vm"></param>
        public void QueueAlbumArt(AlbumViewModel vm) => _albumArtQueue.Push(vm);

        /// <summary>
        /// Get the Album Art for the given MpdFile. If desired, this function can also calculate the dominant color of the art.
        /// </summary>
        /// <param name="f">The MpdFile</param>
        /// <param name="calculateDominantColor">Whether to calculate dominant color through ColorThief</param>
        /// <param name="albumArtWidth">Width of the final SKImage</param>
        /// <returns>An AlbumArt object containing bitmap and color. Returns null if there was no albumart on the MPD server.</returns>
        public async Task<AlbumArt> GetAlbumArtAsync(IMpdFile f, bool calculateDominantColor, int albumArtWidth, CancellationToken token = default)
        {
            var result = new AlbumArt();

            var bitmap = await GetAlbumBitmap(f, token);
            if (bitmap != null)
            {
                bitmap = bitmap.Resize(new SKImageInfo(albumArtWidth, albumArtWidth * bitmap.Height / bitmap.Width), SKFilterQuality.High);
                result.ArtBitmap = SKImage.FromBitmap(bitmap);

                if (calculateDominantColor)
                    result.DominantColor = GetDominantColor(bitmap);

                return result;
            }
            else
                return null;
        }

        private string GetFileIdentifier(IMpdFile f)
        {
            // This allows to cache per album, avoiding saving the same album art a ton of times.
            // Doesn't work if files in an album have different albumarts, but that happens so rarely it's fine to ignore it.
            var uniqueIdentifier = f.HasAlbum ? f.Album : f.HasTitle ? f.Title : f.Path;

            return Miscellaneous.EscapeFilename(uniqueIdentifier);
        }

        private async Task<SKBitmap> GetAlbumBitmap(IMpdFile f, CancellationToken token = default)
        {
            SKBitmap result;
            var foundUsableArt = false;
            var fileName = GetFileIdentifier(f);

            // Try loading from art cache first
            if (await IsAlbumArtCachedAsync(f))
                return await LoadImageFromFile(fileName);

            // Get albumart from MPD
            List<byte> data = new List<byte>();
            try
            {
                using (var c = await _mpdService.GetConnectionAsync(token))
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

                    // Create the SKImage.
                    if (foundUsableArt)
                        result = ImageFromBytes(data.ToArray());
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

        private QuantizedColor GetDominantColor(SKBitmap art)
        {
            // Get dominant color of albumart
            var smallArt = art.Resize(new SKImageInfo(50, 50), SKFilterQuality.Low);

            var colorThief = new ColorThief();
            var quantizedColor = colorThief.GetColor(smallArt);

            return quantizedColor;
        }



        private async Task SaveArtToFileAsync(string fileName, List<byte> data) => await _applicationStorageService.SaveDataToFileAsync(fileName, data.ToArray(), "AlbumArt");

        private async Task<SKBitmap> LoadImageFromFile(string fileName)
        {
            try
            {
                var fileStream = await _applicationStorageService.OpenFileAsync(fileName, "AlbumArt");
                SKBitmap image = SKBitmap.Decode(fileStream);
                fileStream.Dispose();
                return image;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private SKBitmap ImageFromBytes(byte[] bytes)
        {
            SKBitmap image = SKBitmap.Decode(bytes);

            // Resize overly large images to reduce OOM risk. Is 2048 too small ?
            if (image.Width > 2048)
            {
                image.Resize(new SKImageInfo(2048, 2048 * image.Height / image.Width), SKFilterQuality.High);
            }
            return image;
        }

    }
}
