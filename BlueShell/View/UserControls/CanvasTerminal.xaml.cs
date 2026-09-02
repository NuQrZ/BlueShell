using BlueShell.Helpers;
using BlueShell.Model;
using BlueShell.Terminal;
using BlueShell.Terminal.Abstractions;
using BlueShell.Terminal.Implementations;
using BlueShell.Terminal.Infrastructure;
using BlueShell.ViewModel;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.UI;
using Microsoft.UI.Input;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Threading;
using Windows.System;
using Windows.UI;
using Windows.UI.Text;

namespace BlueShell.View.UserControls
{
    public sealed partial class CanvasTerminal : UserControl
    {
        private const float PaddingLeft = 8;
        private const float PaddingTop = 8;

        private const float MinFontSize = 8;
        private const float MaxFontSize = 48;

        private const string Prompt = "Shell > ";

        private const VirtualKey OemPlus = (VirtualKey)0xBB;
        private const VirtualKey OemMinus = (VirtualKey)0xBD;

        private bool _caretVisible = true;

        private bool _refreshPending;
        private bool _scrollToBottomPending;

        private bool _followingOutput = true;

        // Sprečava da programske promene ScrollViewer-a
        // izgledaju kao korisničko skrolovanje.
        private bool _updatingScrollView;

        private readonly CanvasTextFormat _textFormat = new()
        {
            FontFamily = "Cascadia Code",
            FontSize = 18,
            WordWrapping = CanvasWordWrapping.NoWrap,
        };

        private readonly DispatcherTimer _dispatcherTimer = new()
        {
            Interval = TimeSpan.FromMilliseconds(400)
        };

        private Color DefaultColor =>
            ActualTheme == ElementTheme.Light
                ? Colors.Black
                : Colors.White;

        private TabModel? _tabModel;

        private readonly ITerminalOutput? _terminalOutput;
        private readonly TerminalViewModel? _terminalViewModel;
        private readonly TerminalBuffer _terminalBuffer = new();

        public CanvasTerminal()
        {
            InitializeComponent();

            _terminalOutput = new TerminalOutput(
                _terminalBuffer,
                () => ActualTheme);

            TerminalCommandDispatcher dispatcher = new(
                TerminalCommandRegistry.CreateDefault());

            _terminalViewModel = new TerminalViewModel(
                dispatcher,
                () => new TerminalCommandContext(
                    _terminalOutput,
                    _tabModel,
                    CancellationToken.None));

            _dispatcherTimer.Tick += DispatcherTimer_Tick;
            _terminalBuffer.Changed += TerminalBuffer_Changed;
        }

        public void BuildTabModel(TabModel? tabModel)
        {
            _tabModel = tabModel;
        }

        private static bool IsKeyDown(VirtualKey key)
        {
            return (InputKeyboardSource.GetKeyStateForCurrentThread(key)
                    & Windows.UI.Core.CoreVirtualKeyStates.Down) != 0;
        }

        public void UpdateScrollView(bool isCommandRunning)
        {
            float lineHeight = _textFormat.FontSize + 4;

            int visibleLineCount =
                _terminalBuffer.Count +
                (isCommandRunning ? 0 : 1);

            double contentHeight =
                PaddingTop +
                visibleLineCount * lineHeight +
                PaddingTop;

            Terminal.Height = Math.Max(
                TerminalScrollView.ViewportHeight,
                contentHeight);
        }

        public void RefreshTerminal(bool scrollToBottom)
        {
            /*
             * Mora da se zapamti PRE promene Height-a.
             *
             * UpdateScrollView može da promeni ScrollableHeight,
             * što može da izazove ViewChanged.
             */
            bool shouldScrollToBottom =
                scrollToBottom && _followingOutput;

            _updatingScrollView = true;

            try
            {
                UpdateScrollView(
                    _terminalViewModel!.IsCommandRunning);

                Terminal.Invalidate();

                /*
                 * Potreban nam je novi ScrollableHeight pre ChangeView.
                 */
                TerminalScrollView.UpdateLayout();

                if (!shouldScrollToBottom)
                {
                    return;
                }

                TerminalScrollView.ChangeView(
                    null,
                    TerminalScrollView.ScrollableHeight,
                    null,
                    true);

                /*
                 * Još uvek smo u follow režimu.
                 *
                 * ViewChanged izazvan programskim promenama
                 * se ignoriše dok je _updatingScrollView == true.
                 */
                _followingOutput = true;
            }
            finally
            {
                _updatingScrollView = false;
            }
        }

