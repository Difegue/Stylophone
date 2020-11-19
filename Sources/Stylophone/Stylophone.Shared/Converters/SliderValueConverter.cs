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
            return MiscHelpers.FormatTimeString(System.Convert.ToDouble(value) * 1000);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
