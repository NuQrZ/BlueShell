namespace BlueShell.Terminal.Abstractions
{
    public interface ITerminalOutput
    {
        void Write(
            string text,
            TerminalMessageKind messageKind = TerminalMessageKind.Output);
        void WriteLine(
            string text = "",
            TerminalMessageKind messageKind = TerminalMessageKind.Output);
        void Clear();
    }
}
