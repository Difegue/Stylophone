using Stylophone.Common.Helpers;
using System;
using Windows.UI.Xaml.Data;

namespace Stylophone.Helpers
{
    /// <summary>
    ///     Used for now playing slider, shows human readable time
    /// </summary>
    public class SliderValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return Miscellaneous.FormatTimeString(System.Convert.ToDouble(value));
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
