using BlueShell.Terminal.Abstractions;
using Microsoft.UI;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using Windows.UI;
using Windows.UI.Text;

namespace BlueShell.Terminal.Implementations
{
    public sealed class TerminalOutput(TerminalBuffer terminalBuffer, Func<ElementTheme> themeProvider) : ITerminalOutput
    {
        public void Write(string text, TerminalMessageKind terminalMessageKind, FontWeight? fontWeight = null, FontStyle fontStyle = FontStyle.Normal)
        {
            terminalBuffer.Write(text,
                                  GetColor(terminalMessageKind),
                                  fontWeight ?? FontWeights.Normal,
                                  fontStyle);
        }

        public void WriteLine(string text, TerminalMessageKind terminalMessageKind, FontWeight? fontWeight = null, FontStyle fontStyle = FontStyle.Normal)
        {
            terminalBuffer.WriteLine(text,
                                      GetColor(terminalMessageKind),
                                      fontWeight ?? FontWeights.Normal,
                                      fontStyle);
        }

        public void WriteLines(IEnumerable<string> lines, TerminalMessageKind terminalMessageKind = TerminalMessageKind.Output, FontWeight? fontWeight = null, FontStyle fontStyle = FontStyle.Normal)
        {
            terminalBuffer.WriteLines(lines,
                                      GetColor(terminalMessageKind),
                                      fontWeight ?? FontWeights.Normal,
                                      fontStyle);
        }

        public void Clear()
        {
            terminalBuffer.Clear();
        }

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
                    TerminalMessageKind.PrintOutput => Color.FromArgb(255, 188, 188, 188),
                    TerminalMessageKind.Error => Color.FromArgb(255, 255, 0, 0),
                    TerminalMessageKind.Warning => Color.FromArgb(255, 255, 175, 0),
                    TerminalMessageKind.Success => Color.FromArgb(255, 0, 255, 0),
                    TerminalMessageKind.Info => Color.FromArgb(255, 0, 175, 255),
                    _ => Colors.White
                },

                ElementTheme.Light => kind switch
                {
                    TerminalMessageKind.PrintOutput => Color.FromArgb(255, 68, 68, 68),
                    TerminalMessageKind.Error => Color.FromArgb(255, 215, 0, 0),
                    TerminalMessageKind.Warning => Color.FromArgb(255, 215, 95, 0),
                    TerminalMessageKind.Success => Color.FromArgb(255, 0, 135, 0),
                    TerminalMessageKind.Info => Color.FromArgb(255, 0, 95, 175),
                    _ => Colors.Black
                },
                _ => Colors.Gray
            };
        }

    }
}
