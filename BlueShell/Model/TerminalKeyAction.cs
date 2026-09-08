namespace BlueShell.Model
{
    public enum TerminalKeyAction
    {
        None,
        BlockInput,

        ZoomIn,
        ZoomOut,
        Cancel,

        MoveCaretLeft,
        MoveCaretRight,
        MoveCaretWordLeft,
        MoveCaretWordRight,

        SelectWordLeft,
        SelectWordRight,
        SelectCaretLeft,
        SelectCaretRight,
        SelectHome,
        SelectEnd,

        GoToHome,
        GoToEnd,

        PageUp,
        PageDown,

        Backspace,
        ControlBackspace,
        Submit
    }
}
