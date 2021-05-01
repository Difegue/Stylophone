using SkiaSharp;
using Stylophone.Common.Helpers;
using System;
using SkiaSharp.Views.UWP;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace FluentMPC.Helpers
{
    /// <summary>
    ///     Converts an SKImage to a WriteableBitmap for use in XAML.
    /// </summary>
    public class SKImageToUWPConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var image = value as SKImage;

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
