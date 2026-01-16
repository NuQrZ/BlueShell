namespace BlueShell.Terminal.Abstractions
{
    public interface ITerminalOutput
    {
        void Print(string text, TerminalMessageKind kind = TerminalMessageKind.Output, string? fontName = "Consolas");
        void PrintLine(string text, TerminalMessageKind kind = TerminalMessageKind.Output, string? fontName = "Consolas");
        void SetTextWrap(bool wrap);
        void Clear();
    }
}
