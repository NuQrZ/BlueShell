using BlueShell.Model;
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
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Windows.System;
using Windows.UI;
using Windows.UI.Text;

namespace BlueShell.View.UserControls
{
    public sealed partial class CanvasTerminal : UserControl
    {
        private const string Prompt = "Shell > ";

        private const VirtualKey OemPlus = (VirtualKey)0xBB;
        private const VirtualKey OemMinus = (VirtualKey)0xBD;

        private const float PaddingLeft = 8;
        private const float PaddingTop = 8;
        private const float PaddingBottom = 8;

        private const float MinFontSize = 8;
        private const float MaxFontSize = 48;

        private const int MaxHistorySize = 100_000;

        private readonly List<TerminalLine> _completedLines = [];
        private readonly StringBuilder _currentLine = new();

        private readonly CanvasTextFormat _textFormat = new()
        {
            FontFamily = "Cascadia Code",
            FontSize = 18,
            WordWrapping = CanvasWordWrapping.NoWrap,
        };

        private bool _caretVisible = true;
        private bool _isCommandRunning;
        private bool _startNewOutputLine = true;
        private bool _refreshPending;
        private bool _scrollToBottomPending;

        private readonly DispatcherTimer _dispatcherTimer = new()
        {
            Interval = TimeSpan.FromMilliseconds(400)
        };

        private TabModel? _tabModel;
        private ITerminalOutput? _terminalOutput;
        private TerminalViewModel? _terminalViewModel;

        private Color DefaultColor =>
            ActualTheme == ElementTheme.Light
                ? Colors.Black
                : Colors.White;

        public CanvasTerminal()
        {
            InitializeComponent();

            TerminalScrollView.SizeChanged += TerminalScrollView_SizeChanged;
            _dispatcherTimer.Tick += DispatcherTimer_Tick;
        }

        private void TerminalScrollView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RefreshTerminal(true);
        }

        #region HelperMethods
        public void BuildTabModel(TabModel? tabModel)
        {
            _tabModel = tabModel;
        }

        private static bool IsKeyDown(VirtualKey key)
        {
            return (InputKeyboardSource.GetKeyStateForCurrentThread(key)
                    & Windows.UI.Core.CoreVirtualKeyStates.Down) != 0;
        }

        private void UpdateScrollView()
        {
            float lineHeight = _textFormat.FontSize + 4;

            int visibleLineCount = _completedLines.Count + (_isCommandRunning ? 0 : 1);

            double contentHeight = PaddingTop + visibleLineCount * lineHeight + PaddingBottom;

            Terminal.Height = Math.Max(TerminalScrollView.ViewportHeight, contentHeight);
        }

        private TerminalLine GetCurrentOutputLine()
        {
            if (_startNewOutputLine || _completedLines.Count == 0)
            {
                TerminalLine line = new();

                _completedLines.Add(line);
                _startNewOutputLine = false;

                return line;
            }

            return _completedLines[^1];
        }

        public void Write(string text, Color? textColor, FontWeight fontWeight, FontStyle fontStyle)
        {
            TerminalLine line = GetCurrentOutputLine();

            TerminalLineSegment segment = new(
                text,
                textColor ?? DefaultColor,
                fontWeight,
                fontStyle);

            line.AddSegment(segment);

            TrimHistory();

            RequestRefresh(true);
        }

        public void WriteLine(string text, Color? textColor, FontWeight fontWeight, FontStyle fontStyle)
        {
            TerminalLine line = GetCurrentOutputLine();

            TerminalLineSegment segment = new(
                text,
                textColor ?? DefaultColor,
                fontWeight,
                fontStyle);

            line.AddSegment(segment);
            _startNewOutputLine = true;

            TrimHistory();

            RequestRefresh(true);
        }

        public void WriteLines(IEnumerable<string> lines, Color? textColor, FontWeight fontWeight, FontStyle fontStyle)
        {
            foreach (string text in lines)
            {
                TerminalLine line = new();

                line.AddSegment(
                    new TerminalLineSegment(
                        text,
                        textColor ?? DefaultColor,
                        fontWeight,
                        fontStyle));

                _completedLines.Add(line);
            }

            _startNewOutputLine = true;

            TrimHistory();

            RefreshTerminal(true);
        }

        public void Clear()
        {
            _completedLines.Clear();
            _currentLine.Clear();
            _startNewOutputLine = true;

            RequestRefresh(true);
        }

