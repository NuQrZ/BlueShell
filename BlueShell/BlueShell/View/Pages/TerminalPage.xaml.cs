using BlueShell.Helpers;
using BlueShell.Terminal.Abstractions;
using BlueShell.Terminal.Infrastructure;
using BlueShell.Terminal.WinUI;
using BlueShell.ViewModel;
using Microsoft.UI.Input;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Core;

namespace BlueShell.View.Pages
{
    public sealed partial class TerminalPage : Page
    {
        private const string Prompt = "Shell > ";
        private readonly int _inputStart = Prompt.Length;

        private bool _splitView = true;

        private TerminalOutput? _terminalOutput;
        private DataDisplay? _dataDisplay;
        private TerminalViewModel? _terminalViewModel;

        private GridLength _savedTerminalWidth = new(2, GridUnitType.Star);
        private GridLength _savedDisplayWidth = new(3, GridUnitType.Star);

        private DispatcherTimer? _highlightDebouncer;

        public TerminalPage()
        {
            InitializeComponent();
            Loaded += TerminalPage_Loaded;
        }

        private void InitializeInput()
        {
            Terminal.Document.SetText(TextSetOptions.None, Prompt);
            Terminal.Document.Selection.SetRange(_inputStart, _inputStart);
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

        private async Task HandleInput()
        {
            RichEditTextDocument textDocument = Terminal.Document;

            ITextRange fullRange = textDocument.GetRange(0, int.MaxValue);
            int end = fullRange.EndPosition;

            ITextRange inputRange = textDocument.GetRange(_inputStart, end);
            inputRange.GetText(TextGetOptions.None, out string command);

            command = command.Trim();

            textDocument.SetText(TextSetOptions.None, Prompt);
            textDocument.Selection.SetRange(_inputStart, _inputStart);

            await _terminalViewModel!.SubmitAsync(command);
        }

        private void TerminalPage_Loaded(object sender, RoutedEventArgs e)
        {
            _terminalOutput = new TerminalOutput(
                OutputRepeater,
                OutputScrollViewer,
                () => ActualTheme);

            _dataDisplay = new DataDisplay(DataDisplay);

            TerminalCommandDispatcher dispatcher = new(
                TerminalCommandRegistry.CreateDefault());

            _terminalViewModel = new TerminalViewModel(dispatcher,
                () => new TerminalCommandContext(_terminalOutput, _dataDisplay, CancellationToken.None));

            InitializeInput();
            Terminal.Focus(FocusState.Programmatic);

            _highlightDebouncer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromMilliseconds(30)
            };

            _highlightDebouncer.Tick += HighlightDebouncer_Tick;
        }

        private void HighlightDebouncer_Tick(object? sender, object e)
        {
            _highlightDebouncer?.Stop();
            TerminalUtilities.HighlightCurrentInput(
                Terminal.Document,
                _inputStart,
                ActualTheme);
        }

        private async void Terminal_PreviewKeyDown(object sender, KeyRoutedEventArgs e)
        {
            try
            {
                ITextSelection textSelection = Terminal.Document.Selection;

                int start = textSelection.StartPosition;
                int end = textSelection.EndPosition;

                bool isCtrl = (
                    InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Control)
                    & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down;

                if (e.Key == VirtualKey.Enter)
                {
                    e.Handled = true;
                    await HandleInput();
                    return;
                }

                if (e.Key == VirtualKey.Home)
                {
                    e.Handled = true;
                    textSelection.SetRange(_inputStart, _inputStart);
                }

                if (e.Key is VirtualKey.Left or VirtualKey.Right && start <= _inputStart)
                {
                    e.Handled = true;
                    textSelection.SetRange(_inputStart, _inputStart);
                }

                if (e.Key is VirtualKey.Back)
                {
                    if (textSelection.Length > 0)
                    {
                        if (start < _inputStart)
                        {
                            textSelection.SetRange(_inputStart, _inputStart);
                        }

                        return;
                    }

                    if (start > _inputStart)
                    {
                        return;
                    }

                    e.Handled = true;
                    textSelection.SetRange(_inputStart, _inputStart);

                    return;
                }

                if (e.Key is VirtualKey.Delete)
                {
                    if (start <= _inputStart)
                    {
                        e.Handled = true;
                        textSelection.SetRange(_inputStart, _inputStart);
                        return;
                    }

                    if (start == _inputStart)
                    {
                        e.Handled = true;

                        RichEditTextDocument textDocument = Terminal.Document;
                        ITextRange range = textDocument.GetRange(_inputStart, _inputStart + 1);

                        if (range.EndPosition > range.StartPosition)
                        {
                            range.Text = string.Empty;
                        }

                        return;
                    }
                }

                if (isCtrl)
                {
                    switch (e.Key)
                    {
                        case VirtualKey.A:
                            e.Handled = true;
                            textSelection.SetRange(_inputStart, end);
                            return;
                        case VirtualKey.Q:
                            _terminalViewModel?.Cancel();
                            return;
                    }
                }
            }
            catch (Exception exception)
            {
                _terminalOutput?.WriteLine(exception.Message, messageKind: TerminalMessageKind.Error);
            }
        }

        private void Terminal_SelectionChanged(object sender, RoutedEventArgs e)
        {
            ITextSelection textSelection = Terminal.Document.Selection;

            if (textSelection.StartPosition < _inputStart)
            {
                textSelection.SetRange(_inputStart, _inputStart);
            }
        }

        private void Terminal_TextChanging(RichEditBox sender, RichEditBoxTextChangingEventArgs args)
        {
            _highlightDebouncer?.Stop();
            _highlightDebouncer?.Start();
        }
    }
}
