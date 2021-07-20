using SkiaSharp;
using System;
using SkiaSharp.Views.UWP;
using Windows.UI.Xaml.Data;

namespace Stylophone.Helpers
{
    /// <summary>
    ///     Converts an SKImage to a WriteableBitmap for use in XAML.
    /// </summary>
    public class SKImageToUWPConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var image = value as SKImage;

            // Resize image if parameter specified
            if (parameter is string width && image != null)
            {
                try
                {
                    var w = int.Parse(width);
                    w = (int)(w * Windows.Graphics.Display.DisplayInformation.GetForCurrentView().RawPixelsPerViewPixel);
                    var bitmap = SKBitmap.FromImage(image);
                    image = SKImage.FromBitmap(bitmap.Resize(new SKImageInfo(w, w * bitmap.Height / bitmap.Width), SKFilterQuality.High));
                }
                catch { }
            }
                
            return image?.ToWriteableBitmap();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    ///     Converts an SKImage to a WriteableBitmap for use in XAML.
    /// </summary>
    public class SKColorToUWPConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var color = (SKColor)value;

            return color.ToColor();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
