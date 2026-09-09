namespace BlueShell.Model.Terminal
{
    public sealed record TerminalInputState(
        string Text,
        int CaretPosition,
        int? SelectionAnchor);
}
