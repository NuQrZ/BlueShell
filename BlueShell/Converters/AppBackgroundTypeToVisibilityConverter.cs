using BlueShell.Model.Settings;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace BlueShell.Converters
{
    public sealed partial class AppBackgroundTypeToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is not AppBackgroundType backgroundType || parameter is not string parameterString || !Enum.TryParse(
                    parameterString,
                    out AppBackgroundType expectedBackgroundType))
                return Visibility.Collapsed;

            return backgroundType == expectedBackgroundType
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotSupportedException();
        }
    }
}