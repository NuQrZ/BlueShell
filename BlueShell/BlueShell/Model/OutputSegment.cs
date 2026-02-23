using Microsoft.UI.Text;
using Microsoft.UI.Xaml.Media;
using Windows.UI.Text;

namespace BlueShell.Model
{
    public sealed class OutputSegment
    {
        public string Text { get; init; } = string.Empty;
        public SolidColorBrush? Color { get; init; }
        public FontWeight FontWeight { get; init; } = FontWeights.Normal;
        public FontStyle FontStyle { get; init; } = FontStyle.Normal;
    }
}