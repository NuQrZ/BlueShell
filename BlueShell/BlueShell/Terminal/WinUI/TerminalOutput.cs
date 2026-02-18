using BlueShell.Model;
using BlueShell.Terminal.Abstractions;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.ObjectModel;
using Windows.UI;

namespace BlueShell.Terminal.WinUI
{
    public sealed class TerminalOutput : ITerminalOutput
    {
        private readonly ObservableCollection<OutputLine> _lines = [];
        private readonly ItemsRepeater _terminalOutput;
        private readonly ScrollViewer _scrollViewer;

        private Func<ElementTheme> _themeProvider;

        private const int MaxLines = 100_000;

        public TerminalOutput(
            ItemsRepeater terminalOutput,
            ScrollViewer scrollViewer,
            Func<ElementTheme> themeProvider)
        {
            _lines = [];

            _terminalOutput = terminalOutput;
            _scrollViewer = scrollViewer;
            _themeProvider = themeProvider;

            _terminalOutput.ItemsSource = _lines;
        }

        public void Write(string text, TerminalMessageKind messageKind = TerminalMessageKind.Output)
        {
            _lines.Add(
                new OutputLine(
                    text,
                    new SolidColorBrush(
                        GetColor(messageKind))));
            if (_lines.Count > MaxLines)
            {
                _lines.RemoveAt(0);
            }

            AutoScroll();
        }

        public void WriteLine(string text = "", TerminalMessageKind messageKind = TerminalMessageKind.Output)
        {
            _lines.Add(
                new OutputLine(
                    text + "\n",
                    new SolidColorBrush(
                        GetColor(messageKind))));
            if (_lines.Count > MaxLines)
            {
                _lines.RemoveAt(0);
            }

            AutoScroll();
        }

        public void Clear()
        {
            _lines.Clear();
        }

        private void AutoScroll()
        {
            _scrollViewer.ChangeView(
                null,
                _scrollViewer.ScrollableHeight,
                null);
        }

        private Color GetColor(TerminalMessageKind kind)
        {
            ElementTheme theme = _themeProvider();

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
    }
}
