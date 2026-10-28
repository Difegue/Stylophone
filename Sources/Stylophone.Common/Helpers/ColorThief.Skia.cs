
using ColorThiefDotNet;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Stylophone.Common.Helpers
{
    public class ColorThief
    {
        public const int DefaultColorCount = 5;
        public const int DefaultQuality = 10;
        public const bool DefaultIgnoreWhite = true;
        public const int ColorDepth = 4;

        /// <summary>
        ///     Use the median cut algorithm to cluster similar colors.
        /// </summary>
        /// <param name="pixelArray">Pixel array.</param>
        /// <param name="colorCount">The color count.</param>
        /// <returns></returns>
        private CMap GetColorMap(byte[][] pixelArray, int colorCount)
        {
            // Send array to quantize function which clusters values using median
            // cut algorithm

            if (colorCount > 0)
            {
                --colorCount;
            }

            var cmap = Mmcq.Quantize(pixelArray, colorCount);
            return cmap;
        }

        private byte[][] ConvertPixels(byte[] pixels, int pixelCount, int quality, bool ignoreWhite)
        {
            var expectedDataLength = pixelCount * ColorDepth;
            if (expectedDataLength != pixels.Length)
            {
                throw new ArgumentException("(expectedDataLength = "
                                            + expectedDataLength + ") != (pixels.length = "
                                            + pixels.Length + ")");
            }

            // Store the RGB values in an array format suitable for quantize
            // function

            // numRegardedPixels must be rounded up to avoid an
            // ArrayIndexOutOfBoundsException if all pixels are good.

            var numRegardedPixels = (pixelCount + quality - 1) / quality;

            var numUsedPixels = 0;
            var pixelArray = new byte[numRegardedPixels][];

            for (var i = 0; i < pixelCount; i += quality)
            {
                var offset = i * ColorDepth;
                var b = pixels[offset];
                var g = pixels[offset + 1];
                var r = pixels[offset + 2];
                var a = pixels[offset + 3];

                // If pixel is mostly opaque and not white
                if (a >= 125 && !(ignoreWhite && r > 250 && g > 250 && b > 250))
                {
                    pixelArray[numUsedPixels] = new[] { r, g, b };
                    numUsedPixels++;
                }
            }

            // Remove unused pixels from the array
            var copy = new byte[numUsedPixels][];
            Array.Copy(pixelArray, copy, numUsedPixels);
            return copy;
        }

        /// <summary>
        ///     Use the median cut algorithm to cluster similar colors and return the base color from the largest cluster.
        /// </summary>
        /// <param name="sourceImage">The source image.</param>
        /// <param name="quality">
        ///     1 is the highest quality settings. 10 is the default. There is
        ///     a trade-off between quality and speed. The bigger the number,
        ///     the faster a color will be returned but the greater the
        ///     likelihood that it will not be the visually most dominant color.
        /// </param>
        /// <param name="ignoreWhite">if set to <c>true</c> [ignore white].</param>
        /// <returns></returns>
        public QuantizedColor GetColor(SKBitmap sourceImage, int quality = DefaultQuality, bool ignoreWhite = DefaultIgnoreWhite)
        {
            var palette = GetPalette(sourceImage, 3, quality, ignoreWhite);

            // Handle case where GetPalette returns an empty list (because GetColorMap failed?)
            if (palette.Count == 0)
            {
                return new QuantizedColor(SKColors.Black, 1);
            }

            var avgR = Convert.ToByte(palette.Average(a => a.Color.Red));
            var avgG = Convert.ToByte(palette.Average(a => a.Color.Green));
            var avgB = Convert.ToByte(palette.Average(a => a.Color.Blue));
            var avgA = Convert.ToByte(palette.Average(a => a.Color.Alpha));
            var avgColor = new SKColor(avgR, avgG, avgB, avgA);

            var dominantColor = new QuantizedColor(avgColor, Convert.ToInt32(palette.Average(a => a.Population)));

            return dominantColor;
        }

        /// <summary>
        ///     Pick the most vivid color of the image, and make it saturated and bright enough to work as an accent.
        ///     Unlike <see cref="GetColor"/>, swatches aren't averaged together, so the color doesn't get muddied.
        /// </summary>
        /// <param name="sourceImage">The source image.</param>
        /// <param name="colorCount">How many swatches to choose from.</param>
        /// <param name="quality">See <see cref="GetColor"/>.</param>
        /// <param name="ignoreWhite">if set to <c>true</c> [ignore white].</param>
        /// <returns></returns>
        public QuantizedColor GetVibrantColor(SKBitmap sourceImage, int colorCount = 8, int quality = DefaultQuality, bool ignoreWhite = DefaultIgnoreWhite)
        {
            var palette = GetPalette(sourceImage, colorCount, quality, ignoreWhite);

            if (palette.Count == 0)
            {
                return new QuantizedColor(SKColors.Gray, 1);
            }
            float Saturation(QuantizedColor c) { c.Color.ToHsl(out _, out var s, out _); return s / 100; }
            float Lightness(QuantizedColor c) { c.Color.ToHsl(out _, out _, out var l); return l / 100; }

            // Like Android's Palette, only consider colorful swatches whose lightness doesn't wash out their hue
            var candidates = palette.Where(c => Saturation(c) >= 0.25 && Lightness(c) is >= 0.15f and <= 0.85f).ToList();
            if (candidates.Count == 0)
            {
                candidates = palette;
            }

            var highestPopulation = candidates.Max(c => c.Population);
            var vibrant = candidates.OrderByDescending(c =>
                Mmcq.CreateComparisonValue(Saturation(c), 1, Lightness(c), 0.5, c.Population, highestPopulation)).First();

            vibrant.Color.ToHsl(out var hue, out var saturation, out var lightness);

            // Leave grays alone, as boosting their saturation would make up a hue
            var vibrantColor = saturation < 10 ?
                SKColor.FromHsl(hue, saturation, Math.Min(Math.Max(lightness, 45), 65)) :
                SKColor.FromHsl(hue, Math.Max(saturation, 60), Math.Min(Math.Max(lightness, 45), 60));

            return new QuantizedColor(vibrantColor, vibrant.Population);
        }

        /// <summary>
        ///     Use the median cut algorithm to cluster similar colors.
        /// </summary>
        /// <param name="sourceImage">The source image.</param>
        /// <param name="colorCount">The color count.</param>
        /// <param name="quality">
        ///     1 is the highest quality settings. 10 is the default. There is
        ///     a trade-off between quality and speed. The bigger the number,
        ///     the faster a color will be returned but the greater the
        ///     likelihood that it will not be the visually most dominant color.
        /// </param>
        /// <param name="ignoreWhite">if set to <c>true</c> [ignore white].</param>
        /// <returns></returns>
        /// <code>true</code>
        public List<QuantizedColor> GetPalette(SKBitmap sourceImage, int colorCount = DefaultColorCount, int quality = DefaultQuality, bool ignoreWhite = DefaultIgnoreWhite)
        {
            var pixelArray = GetPixelsFast(sourceImage, quality, ignoreWhite);
            var cmap = GetColorMap(pixelArray, colorCount);
            if (cmap != null)
            {
                var colors = cmap.GeneratePalette();
                return colors;
            }
            return new List<QuantizedColor>();
        }

        private byte[][] GetPixelsFast(SKBitmap sourceImage, int quality, bool ignoreWhite)
        {
            if (quality < 1)
            {
                quality = DefaultQuality;
            }

            var pixels = GetIntFromPixel(sourceImage);
            var pixelCount = sourceImage.Width * sourceImage.Height;

            return ConvertPixels(pixels, pixelCount, quality, ignoreWhite);
        }

        private byte[] GetIntFromPixel(SKBitmap bmp)
        {
            var pixelList = new byte[bmp.Width * bmp.Height * 4];
            int count = 0;

            for (var x = 0; x < bmp.Width; x++)
            {
                for (var y = 0; y < bmp.Height; y++)
                {
                    var clr = bmp.GetPixel(x, y);

                    pixelList[count] = clr.Blue;
                    count++;

                    pixelList[count] = clr.Green;
                    count++;

                    pixelList[count] = clr.Red;
                    count++;

                    pixelList[count] = clr.Alpha;
                    count++;
                }
            }

            return pixelList;
        }
    }


}