        private void TrimHistory()
        {
            int totalCount = _completedLines.Count;
            int overflow = totalCount - MaxHistorySize;

            if (totalCount > MaxHistorySize)
            {
                _completedLines.RemoveRange(0, overflow);
            }
        }

        private void RefreshTerminal(bool scrollToBottom)
        {
            UpdateScrollView();
            Terminal.Invalidate();

            if (!scrollToBottom)
            {
                return;
            }

            DispatcherQueue.TryEnqueue(() =>
            {
                TerminalScrollView.UpdateLayout();

                TerminalScrollView.ChangeView(
                    null,
                    TerminalScrollView.ScrollableHeight,
                    null,
                    true);
            });
        }

        private void RequestRefresh(bool scrollToBottom)
        {
            _scrollToBottomPending |= scrollToBottom;

            if (_refreshPending)
            {
                return;
            }

            _refreshPending = true;

            DispatcherQueue.TryEnqueue(() =>
            {
                _refreshPending = false;

                bool shouldScrollToBottom = _scrollToBottomPending;
                _scrollToBottomPending = false;

                RefreshTerminal(shouldScrollToBottom);
            });
        }
        #endregion

        private void DispatcherTimer_Tick(object? sender, object e)
        {
            if (_isCommandRunning)
            {
                return;
            }

            _caretVisible = !_caretVisible;
            Terminal.Invalidate();
        }

        private void Terminal_RegionsInvalidated(CanvasVirtualControl sender, CanvasRegionsInvalidatedEventArgs eventArgs)
        {
            float lineHeight = _textFormat.FontSize + 4;

            float caretHeight = _textFormat.FontSize;
            float caretOffsetY = (lineHeight - caretHeight) / 2;

            foreach (var region in eventArgs.InvalidatedRegions)
            {
                using var drawingSession = sender.CreateDrawingSession(region);

                int firstVisibleLine = Math.Max(0, (int)Math.Floor((region.Top - PaddingTop) / lineHeight) - 1);

                int lastVisibleLine = Math.Min(_completedLines.Count - 1, (int)Math.Ceiling((region.Bottom - PaddingTop) / lineHeight) + 1);

                for (int i = firstVisibleLine; i <= lastVisibleLine; i++)
                {
                    TerminalLine line = _completedLines[i];

                    float lineY = PaddingTop + i * lineHeight;

                    float currentX = PaddingLeft;

                    foreach (TerminalLineSegment segment in line.Segments)
                    {
                        _textFormat.FontWeight = segment.FontWeight;
                        _textFormat.FontStyle = segment.FontStyle;

                        drawingSession.DrawText(
                            segment.Text,
                            currentX,
                            lineY,
                            segment.Color,
                            _textFormat);

                        using CanvasTextLayout segmentLayout = new(
                            sender.Device,
                            segment.Text,
                            _textFormat,
                            0,
                            0);

                        float segmentWidth = (float)segmentLayout
                                .LayoutBoundsIncludingTrailingWhitespace
                                .Width;

                        currentX += segmentWidth;
                    }
                }

                if (_isCommandRunning)
                {
                    continue;
                }

                float promptY = PaddingTop + _completedLines.Count * lineHeight;

                bool promptIntersectsRegion = promptY + lineHeight >= region.Top && promptY <= region.Bottom;

                if (!promptIntersectsRegion)
                {
                    continue;
                }

                _textFormat.FontWeight = FontWeights.Normal;
                _textFormat.FontStyle = FontStyle.Normal;

                string currentLine = Prompt + _currentLine;

                drawingSession.DrawText(
                    currentLine,
                    PaddingLeft,
                    promptY,
                    DefaultColor,
                    _textFormat);

                using CanvasTextLayout currentLineLayout = new(
                    sender.Device,
                    currentLine,
                    _textFormat,
                    0,
                    0);

                float currentLineWidth = (float)currentLineLayout
                        .LayoutBoundsIncludingTrailingWhitespace
                        .Width;

                float caretX = PaddingLeft + currentLineWidth;

                if (_caretVisible)
                {
                    drawingSession.DrawLine(
                        caretX,
                        promptY + caretOffsetY,
                        caretX,
                        promptY + caretOffsetY + caretHeight,
                        DefaultColor);
                }
            }
        }

        private void TerminalUserControl_CharacterReceived(UIElement sender, CharacterReceivedRoutedEventArgs eventArgs)
        {
            if (_isCommandRunning || IsKeyDown(VirtualKey.Control))
            {
                return;
            }

            int intChar = (int)eventArgs.Character;

            if (intChar < 32 || intChar == 127)
            {
                return;
            }

            string text = char.ConvertFromUtf32(intChar);

            _currentLine.Append(text);

            Terminal.Invalidate();
            eventArgs.Handled = true;
        }

