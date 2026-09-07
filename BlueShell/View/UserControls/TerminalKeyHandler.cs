using BlueShell.Model;
using Microsoft.UI.Input;
using Windows.System;

namespace BlueShell.View.UserControls
{
    public static class TerminalKeyHandler
    {
        private const VirtualKey OemPlus = (VirtualKey)0xBB;
        private const VirtualKey OemMinus = (VirtualKey)0xBD;

        public static bool IsKeyDown(VirtualKey key)
        {
            return (InputKeyboardSource.GetKeyStateForCurrentThread(key) &
                    Windows.UI.Core.CoreVirtualKeyStates.Down) != 0;
        }

        public static TerminalKeyAction HandleKey(VirtualKey originalKey, VirtualKey key, bool isCommandRunning)
        {
            bool isCtrlPressed = IsKeyDown(VirtualKey.Control);
            bool isShiftPressed = IsKeyDown(VirtualKey.Shift);

            if (isCtrlPressed)
            {
                if (originalKey == OemPlus || key == VirtualKey.Add)
                {
                    return TerminalKeyAction.ZoomIn;
                }
                if (originalKey == OemMinus || key == VirtualKey.Subtract)
                {
                    return TerminalKeyAction.ZoomOut;
                }
                if (key == VirtualKey.Q)
                {
                    return TerminalKeyAction.Cancel;
                }
            }

            if (key == VirtualKey.PageUp)
            {
                return TerminalKeyAction.PageUp;
            }
            if (key == VirtualKey.PageDown)
            {
                return TerminalKeyAction.PageDown;
            }

            if (isCommandRunning)
            {
                return TerminalKeyAction.BlockInput;
            }

            if (isCtrlPressed)
            {
                if (key == VirtualKey.Left)
                {
                    return TerminalKeyAction.MoveCaretWordLeft;
                }
                if (key == VirtualKey.Right)
                {
                    return TerminalKeyAction.MoveCaretWordRight;
                }
                if (key == VirtualKey.Back)
                {
                    return TerminalKeyAction.ControlBackspace;
                }
            }

            if (isShiftPressed)
            {
                if (key == VirtualKey.Left)
                {
                    return TerminalKeyAction.SelectWordLeft;
                }
                if (key == VirtualKey.Right)
                {
                    return TerminalKeyAction.SelectWordRight;
                }
                if (key == VirtualKey.Home)
                {
                    return TerminalKeyAction.SelectHome;
                }
                if (key == VirtualKey.End)
                {
                    return TerminalKeyAction.SelectEnd;
                }
            }

            return key switch
            {
                VirtualKey.Left => TerminalKeyAction.MoveCaretLeft,
                VirtualKey.Right => TerminalKeyAction.MoveCaretRight,
                VirtualKey.Home => TerminalKeyAction.GoToHome,
                VirtualKey.End => TerminalKeyAction.GoToEnd,
                VirtualKey.Enter => TerminalKeyAction.Submit,
                VirtualKey.Back => TerminalKeyAction.Backspace,
                _ => TerminalKeyAction.None
            };
        }
    }
}
