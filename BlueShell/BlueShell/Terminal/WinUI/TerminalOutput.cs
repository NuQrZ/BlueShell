using BlueShell.Terminal.Abstractions;
using Microsoft.UI;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using Windows.UI;

namespace BlueShell.Terminal.WinUI
{
    public sealed class TerminalOutput(
        RichEditBox terminal,
        Func<ElementTheme> themeProvider,
        Action<int> setInputStart,
        Action<string, TerminalMessageKind, string> onPrinted)
        : ITerminalOutput
    {
        private Color GetColor(TerminalMessageKind kind)
        {
            ElementTheme theme = themeProvider();

            if (theme == ElementTheme.Default)
            {
                theme = Application.Current.RequestedTheme == ApplicationTheme.Dark
                    ? ElementTheme.Dark
                    : ElementTheme.Light;
            }

            return theme switch
            {
                ElementTheme.Dark => kind switch
                {
                    TerminalMessageKind.PrintOutput => Color.FromArgb(255, 110, 190, 220),
                    TerminalMessageKind.Error => Color.FromArgb(255, 220, 80, 80),
                    TerminalMessageKind.Warning => Color.FromArgb(255, 210, 160, 90),
                    TerminalMessageKind.Success => Color.FromArgb(255, 80, 180, 130),
                    TerminalMessageKind.Info => Color.FromArgb(255, 160, 170, 185),
                    _ => Colors.White
                },

                ElementTheme.Light => kind switch
                {
                    TerminalMessageKind.PrintOutput => Color.FromArgb(255, 60, 120, 155),
                    TerminalMessageKind.Error => Color.FromArgb(255, 190, 55, 55),
                    TerminalMessageKind.Warning => Color.FromArgb(255, 170, 115, 40),
                    TerminalMessageKind.Success => Color.FromArgb(255, 45, 135, 90),
                    TerminalMessageKind.Info => Color.FromArgb(255, 90, 100, 115),
                    _ => Colors.Black
                },

                _ => Colors.Gray
            };
        }

        private void ResetTypingStyle()
        {
            RichEditTextDocument textDocument = terminal.Document;
            ElementTheme theme = themeProvider();

            textDocument.Selection.CharacterFormat.ForegroundColor =
                theme == ElementTheme.Light ? Colors.Black : Colors.White;
            textDocument.Selection.CharacterFormat.Bold = FormatEffect.Off;
        }

        private void AppendInternal(string text, TerminalMessageKind kind, string fontName, bool isRestoring)
        {
            RichEditTextDocument textDocument = terminal.Document;

            int end = textDocument.GetRange(0, int.MaxValue).EndPosition;
            ITextRange range = textDocument.GetRange(end, end);

            range.CharacterFormat.ForegroundColor = GetColor(kind);
            range.CharacterFormat.Bold =
                kind is TerminalMessageKind.Error or TerminalMessageKind.Success
                    ? FormatEffect.On
                    : FormatEffect.Off;
            range.CharacterFormat.Name = fontName;
            range.SetText(TextSetOptions.None, text);

            if (!isRestoring)
            {
                onPrinted(text, kind, fontName);
            }

            int newEnd = range.EndPosition;
            textDocument.Selection.SetRange(newEnd, newEnd);
            ResetTypingStyle();

            setInputStart(newEnd);
        }

        public void Print(string text, TerminalMessageKind kind = TerminalMessageKind.Output, string fontName = "Cascadia Code", bool isRestoring = false)
        {
            if (terminal.DispatcherQueue == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            if (terminal.DispatcherQueue.HasThreadAccess)
            {
                AppendInternal(text, kind, fontName, isRestoring);
                return;
            }

            terminal.DispatcherQueue.TryEnqueue(() =>
            {
                AppendInternal(text, kind, fontName, isRestoring);
            });
        }

        public void PrintLine(string text = "", TerminalMessageKind kind = TerminalMessageKind.Output, string fontName = "Cascadia Code", bool isRestoring = false)
        {
            if (terminal.DispatcherQueue == null)
            {
                return;
            }

            if (terminal.DispatcherQueue.HasThreadAccess)
            {
                AppendInternal(text + "\r\n", kind, fontName, isRestoring);
                return;
            }

            terminal.DispatcherQueue.TryEnqueue(() =>
            {
                AppendInternal(text + "\r\n", kind, fontName, isRestoring);
            });
        }

        public void SetTextWrap(bool wrap)
        {
            terminal.TextWrapping = wrap ? TextWrapping.Wrap : TextWrapping.NoWrap;
        }

        public void Clear()
        {
            if (terminal.DispatcherQueue.HasThreadAccess)
            {
                terminal.Document.SetText(TextSetOptions.None, "");
                int end = terminal.Document.GetRange(0, int.MaxValue).EndPosition;
                setInputStart(end);
                ResetTypingStyle();
                return;
            }

            terminal.DispatcherQueue.TryEnqueue(() =>
            {
                terminal.Document.SetText(TextSetOptions.None, "");
                int end = terminal.Document.GetRange(0, int.MaxValue).EndPosition;
                setInputStart(end);
                ResetTypingStyle();
            });
        }
    }
}
