namespace BlueShell.Terminal.Abstractions
{
    public interface ITerminalOutput
    {
        void Print(string text, TerminalMessageKind kind = TerminalMessageKind.Output, string fontName = "Cascadia Code", bool isRestoring = false);
        void PrintLine(string text = "", TerminalMessageKind kind = TerminalMessageKind.Output, string fontName = "Cascadia Code", bool isRestoring = false);
        void SetTextWrap(bool wrap);
        void Clear();
    }
}