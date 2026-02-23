using BlueShell.Terminal.WinUI;
using Windows.UI.Text;

namespace BlueShell.Terminal.Abstractions
{
    public interface ITerminalOutput
    {
        LineBuilder Line();
        void Write(
            string text,
            TerminalMessageKind messageKind = TerminalMessageKind.Output,
            FontStyle fontStyle = FontStyle.Normal,
            FontWeight? fontWeight = null);
        void WriteLine(
            string text = "",
            TerminalMessageKind messageKind = TerminalMessageKind.Output,
            FontStyle fontStyle = FontStyle.Normal,
            FontWeight? fontWeight = null);
        void Clear();
    }
}
