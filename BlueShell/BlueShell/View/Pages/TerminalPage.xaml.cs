using BlueShell.Core;
using BlueShell.Terminal.Abstractions;
using BlueShell.Terminal.Infrastructure;
using BlueShell.Terminal.WinUI;
using BlueShell.ViewModel;
using Microsoft.UI;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using FormatEffect = Microsoft.UI.Text.FormatEffect;
using ITextRange = Microsoft.UI.Text.ITextRange;
using ITextSelection = Microsoft.UI.Text.ITextSelection;
using PointOptions = Microsoft.UI.Text.PointOptions;
using RichEditTextDocument = Microsoft.UI.Text.RichEditTextDocument;
using TextGetOptions = Microsoft.UI.Text.TextGetOptions;
using TextSetOptions = Microsoft.UI.Text.TextSetOptions;

namespace BlueShell.View.Pages
{
    public sealed partial class TerminalPage
    {
        private const string Prompt = "Terminal > ";

        private int _inputStart;
        private int _enterCount;

        private bool _suppressHighlight;
        private bool _splitView = true;

        private DispatcherTimer? _highLightTimer;

        private TerminalViewModel? _terminalViewModel;
        private TerminalOutput? _terminalOutput;

        private GridLength _savedTerminalWidth = new(2, GridUnitType.Star);
        private GridLength _savedDisplayWidth = new(3, GridUnitType.Star);

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

                _terminalViewModel = new TerminalViewModel(dispatcher,
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

            ActualThemeChanged += AppActualThemeChanged;
        }

        private void AppActualThemeChanged(FrameworkElement sender, object args)
        {
            HighlightKeyWords();
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

        public void ToggleLayout()
        {
            _splitView = !_splitView;
            if (_splitView)
            {
                DataDisplayGrid.Visibility = Visibility.Visible;

                TerminalColumnDefinition.Width = _savedTerminalWidth;
                DisplayColumnDefinition.Width = _savedDisplayWidth;
            }
            else
            {
                if (TerminalColumnDefinition.Width.Value > 0)
                {
                    _savedTerminalWidth = TerminalColumnDefinition.Width;
                }

                if (DisplayColumnDefinition.Width.Value > 0)
                {
                    _savedDisplayWidth = DisplayColumnDefinition.Width;
                }

                DataDisplayGrid.Visibility = Visibility.Collapsed;
                DisplayColumnDefinition.Width = new GridLength(0);

                TerminalColumnDefinition.Width = new GridLength(1, GridUnitType.Star);
            }
        }

        private void HighlightCurrentTokenOnly()
        {
            RichEditTextDocument document = Terminal.Document;
            ITextSelection textSelection = document.Selection;

            int end = document.GetRange(0, int.MaxValue).EndPosition;

            if (end <= _inputStart)
            {
                return;
            }

            int textSelector = textSelection.StartPosition;

            ITextRange inputRange = document.GetRange(_inputStart, end);
            inputRange.GetText(TextGetOptions.None, out string currentText);
            currentText = currentText.TrimEnd('\r', '\n');


            int selectorIndex = Math.Min(currentText.Length, textSelector - _inputStart);

            if (selectorIndex < 0)
            {
                selectorIndex = 0;
            }

            int lineStart = currentText.LastIndexOf('\n', Math.Max(0, selectorIndex - 1));
            if (lineStart < 0)
            {
                lineStart = 0;
            }
            else
            {
                lineStart += 1;
            }

            int wordStart = selectorIndex;
            while (wordStart > lineStart && !char.IsWhiteSpace(currentText[wordStart - 1]))
            {
                wordStart--;
            }

            int wordEnd = selectorIndex;
            while (wordEnd < currentText.Length && !char.IsWhiteSpace(currentText[wordEnd]))
            {
                wordEnd++;
            }

            if (wordEnd <= wordStart)
            {
                return;
            }

            string token = currentText.Substring(wordStart, wordEnd - wordStart);

            Color defaultColor = ActualTheme == ElementTheme.Light ? Colors.Black : Colors.White;

            ITextRange tokenRange = document.GetRange(_inputStart + wordStart, _inputStart + wordEnd);

            if (token.Contains(">>", StringComparison.Ordinal))
            {
                return;
            }

            if (token.Contains('"', StringComparison.Ordinal))
            {
                return;
            }

            Dictionary<string, Color> keywordColors = ActualTheme == ElementTheme.Light
                ? Utilities.LightThemeKeywordColors
                : Utilities.DarkThemeKeywordColors;

            bool isKeyword = keywordColors.TryGetValue(token, out Color color);

            if (!isKeyword)
            {
                tokenRange.CharacterFormat.ForegroundColor = defaultColor;
                tokenRange.CharacterFormat.Bold = FormatEffect.Off;
                return;
            }

            tokenRange.CharacterFormat.ForegroundColor = color;
            tokenRange.CharacterFormat.Bold = FormatEffect.Off;
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
                RichEditTextDocument document = Terminal.Document;
                ITextSelection textSelection = document.Selection;

                bool ctrlDown =
                    (InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Control) & CoreVirtualKeyStates.Down) ==
                    CoreVirtualKeyStates.Down;

                if (e.Key == VirtualKey.Enter)
                {
                    e.Handled = true;
                    await SubmitCurrentLineAsync();
                    return;
                }

                if (textSelection.StartPosition < _inputStart || textSelection.EndPosition < _inputStart)
                {
                    if (!(ctrlDown && e.Key is VirtualKey.A or VirtualKey.C))
                    {
                        e.Handled = true;
                        textSelection.SetRange(_inputStart, _inputStart);
                        return;
                    }
                }

                if (ctrlDown)
                {
                    switch (e.Key)
                    {
                        case VirtualKey.A:
                            e.Handled = true;
                            Microsoft.UI.Text.ITextCharacterFormat characterFormat = Terminal.Document.Selection.CharacterFormat;
                            Terminal.Document.Selection.SetRange(0, int.MaxValue);
                            Terminal.Document.Selection.CharacterFormat = characterFormat;
                            return;

                        case VirtualKey.C:
                            return;
                    }
                }

                if (textSelection.Length > 0 && (textSelection.StartPosition < _inputStart || textSelection.EndPosition < _inputStart))
                {
                    switch (e.Key)
                    {
                        case VirtualKey.Back:
                        case VirtualKey.Delete:
                            return;
                    }

                    switch (ctrlDown)
                    {
                        case true when e.Key is VirtualKey.X:
                            e.Handled = true;
                            return;
                    }
                }

                if (textSelection.Length == 0)
                {
                    switch (e.Key)
                    {
                        case VirtualKey.Left when textSelection.StartPosition <= _inputStart:
                        case VirtualKey.Home when textSelection.StartPosition <= _inputStart:
                            e.Handled = true;
                            textSelection.SetRange(_inputStart, _inputStart);
                            return;
                        case VirtualKey.Back when textSelection.StartPosition <= _inputStart:
                            e.Handled = true;
                            return;
                    }
                }
                else
                {
                    if (e.Key == VirtualKey.Home)
                    {
                        e.Handled = true;
                        textSelection.SetRange(_inputStart, _inputStart);
                        return;
                    }
                }

                if (e.Key is VirtualKey.Up or VirtualKey.Down || (ctrlDown && e.Key == VirtualKey.E))
                {
                    e.Handled = true;
                }
            }
            catch (Exception exception)
            {
                _terminalOutput?.PrintLine(exception.Message, TerminalMessageKind.Error);
            }
        }

