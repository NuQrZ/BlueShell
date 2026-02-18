using Microsoft.UI.Xaml.Data;
using System;

namespace BlueShell.Converters
{
    internal partial class TextTrimmer : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is string text && parameter is string lengthStr && int.TryParse(lengthStr, out int maxLength))
            {
                if (text.Length > maxLength)
                {
                    return string.Concat(text.AsSpan(0, maxLength), "...");
                }
                return text;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}