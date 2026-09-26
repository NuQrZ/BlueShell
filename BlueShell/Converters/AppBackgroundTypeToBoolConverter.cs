using BlueShell.Model.Settings;
using Microsoft.UI.Xaml.Data;
using System;

namespace BlueShell.Converters
{
    public sealed class AppBackgroundTypeToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is not AppBackgroundType backgroundType || parameter is not string targetTypeString)
            {
                return false;
            }

            return Enum.TryParse(
                       targetTypeString,
                       out AppBackgroundType targetTypeValue)
                   && backgroundType == targetTypeValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotSupportedException();
        }
    }
}