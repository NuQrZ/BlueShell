using System.Collections.Generic;
using Windows.UI.Text;

namespace BlueShell.Terminal.Abstractions
{
    public interface ITerminalOutput
    {
        void Write(string text = "",
                   TerminalMessageKind terminalMessageKind = TerminalMessageKind.Output,
                   FontWeight? fontWeight = null,
                   FontStyle fontStyle = FontStyle.Normal);
        void WriteLine(string text = "",
                       TerminalMessageKind terminalMessageKind = TerminalMessageKind.Output,
                       FontWeight? fontWeight = null,
                       FontStyle fontStyle = FontStyle.Normal);
        void WriteLines(IEnumerable<string> lines,
               TerminalMessageKind terminalMessageKind = TerminalMessageKind.Output,
               FontWeight? fontWeight = null,
               FontStyle fontStyle = FontStyle.Normal);
        void Clear();
    }
}
