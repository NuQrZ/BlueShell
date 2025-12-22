using BlueShell.Terminal.Abstractions;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using Windows.UI;
using Microsoft.UI.Text;

namespace BlueShell.Terminal.WinUI
{
    public sealed class TerminalOutput(
        RichEditBox terminal,
        Func<ElementTheme> themeProvider,
        Action<int> setInputStart)
        : ITerminalOutput
    {
        private Color GetColor(TerminalMessageKind kind)
        {
            ElementTheme theme = themeProvider();
            return kind switch
            {
                TerminalMessageKind.Error => Color.FromArgb(255, 255, 90, 90),
                TerminalMessageKind.Warning => Color.FromArgb(255, 255, 193, 7),
                TerminalMessageKind.Success => Color.FromArgb(255, 90, 200, 120),
                TerminalMessageKind.Info => Color.FromArgb(255, 100, 160, 255),
                _ => theme == ElementTheme.Light ? Colors.Black : Colors.White
            };
        }

        private void ResetTypingStyle()
        {
            Microsoft.UI.Text.RichEditTextDocument textDocument = terminal.Document;
            ElementTheme theme = themeProvider();

            textDocument.Selection.CharacterFormat.ForegroundColor =
                theme == ElementTheme.Light ? Colors.Black : Colors.White;
            textDocument.Selection.CharacterFormat.Bold = FormatEffect.Off;
        }

        private void AppendInternal(string text, TerminalMessageKind kind)
        {
            Microsoft.UI.Text.RichEditTextDocument textDocument = terminal.Document;

            int end = textDocument.GetRange(0, int.MaxValue).EndPosition;
            Microsoft.UI.Text.ITextRange range = textDocument.GetRange(end, end);

            range.CharacterFormat.ForegroundColor = GetColor(kind);
            range.CharacterFormat.Bold =
                kind is TerminalMessageKind.Error or TerminalMessageKind.Success
                    ? FormatEffect.On
                    : FormatEffect.Off;

            range.SetText(TextSetOptions.None, text);

            int newEnd = range.EndPosition;
            textDocument.Selection.SetRange(newEnd, newEnd);
            ResetTypingStyle();

            setInputStart(newEnd);
        }

        public void Print(string text, TerminalMessageKind kind = TerminalMessageKind.Output)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            terminal.DispatcherQueue.TryEnqueue(() =>
            {
                AppendInternal(text, kind);
            });
        }

        public void PrintLine(string text, TerminalMessageKind kind = TerminalMessageKind.Output)
        {
            terminal.DispatcherQueue.TryEnqueue(() =>
            {
                AppendInternal(text + "\r\n", kind);
            });
        }

        public void Clear()
        {
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
