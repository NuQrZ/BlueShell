using Windows.UI;
using Windows.UI.Text;

namespace BlueShell.Model
{
    public sealed class TerminalLineSegment(string text, Color? color, FontWeight fontWeight, FontStyle fontStyle)
    {
        public string Text { get; set; } = text;
        public Color? Color { get; set; } = color;
        public FontWeight FontWeight { get; set; } = fontWeight;
        public FontStyle FontStyle { get; set; } = fontStyle;
    }
}