        public void RequestRefresh(bool scrollToBottom)
        {
            /*
             * Ako je makar jedan zahtev tražio scroll na dno,
             * ne želimo da ga kasniji Refresh(false) poništi.
             */
            _scrollToBottomPending |= scrollToBottom;

            if (_refreshPending)
            {
                return;
            }

            _refreshPending = true;

            DispatcherQueue.TryEnqueue(() =>
            {
                _refreshPending = false;

                bool shouldScrollToBottom =
                    _scrollToBottomPending;

                _scrollToBottomPending = false;

                RefreshTerminal(shouldScrollToBottom);
            });
        }

        private void DispatcherTimer_Tick(
            object? sender,
            object e)
        {
            if (_terminalViewModel!.IsCommandRunning)
            {
                return;
            }

            _caretVisible = !_caretVisible;

            Terminal.Invalidate();
        }

        private void TerminalBuffer_Changed(
            object? sender,
            EventArgs e)
        {
            RequestRefresh(true);
        }

        private void TerminalScrollView_SizeChanged(
            object sender,
            SizeChangedEventArgs e)
        {
            /*
             * Ako smo pratili output, ostani na dnu.
             *
             * Ako je korisnik otišao gore,
             * RefreshTerminal neće ga vratiti dole jer je
             * _followingOutput == false.
             */
            RefreshTerminal(true);
        }

        private void Terminal_RegionsInvalidated(
            CanvasVirtualControl sender,
            CanvasRegionsInvalidatedEventArgs eventArgs)
        {
            int completedLinesCount =
                _terminalBuffer.Count;

            float lineHeight =
                _textFormat.FontSize + 4;

            float caretHeight =
                _textFormat.FontSize;

            float caretOffsetY =
                (lineHeight - caretHeight) / 2;

            foreach (var region in eventArgs.InvalidatedRegions)
            {
                using var drawingSession =
                    sender.CreateDrawingSession(region);

                int firstVisibleLine = Math.Max(
                    0,
                    (int)Math.Floor(
                        (region.Top - PaddingTop) /
                        lineHeight) - 1);

                int lastVisibleLine = Math.Min(
                    completedLinesCount - 1,
                    (int)Math.Ceiling(
                        (region.Bottom - PaddingTop) /
                        lineHeight) + 1);

                for (int i = firstVisibleLine;
                     i <= lastVisibleLine;
                     i++)
                {
                    TerminalLine line =
                        _terminalBuffer.Lines[i];

                    float lineY =
                        PaddingTop +
                        i * lineHeight;

                    float currentX =
                        PaddingLeft;

                    foreach (TerminalLineSegment segment
                             in line.Segments)
                    {
                        _textFormat.FontWeight =
                            segment.FontWeight;

                        _textFormat.FontStyle =
                            segment.FontStyle;

                        drawingSession.DrawText(
                            segment.Text,
                            currentX,
                            lineY,
                            segment.Color ?? DefaultColor,
                            _textFormat);

                        using CanvasTextLayout segmentLayout = new(
                            sender.Device,
                            segment.Text,
                            _textFormat,
                            0,
                            0);

                        float segmentWidth =
                            (float)segmentLayout
                                .LayoutBoundsIncludingTrailingWhitespace
                                .Width;

                        currentX += segmentWidth;
                    }
                }

                if (_terminalViewModel!.IsCommandRunning)
                {
                    continue;
                }

                float promptY =
                    PaddingTop +
                    completedLinesCount * lineHeight;

                bool promptIntersectsRegion =
                    promptY + lineHeight >= region.Top &&
                    promptY <= region.Bottom;

                if (!promptIntersectsRegion)
                {
                    continue;
                }

                _textFormat.FontWeight =
                    FontWeights.Normal;

                _textFormat.FontStyle =
                    FontStyle.Normal;

                drawingSession.DrawText(
                    Prompt,
                    PaddingLeft,
                    promptY,
                    DefaultColor,
                    _textFormat);

                using CanvasTextLayout promptLayout = new(
                    sender.Device,
                    Prompt,
                    _textFormat,
                    0,
                    0);

                float promptWidth =
                    (float)promptLayout
                        .LayoutBoundsIncludingTrailingWhitespace
                        .Width;

                string currentInput =
                    _terminalViewModel.CurrentLine;

                drawingSession.DrawText(
                    currentInput,
                    PaddingLeft + promptWidth,
                    promptY,
                    TerminalUtilities.GetCommandColor(
                        currentInput,
                        ActualTheme,
                        DefaultColor),
                    _textFormat);

                string textBeforeCaret =
                    currentInput[
                        .._terminalViewModel.CaretPosition];

                using CanvasTextLayout textBeforeCaretLayout = new(
                    sender.Device,
                    textBeforeCaret,
                    _textFormat,
                    0,
                    0);

                float textBeforeCaretWidth =
                    (float)textBeforeCaretLayout
                        .LayoutBoundsIncludingTrailingWhitespace
                        .Width;

                float caretX =
                    PaddingLeft +
                    promptWidth +
                    textBeforeCaretWidth;

                if (_caretVisible)
                {
                    drawingSession.DrawLine(
                        caretX,
                        promptY + caretOffsetY,
                        caretX,
                        promptY +
                        caretOffsetY +
                        caretHeight,
                        DefaultColor);
                }
            }
        }