        private void Terminal_TextChanging(RichEditBox sender, RichEditBoxTextChangingEventArgs args)
        {
            if (_suppressHighlight)
            {
                return;
            }

            HighlightCurrentTokenOnly();

            _highLightTimer?.Stop();
            _highLightTimer?.Start();
        }

        private static string SanitizePaste(string pastedText)
        {
            pastedText = pastedText.Replace("\r\n", "\n").Replace("\r", "\n");

            pastedText = pastedText.Replace("\n", " ");
            pastedText = pastedText.Replace("\t", " ");

            return pastedText;
        }

        private async void Terminal_Paste(object sender, TextControlPasteEventArgs e)
        {
            try
            {
                e.Handled = true;

                DataPackageView content = Clipboard.GetContent();
                if (content == null || !content.Contains(StandardDataFormats.Text))
                {
                    return;
                }

                string pastedText = SanitizePaste(await content.GetTextAsync());

                string[] lines = pastedText.Split('\n');

                RichEditTextDocument document = Terminal.Document;
                ITextSelection textSelection = document.Selection;

                int start = textSelection.StartPosition;
                int end = textSelection.EndPosition;

                if (start < _inputStart)
                {
                    return;
                }

                if (end < _inputStart)
                {
                    return;
                }

                foreach (var line in lines)
                {
                    string fixedLine = line.Replace("\t", "");

                    textSelection.SetRange(start, end);

                    await Task.Delay(30);

                    textSelection.SetText(TextSetOptions.None, string.Empty);
                    textSelection.SetText(TextSetOptions.None, fixedLine);
                }
            }
            catch (Exception exception)
            {
                _terminalOutput?.PrintLine(exception.Message, TerminalMessageKind.Error);
            }
        }
    }
}
