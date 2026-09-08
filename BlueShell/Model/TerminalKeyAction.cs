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

        Delete,
        ControlDelete,

        SelectWordLeft,
        SelectWordRight,
        SelectCaretLeft,
        SelectCaretRight,
        SelectHome,
        SelectEnd,

        Copy,
        Paste,
        Cut,

        GoToHome,
        GoToEnd,

        PageUp,
        PageDown,

        Backspace,
        ControlBackspace,
        Submit
    }
}
