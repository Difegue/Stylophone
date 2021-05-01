using Stylophone.Localization.Strings;
using System;

using Windows.UI.Xaml.Data;

namespace FluentMPC.Helpers
{
    public class EnumToBooleanConverter : IValueConverter
    {
        public Type EnumType { get; set; }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (parameter is string enumString)
            {
                if (!Enum.IsDefined(EnumType, value))
                {
                    throw new ArgumentException(Resources.ExceptionEnumToBooleanConverterValueMustBeAnEnum);
                }

                var enumValue = Enum.Parse(EnumType, enumString);

                return enumValue.Equals(value);
            }

            throw new ArgumentException(Resources.ExceptionEnumToBooleanConverterParameterMustBeAnEnumName);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (parameter is string enumString)
            {
                return Enum.Parse(EnumType, enumString);
            }

            throw new ArgumentException(Resources.ExceptionEnumToBooleanConverterParameterMustBeAnEnumName);
        }
    }
}
