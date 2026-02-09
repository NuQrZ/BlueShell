using BlueShell.Terminal.Abstractions;

namespace BlueShell.View.Pages.States
{
    public sealed class TerminalLine
    {
        public string Text { get; set; } = "";
        public string FontName { get; set; } = "Cascadia Code";
        public TerminalMessageKind MessageKind { get; set; }
    }
}