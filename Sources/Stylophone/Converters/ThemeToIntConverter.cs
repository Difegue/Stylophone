using Stylophone.Common.Interfaces;
using Stylophone.Localization.Strings;
using System;

using Windows.UI.Xaml.Data;

namespace Stylophone.Helpers
{
    public class ThemeToIntConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            
            if (value is Theme t)
                return (int)t;
            
            throw new ArgumentException("Not a Theme");
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is int enumInt)
            {
                // Cast enumInt to EnumType
                return Enum.ToObject(typeof(Theme), enumInt);
            }

            throw new ArgumentException("Not an int");
        }
    }
}
