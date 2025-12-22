namespace BlueShell.Terminal.Abstractions
{
    public interface ITerminalOutput
    {
        void Print(string text, TerminalMessageKind kind = TerminalMessageKind.Output);
        void PrintLine(string text, TerminalMessageKind kind = TerminalMessageKind.Output);
        void Clear();
    }
}