        private void TerminalUserControl_Loaded(object sender, RoutedEventArgs eventArgs)
        {
            _terminalOutput = new TerminalOutput(
                this,
                () => ActualTheme);

            TerminalCommandDispatcher dispatcher = new(
                TerminalCommandRegistry.CreateDefault());

            _terminalViewModel = new TerminalViewModel(
                dispatcher,
                () => new TerminalCommandContext(
                    _terminalOutput,
                    _tabModel,
                    CancellationToken.None));

            Focus(FocusState.Programmatic);

            _caretVisible = true;
            _dispatcherTimer.Start();

            RefreshTerminal(false);
        }

        private void TerminalUserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            _dispatcherTimer.Stop();
        }

        private void TerminalUserControl_PointerPressed(object sender, PointerRoutedEventArgs eventArgs)
        {
            Focus(FocusState.Pointer);

            _caretVisible = true;
            _dispatcherTimer.Start();

            Terminal.Invalidate();

            eventArgs.Handled = true;
        }

        private void TerminalUserControl_LostFocus(object sender, RoutedEventArgs e)
        {
            _dispatcherTimer.Stop();
            _caretVisible = false;
            Terminal.Invalidate();
        }

        private async void TerminalUserControl_KeyDown(object sender, KeyRoutedEventArgs eventArgs)
        {
            bool isCtrlPressed = IsKeyDown(VirtualKey.Control);

            if (isCtrlPressed)
            {
                if (eventArgs.OriginalKey == OemPlus ||
                    eventArgs.Key == VirtualKey.Add)
                {
                    _textFormat.FontSize = Math.Min(_textFormat.FontSize + 1, MaxFontSize);

                    RefreshTerminal(false);
                    eventArgs.Handled = true;
                    return;
                }

                if (eventArgs.OriginalKey == OemMinus ||
                    eventArgs.Key == VirtualKey.Subtract)
                {
                    _textFormat.FontSize = Math.Max(_textFormat.FontSize - 1, MinFontSize);

                    RefreshTerminal(false);
                    eventArgs.Handled = true;
                    return;
                }

                if (eventArgs.Key == VirtualKey.C)
                {
                    _terminalViewModel?.Cancel();
                    eventArgs.Handled = true;
                    return;
                }
            }

            if (_isCommandRunning)
            {
                eventArgs.Handled = true;
                return;
            }

            switch (eventArgs.Key)
            {
                case VirtualKey.Enter:
                    {
                        string currentLine = _currentLine.ToString();
                        string completeLine = Prompt + currentLine;

                        TerminalLineSegment segment = new(
                            completeLine,
                            DefaultColor,
                            FontWeights.Normal,
                            FontStyle.Normal);

                        TerminalLine terminalLine = new();
                        terminalLine.AddSegment(segment);

                        _completedLines.Add(terminalLine);

                        TrimHistory();

                        _startNewOutputLine = true;

                        _currentLine.Clear();
                        _isCommandRunning = true;
                        _caretVisible = false;

                        RefreshTerminal(true);

                        try
                        {
                            await _terminalViewModel!.SubmitAsync(currentLine);
                        }
                        finally
                        {
                            _isCommandRunning = false;
                            _caretVisible = true;

                            RefreshTerminal(true);
                        }

                        eventArgs.Handled = true;
                        break;
                    }

                case VirtualKey.Back:
                    {
                        if (_currentLine.Length == 0)
                        {
                            eventArgs.Handled = true;
                            return;
                        }

                        _currentLine.Remove(_currentLine.Length - 1, 1);

                        Terminal.Invalidate();
                        eventArgs.Handled = true;
                        break;
                    }
            }
        }

        private void TerminalUserControl_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            if (!IsKeyDown(VirtualKey.Control))
            {
                return;
            }

            PointerPoint pointerPoint = e.GetCurrentPoint(Terminal);
            PointerPointProperties pointerPointProperties = pointerPoint.Properties;

            int mouseWheelDelta = pointerPointProperties.MouseWheelDelta;

            if (mouseWheelDelta > 0)
            {
                _textFormat.FontSize = Math.Min(_textFormat.FontSize + 1, MaxFontSize);
            }
            else if (mouseWheelDelta < 0)
            {
                _textFormat.FontSize = Math.Max(_textFormat.FontSize - 1, MinFontSize);
            }

            RefreshTerminal(false);
            e.Handled = true;
        }
    }
}