        private void TerminalUserControl_CharacterReceived(
            UIElement sender,
            CharacterReceivedRoutedEventArgs eventArgs)
        {
            if (_terminalViewModel!.IsCommandRunning ||
                IsKeyDown(VirtualKey.Control))
            {
                return;
            }

            int intChar =
                (int)eventArgs.Character;

            if (intChar < 32 ||
                intChar == 127)
            {
                return;
            }

            string text =
                char.ConvertFromUtf32(intChar);

            _terminalViewModel.InsertText(text);

            Terminal.Invalidate();

            eventArgs.Handled = true;
        }

        private void TerminalUserControl_Loaded(
            object sender,
            RoutedEventArgs eventArgs)
        {
            Focus(FocusState.Programmatic);

            _caretVisible = true;

            _dispatcherTimer.Start();

            /*
             * Ako već postoji history pri ponovnom otvaranju
             * terminala, idi na dno samo ako ga pratimo.
             */
            RefreshTerminal(true);
        }

        private void TerminalUserControl_Unloaded(
            object sender,
            RoutedEventArgs e)
        {
            _dispatcherTimer.Stop();
        }

        private void TerminalUserControl_PointerPressed(
            object sender,
            PointerRoutedEventArgs eventArgs)
        {
            Focus(FocusState.Pointer);

            _caretVisible = true;

            _dispatcherTimer.Start();

            Terminal.Invalidate();

            eventArgs.Handled = true;
        }

        private void TerminalUserControl_LostFocus(
            object sender,
            RoutedEventArgs e)
        {
            _dispatcherTimer.Stop();

            _caretVisible = false;

            Terminal.Invalidate();
        }

