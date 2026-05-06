// Vendored from https://github.com/mono/SkiaSharp v2.88.8 — MIT licensed.
// Trimmed to the two extension methods Stylophone actually consumes:
//   - SKColor.ToColor()
//   - SKImage.ToWriteableBitmap()
// The full upstream file also covers Point/Rect/Size conversions and
// SKPicture/SKBitmap/SKPixmap variants — port them back from
// https://github.com/mono/SkiaSharp/blob/v2.88.8/source/SkiaSharp.Views/SkiaSharp.Views.UWP/UWPExtensions.cs
// if a future caller needs them.

using Windows.UI;
using Windows.UI.Xaml.Media.Imaging;

namespace SkiaSharp.Views.UWP
{
    public static class UWPExtensions
    {
        // Color

        public static SKColor ToSKColor(this Color color)
        {
            return new SKColor(color.R, color.G, color.B, color.A);
        }

        public static Color ToColor(this SKColor color)
        {
            return Color.FromArgb(color.Alpha, color.Red, color.Green, color.Blue);
        }

        // WriteableBitmap

        public static WriteableBitmap ToWriteableBitmap(this SKImage skiaImage)
        {
            var info = new SKImageInfo(skiaImage.Width, skiaImage.Height);
            var bitmap = new WriteableBitmap(info.Width, info.Height);
            using (var pixmap = new SKPixmap(info, bitmap.PixelBuffer.GetByteBuffer()))
            {
                skiaImage.ReadPixels(pixmap, 0, 0);
            }
            bitmap.Invalidate();
            return bitmap;
        }
    }
}
