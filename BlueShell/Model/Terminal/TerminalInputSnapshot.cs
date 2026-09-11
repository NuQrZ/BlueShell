namespace BlueShell.Model.Terminal
{
    public sealed record TerminalInputSnapshot(
        string Text,
        int CaretPosition,
        int? SelectionAnchor);
}