        private async void TerminalUserControl_KeyDown(
            object sender,
            KeyRoutedEventArgs eventArgs)
        {
            bool isCtrlPressed =
                IsKeyDown(VirtualKey.Control);

            if (isCtrlPressed)
            {
                if (eventArgs.OriginalKey == OemPlus ||
                    eventArgs.Key == VirtualKey.Add)
                {
                    _textFormat.FontSize = Math.Min(
                        _textFormat.FontSize + 1,
                        MaxFontSize);

                    /*
                     * true NE znači:
                     * "obavezno idi na dno".
                     *
                     * RefreshTerminal će otići na dno samo ako je
                     * _followingOutput već true.
                     */
                    RefreshTerminal(true);

                    eventArgs.Handled = true;
                    return;
                }

                if (eventArgs.OriginalKey == OemMinus ||
                    eventArgs.Key == VirtualKey.Subtract)
                {
                    _textFormat.FontSize = Math.Max(
                        _textFormat.FontSize - 1,
                        MinFontSize);

                    RefreshTerminal(true);

                    eventArgs.Handled = true;
                    return;
                }

                if (eventArgs.Key == VirtualKey.Q)
                {
                    _terminalViewModel?.Cancel();

                    eventArgs.Handled = true;
                    return;
                }

                if (eventArgs.Key == VirtualKey.Left)
                {
                    _terminalViewModel?.MoveCaretWordLeft();

                    Terminal.Invalidate();

                    eventArgs.Handled = true;
                    return;
                }

                if (eventArgs.Key == VirtualKey.Right)
                {
                    _terminalViewModel?.MoveCaretWordRight();

                    Terminal.Invalidate();

                    eventArgs.Handled = true;
                    return;
                }
            }

            if (_terminalViewModel!.IsCommandRunning)
            {
                eventArgs.Handled = true;
                return;
            }

            switch (eventArgs.Key)
            {
                case VirtualKey.Left:
                    _terminalViewModel.MoveCaretLeft();

                    Terminal.Invalidate();

                    eventArgs.Handled = true;
                    break;

                case VirtualKey.Right:
                    _terminalViewModel.MoveCaretRight();

                    Terminal.Invalidate();

                    eventArgs.Handled = true;
                    break;

                case VirtualKey.Home:
                    _terminalViewModel.GoToHome();

                    Terminal.Invalidate();

                    eventArgs.Handled = true;
                    break;

                case VirtualKey.End:
                    _terminalViewModel.GoToEnd();

                    Terminal.Invalidate();

                    eventArgs.Handled = true;
                    break;
            }

            switch (eventArgs.Key)
            {
                case VirtualKey.Enter:
                    {
                        string currentLine =
                            _terminalViewModel.TakeCurrentLine();

                        TerminalLine terminalLine =
                            new();

                        terminalLine.AddSegment(
                            new TerminalLineSegment(
                                Prompt,
                                DefaultColor,
                                FontWeights.Normal,
                                FontStyle.Normal));

                        terminalLine.AddSegment(
                            new TerminalLineSegment(
                                currentLine,
                                TerminalUtilities.GetCommandColor(
                                    currentLine,
                                    ActualTheme,
                                    DefaultColor),
                                FontWeights.Normal,
                                FontStyle.Normal));

                        _terminalBuffer.AddTerminalLine(
                            terminalLine);

                        _caretVisible = false;

                        try
                        {
                            await _terminalViewModel.SubmitAsync(
                                currentLine);
                        }
                        finally
                        {
                            _caretVisible = true;

                            /*
                             * Kada se command završi, prompt se ponovo
                             * pojavljuje i povećava sadržaj za jednu liniju.
                             *
                             * Ako korisnik prati output, treba da vidi
                             * novi prompt.
                             *
                             * Ako je ručno otišao gore, neće ga vratiti.
                             */
                            RefreshTerminal(true);
                        }

                        eventArgs.Handled = true;
                        break;
                    }

                case VirtualKey.Back:
                    {
                        _terminalViewModel.Backspace();

                        Terminal.Invalidate();

                        eventArgs.Handled = true;
                        break;
                    }
            }
        }

        private void TerminalUserControl_PointerWheelChanged(
            object sender,
            PointerRoutedEventArgs e)
        {
            if (!IsKeyDown(VirtualKey.Control))
            {
                return;
            }

            PointerPoint pointerPoint =
                e.GetCurrentPoint(Terminal);

            PointerPointProperties pointerPointProperties =
                pointerPoint.Properties;

            int mouseWheelDelta =
                pointerPointProperties.MouseWheelDelta;

            if (mouseWheelDelta > 0)
            {
                _textFormat.FontSize = Math.Min(
                    _textFormat.FontSize + 1,
                    MaxFontSize);
            }
            else if (mouseWheelDelta < 0)
            {
                _textFormat.FontSize = Math.Max(
                    _textFormat.FontSize - 1,
                    MinFontSize);
            }

            RefreshTerminal(true);

            e.Handled = true;
        }

        private void TerminalScrollView_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (_updatingScrollView)
            {
                return;
            }

            const double tolerance = 2;

            double distanceFromBottom = TerminalScrollView.ScrollableHeight - TerminalScrollView.VerticalOffset;

            _followingOutput = distanceFromBottom <= tolerance;
        }
    }
}