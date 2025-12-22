using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI;
using BlueShell.Core;
using BlueShell.Terminal.Abstractions;
using BlueShell.Terminal.Infrastructure;
using BlueShell.Terminal.WinUI;
using BlueShell.ViewModel;
using Microsoft.UI.Text;

namespace BlueShell.View.Pages
{
    public sealed partial class TerminalPage
    {
        private const string Prompt = "Terminal > ";

        private int _inputStart;
        private int _enterCount;

        private bool _suppressHighlight;
        private DispatcherTimer? _highLightTimer;

        private TerminalViewModel? _terminalViewModel;
        private TerminalOutput? _terminalOutput;

        public TerminalPage()
        {
            InitializeComponent();

            Loaded += (_, _) =>
            {
                IDataDisplay dataDisplay = new DataDisplay(DataDisplay);
                _terminalOutput = new TerminalOutput(
                    Terminal,
                    () => ActualTheme,
                    value => _inputStart = value);

                TerminalCommandDispatcher dispatcher =
                    new(TerminalCommandRegistry.CreateDefault());

                _terminalViewModel = new(dispatcher,
                    () => new TerminalCommandContext(_terminalOutput, dataDisplay, CancellationToken.None));

                _suppressHighlight = true;
                _terminalOutput.Print(Prompt);
                _inputStart = Terminal.Document.Selection.StartPosition;
                _suppressHighlight = false;

                Terminal.Focus(FocusState.Programmatic);

                _highLightTimer = new DispatcherTimer()
                {
                    Interval = TimeSpan.FromMilliseconds(30),
                };
                _highLightTimer.Tick += HighLightTimer_Tick;
            };
        }

        private void HighLightTimer_Tick(object? sender, object e)
        {
            _highLightTimer?.Stop();
            HighlightKeyWords();
        }

        private async void Terminal_PreviewKeyDown(object sender, KeyRoutedEventArgs e)
        {
            try
            {
                RichEditTextDocument textDocument = Terminal.Document;
                ITextSelection selection = textDocument.Selection;

                if (e.Key == VirtualKey.Enter)
                {
                    e.Handled = true;
                    await SubmitCurrentLineAsync();
                    return;
                }


                if (e.Key == VirtualKey.Left)
                {
                    if (selection.StartPosition <= _inputStart)
                    {
                        e.Handled = true;
                        selection.SetRange(_inputStart, _inputStart);
                    }
                }

                if (e.Key == VirtualKey.Back)
                {
                    if (selection.StartPosition <= _inputStart)
                    {
                        e.Handled = true;
                    }
                }

                if (e.Key == VirtualKey.Home)
                {
                    e.Handled = true;
                    selection.SetRange(_inputStart, _inputStart);
                }

                if (e.Key is VirtualKey.Up or VirtualKey.Down)
                {
                    e.Handled = true;
                }
            }
            catch (Exception exception)
            {
                _terminalOutput?.PrintLine(exception.Message, TerminalMessageKind.Error);
            }
        }

        private async Task SubmitCurrentLineAsync()
        {
            RichEditTextDocument textDocument = Terminal.Document;

            int end = textDocument.GetRange(0, int.MaxValue).EndPosition;
            if (end < _inputStart)
            {
                _inputStart = end;
                return;
            }

            ITextRange commandRange = textDocument.GetRange(_inputStart, end);
            commandRange.GetText(TextGetOptions.None, out string commandText);
            commandText = commandText.TrimEnd('\r', '\n');

            bool scroll;

            if (string.IsNullOrWhiteSpace(commandText))
            {
                _suppressHighlight = true;
                _enterCount++;

                scroll = (_enterCount % 10) == 0;

                _terminalOutput?.PrintLine("");
                _terminalOutput?.Print(Prompt);

                if (scroll)
                {
                    textDocument.Selection.ScrollIntoView(PointOptions.None);
                }

                _inputStart = textDocument.Selection.StartPosition;
                _suppressHighlight = false;
                return;
            }

            HighlightKeyWords();

            _suppressHighlight = true;

            _terminalOutput?.PrintLine("");

            await _terminalViewModel!.SubmitAsync(commandText);

            _terminalOutput?.Print(Prompt);

            _inputStart = textDocument.Selection.StartPosition;

            _enterCount++;
            scroll = (_enterCount % 10) == 0;
            if (scroll)
            {
                textDocument.Selection.ScrollIntoView(PointOptions.None);
            }

            _suppressHighlight = false;
        }

        private void HighlightKeyWords()
        {
            RichEditTextDocument textDocument = Terminal.Document;

            int end = textDocument.GetRange(0, int.MaxValue).EndPosition;
            if (end <= _inputStart) return;

            ITextRange commandRange = textDocument.GetRange(_inputStart, end);
            commandRange.GetText(TextGetOptions.None, out string commandText);
            commandText = commandText.TrimEnd('\r', '\n');

            commandRange.CharacterFormat.ForegroundColor = ActualTheme == ElementTheme.Light ? Colors.Black : Colors.White;
            commandRange.CharacterFormat.Bold = FormatEffect.Off;

            var quotedRanges = new List<(int Start, int End)>();

            const string quotePattern = "\"(.*?)\"";

            Color stringColor = ActualTheme == ElementTheme.Light
                ? Color.FromArgb(255, 186, 85, 211)
                : Colors.DarkOrange;

            foreach (Match m in Regex.Matches(commandText, quotePattern))
            {
                quotedRanges.Add((m.Index, m.Index + m.Length));

                ITextRange strRange = textDocument.GetRange(
                    _inputStart + m.Index,
                    _inputStart + m.Index + m.Length);

                strRange.CharacterFormat.ForegroundColor = stringColor;
                strRange.CharacterFormat.Bold = FormatEffect.On;
            }

            static bool IsInsideQuotes(int index, List<(int Start, int End)> ranges)
            {
                foreach (var (s, e) in ranges)
                    if (index >= s && index < e)
                        return true;
                return false;
            }

            Dictionary<string, Color> keywordColors = ActualTheme == ElementTheme.Light
                ? Utilities.LightThemeKeywordColors
                : Utilities.DarkThemeKeywordColors;

            foreach (KeyValuePair<string, Color> keywordColor in keywordColors)
            {
                string keyword = keywordColor.Key;
                Color color = keywordColor.Value;

                string pattern = keyword.StartsWith("-", StringComparison.Ordinal)
                    ? $@"(?<!\S){Regex.Escape(keyword)}(?!\S)"
                    : $@"\b{Regex.Escape(keyword)}\b";

                foreach (Match match in Regex.Matches(commandText, pattern, RegexOptions.IgnoreCase))
                {
                    if (IsInsideQuotes(match.Index, quotedRanges))
                    {
                        continue;
                    }

                    ITextRange hitRange = textDocument.GetRange(
                        _inputStart + match.Index,
                        _inputStart + match.Index + match.Length);

                    hitRange.CharacterFormat.ForegroundColor = color;
                }
            }
        }


        private void Terminal_TextChanging(RichEditBox sender, RichEditBoxTextChangingEventArgs args)
        {
            if (_suppressHighlight)
            {
                return;
            }

            if (sender.Document.Selection.StartPosition < _inputStart)
            {
                return;
            }

            var defaultColor = ActualTheme == ElementTheme.Light ? Colors.Black : Colors.White;
            sender.Document.Selection.CharacterFormat.ForegroundColor = defaultColor;
            sender.Document.Selection.CharacterFormat.Bold = FormatEffect.Off;

            _highLightTimer?.Stop();
            _highLightTimer?.Start();
        }
    }
}
