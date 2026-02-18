using Microsoft.UI.Xaml.Media;

namespace BlueShell.Model
{
    public sealed class OutputLine(string text, SolidColorBrush solidColorBrush)
    {
        public string Text { get; set; } = text;
        public SolidColorBrush SolidColorBrush { get; set; } = solidColorBrush;
    }
}
