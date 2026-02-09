using BlueShell.Model;
using BlueShell.Terminal.Abstractions;
using BlueShell.Terminal.Infrastructure;
using BlueShell.Terminal.WinUI;
using BlueShell.View.Pages.States;
using BlueShell.ViewModel;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.System;
using Windows.UI.Core;
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
        private const string Prompt = "Shell > ";
        private const string StateKey = "TerminalPage";

        private int _inputStart;
        private int _enterCount;

        private bool _suppressHighlight;
        private bool _splitView = true;
        private bool _isInitialized;
        private bool _isRestoring;
        private bool _restoreRequested;

        private DispatcherTimer? _highLightTimer;

        private TerminalViewModel? _terminalViewModel;
        private TerminalOutput? _terminalOutput;
        private DataDisplay? _dataDisplay;
        private TabModel? _tabModel;
        private TerminalPageState? _terminalPageState;

        private GridLength _savedTerminalWidth = new(2, GridUnitType.Star);
        private GridLength _savedDisplayWidth = new(3, GridUnitType.Star);

        public TerminalPage()
        {
            InitializeComponent();

            Loaded += TerminalPage_Loaded;
            Unloaded += TerminalPage_Unloaded;
            ActualThemeChanged += AppActualThemeChanged;
        }

        private void TerminalPage_Unloaded(object sender, RoutedEventArgs e)
        {
            SaveToState();

            if (_highLightTimer is null)
            {
                return;
            }

            _highLightTimer.Stop();
            _highLightTimer.Tick -= HighLightTimer_Tick;
        }

        private void TerminalPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isInitialized)
            {
                return;
            }

            _isInitialized = true;

            _dataDisplay = new DataDisplay(
                DataDisplay,
                (item) =>
                {
                    if (_isRestoring)
                    {
                        return;
                    }

                    if (_terminalPageState is null)
                    {
                        return;
                    }

                    if (item is DataDisplayItem dataDisplayItem)
                    {
                        _terminalPageState.DisplayItems.Add(dataDisplayItem);
                    }
                });
            _terminalOutput = new TerminalOutput(
                Terminal,
                () => ActualTheme,
                value => _inputStart = value,
                onPrinted: (text, messageKind, fontName) =>
                {
                    if (_isRestoring)
                    {
                        return;
                    }

                    if (_terminalPageState is null)
                    {
                        return;
                    }

                    TerminalLine terminalLine = new()
                    {
                        Text = text,
                        MessageKind = messageKind,
                        FontName = fontName
                    };
                    _terminalPageState.Lines.Add(terminalLine);
                });

            TerminalCommandDispatcher dispatcher =
                new(TerminalCommandRegistry.CreateDefault());

            _terminalViewModel = new TerminalViewModel(dispatcher,
                () => new TerminalCommandContext(_terminalOutput, _dataDisplay, CancellationToken.None));

            _suppressHighlight = true;

            _inputStart = Terminal.Document.Selection.StartPosition;
            _suppressHighlight = false;

            Terminal.Focus(FocusState.Programmatic);

            _highLightTimer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromMilliseconds(30),
            };
            _highLightTimer.Tick += HighLightTimer_Tick;
            if (_restoreRequested)
            {
                RestoreFromState();
            }
            else
            {
                _terminalOutput.Print(Prompt, isRestoring: true);
                _terminalPageState?.Lines.Add(new TerminalLine()
                {
                    Text = Prompt,
                    FontName = "Cascadia Code",
                });
            }
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

                DisplayColumnDefinition.Width = new GridLength(0);
                TerminalColumnDefinition.Width = new GridLength(1, GridUnitType.Star);
            }
        }

        private void AppActualThemeChanged(FrameworkElement sender, object args)
        {
            RestoreFromState();
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

            TerminalUtilities.Highlight(new HighlightContext(
                Terminal.Document,
                HighlightMode.CurrentInput,
                ActualTheme,
                _inputStart,
                Prompt));

            _suppressHighlight = true;

            _terminalPageState?.Lines.Add(new TerminalLine()
            {
                Text = commandText,
                FontName = "Cascadia Code",
            });

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

        private void SaveToState()
        {
            if (_terminalPageState is null)
            {
                return;
            }

            RichEditTextDocument document = Terminal.Document;

            _terminalPageState.InputStart = _inputStart;
            _terminalPageState.EnterCount = _enterCount;
            _terminalPageState.TextSelectorPosition = document.Selection.StartPosition;

            _terminalPageState.TerminalWidthStar = TerminalColumnDefinition.Width.Value;
            _terminalPageState.DisplayWidthStar = DisplayColumnDefinition.Width.Value;

            _terminalPageState.SplitView = _splitView;
        }

        private void RestoreFromState()
        {
            if (_terminalPageState is null)
            {
                return;
            }

            if (_terminalOutput is null)
            {
                return;
            }

            _isRestoring = true;
            try
            {
                _highLightTimer?.Stop();

                GridLength restoredTerminalWidth = new(_terminalPageState.TerminalWidthStar, GridUnitType.Star);
                GridLength restoredDisplayWidth = new(_terminalPageState.DisplayWidthStar, GridUnitType.Star);

                TerminalColumnDefinition.Width = restoredTerminalWidth;
                DisplayColumnDefinition.Width = restoredDisplayWidth;

                _splitView = _terminalPageState.SplitView;
                DataDisplayGrid.Visibility = _splitView ? Visibility.Visible : Visibility.Collapsed;

                if (_splitView && _savedDisplayWidth.Value > 0 && _savedTerminalWidth.Value > 0)
                {
                    _savedTerminalWidth = restoredTerminalWidth;
                    _savedDisplayWidth = restoredDisplayWidth;
                }

                Terminal.Document.SetText(TextSetOptions.None, string.Empty);

                if (_terminalPageState.Lines.Count == 0)
                {
                    _terminalOutput.Print(Prompt, TerminalMessageKind.Output, "Cascadia Code", true);
                }

                foreach (TerminalLine terminalLine in _terminalPageState.Lines)
                {
                    _terminalOutput.Print(terminalLine.Text, terminalLine.MessageKind, terminalLine.FontName, true);
                }

                _dataDisplay?.Clear();
                foreach (DataDisplayItem dataDisplayItem in _terminalPageState.DisplayItems)
                {
                    _dataDisplay?.Add(dataDisplayItem);
                }

                _enterCount = _terminalPageState.EnterCount;

                int end = Terminal.Document.GetRange(0, int.MaxValue).EndPosition;
                int textSelector = Math.Clamp(_terminalPageState.TextSelectorPosition, 0, end);

                Terminal.Document.Selection.SetRange(textSelector, textSelector);

                _inputStart = _terminalPageState.InputStart;
            }
            finally
            {
                _isRestoring = false;
                _restoreRequested = false;
                TerminalUtilities.Highlight(new HighlightContext(
                    Terminal.Document,
                    HighlightMode.AllCommands,
                    ActualTheme,
                    _inputStart,
                    Prompt));
                _highLightTimer?.Start();
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            _tabModel = e.Parameter as TabModel ?? DataContext as TabModel;
            _terminalPageState = _tabModel?.GetOrCreateState(StateKey, () => new TerminalPageState());
            _highLightTimer?.Start();

            _restoreRequested =
                _terminalPageState is not null &&
                (_terminalPageState.Lines.Count > 0 ||
                 _terminalPageState.DisplayItems.Count > 0 ||
                 _terminalPageState.EnterCount > 0);

            if (_isInitialized && _restoreRequested)
            {
                RestoreFromState();
            }
        }

        private void HighLightTimer_Tick(object? sender, object e)
        {
            _highLightTimer?.Stop();
            TerminalUtilities.Highlight(new HighlightContext(
                Terminal.Document,
                HighlightMode.CurrentInput,
                ActualTheme,
                _inputStart,
                Prompt));
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
                        case VirtualKey.Q:
                            _terminalViewModel?.Cancel();
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

            TerminalUtilities.Highlight(new HighlightContext(
                Terminal.Document,
                HighlightMode.CurrentToken,
                ActualTheme,
                _inputStart,
                Prompt));

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

                foreach (string line in lines)
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
