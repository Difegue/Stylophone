﻿using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace FluentMPC.Helpers
{
    public class DoubleTopThicknessConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return new Thickness(0, (double)value, 0, 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return ((Thickness)value).Top;
        }
    }
}
