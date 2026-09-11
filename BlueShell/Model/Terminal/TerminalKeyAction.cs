namespace BlueShell.Model.Terminal
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
        SelectAll,
        ClearSelection,

        Copy,
        Paste,
        Cut,

        Undo,
        Redo,

        GoToHome,
        GoToEnd,

        PageUp,
        PageDown,

        HistoryPrevious,
        HistoryNext,

        Backspace,
        ControlBackspace,
        Submit
    }
}
