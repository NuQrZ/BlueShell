using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using System;

namespace BlueShell.Converters
{
    public sealed partial class FilePathToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string? filePath = value as string;

            filePath ??= "ms-appx:///Assets/Icons/Terminal.ico";

            return new BitmapIconSource()
            {
                UriSource = new Uri(filePath),
                ShowAsMonochrome = false
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
