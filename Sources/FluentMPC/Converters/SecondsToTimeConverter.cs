using Stylophone.Common.Helpers;
using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace FluentMPC.Helpers
{
    /// <summary>
    ///     Convert an int value representing seconds into MM:SS format
    /// </summary>
    public class SecondsToTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var intVal = value as int?;

            if (intVal.HasValue)
                return Miscellaneous.FormatTimeString(intVal.Value * 1000);

            return "??:??";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
