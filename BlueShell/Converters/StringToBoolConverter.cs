using Microsoft.UI.Xaml.Data;
using System;

namespace BlueShell.Converters
{
    public partial class StringToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value?.ToString() == parameter?.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value is true;
        }
    }
}
