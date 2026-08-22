using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.UI;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Collections.Generic;
using System.Text;
using Windows.System;
using Windows.UI;

namespace BlueShell.View.UserControls
{
    public sealed partial class CanvasTerminal : UserControl
    {
        private const string Prompt = "BlueShell > ";
        private const VirtualKey OemPlus = (VirtualKey)0xBB;
        private const VirtualKey OemMinus = (VirtualKey)0xBD;

        private List<string> _completedLines = [];
        private StringBuilder _currentLine = new();
        private CanvasTextFormat _textFormat = new()
        {
            FontFamily = "Cascadia Code",
            FontSize = 18,
            WordWrapping = CanvasWordWrapping.NoWrap
        };

        private bool _caretVisible = true;
        private readonly DispatcherTimer _dispatcherTimer = new()
        {
            Interval = TimeSpan.FromMilliseconds(400)
        };

        public CanvasTerminal()
        {
            InitializeComponent();

            _dispatcherTimer.Tick += DispatcherTimer_Tick;
        }

        private void UpdateScrollView()
        {
            float lineHeight = _textFormat.FontSize + 4;

            double contentHeight = 8 + ((_completedLines.Count + 1) * lineHeight) + lineHeight;

            Terminal.Height = Math.Max(TerminalScrollView.ViewportHeight, contentHeight);
        }

        private void DispatcherTimer_Tick(object? sender, object e)
        {
            _caretVisible = !_caretVisible;

            Terminal.Invalidate();
        }

        private void Terminal_Draw(CanvasControl sender, CanvasDrawEventArgs eventArgs)
        {
            const float x = 8;

            float lineHeight = _textFormat.FontSize + 4;
            float caretHeight = _textFormat.FontSize;
            float caretOffsetY = (lineHeight - caretHeight) / 2;

            float y = 8;

            Color textColor =
                ActualTheme == ElementTheme.Light
                    ? Colors.Black
                    : Colors.White;

            foreach (string completedLine in _completedLines)
            {
                eventArgs.DrawingSession.DrawText(
                    completedLine,
                    x,
                    y,
                    textColor,
                    _textFormat);

                y += lineHeight;
            }

            string currentLine = Prompt + _currentLine;

            eventArgs.DrawingSession.DrawText(
                currentLine,
                x,
                y,
                textColor,
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

            float caretX = x + currentLineWidth;

            if (_caretVisible)
            {
                eventArgs.DrawingSession.DrawLine(
                    caretX,
                    y + caretOffsetY,
                    caretX,
                    y + caretOffsetY + caretHeight,
                    textColor);
            }
        }

        private void TerminalUserControl_CharacterReceived(UIElement sender, CharacterReceivedRoutedEventArgs eventArgs)
        {
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
            Focus(FocusState.Programmatic);

            _caretVisible = true;
            _dispatcherTimer.Start();

            Terminal.Invalidate();
        }

        private void TerminalUserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            _dispatcherTimer.Stop();
            Terminal.Invalidate();
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
            Terminal.Invalidate();
        }

        private void TerminalUserControl_KeyDown(object sender, KeyRoutedEventArgs eventArgs)
        {
            bool isCtrlPressed = (InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Control)
                & Windows.UI.Core.CoreVirtualKeyStates.Down) != 0;

            if (eventArgs.Key == VirtualKey.Control)
            {
                eventArgs.Handled = true;
                return;
            }

            if (isCtrlPressed)
            {
                if (eventArgs.Key == OemPlus ||
                    eventArgs.Key == VirtualKey.Add)
                {
                    _textFormat.FontSize += 1;

                    Terminal.Invalidate();
                    eventArgs.Handled = true;
                    return;
                }

                if (eventArgs.Key == OemMinus ||
                    eventArgs.Key == VirtualKey.Subtract)
                {
                    if (_textFormat.FontSize > 1)
                    {
                        _textFormat.FontSize -= 1;
                    }
                    else
                    {
                        return;
                    }

                    Terminal.Invalidate();
                    eventArgs.Handled = true;
                    return;
                }
            }

            switch (eventArgs.Key)
            {
                case VirtualKey.Enter:
                    string currentLine = _currentLine.ToString();
                    string completeLine = Prompt + currentLine;
                    _completedLines.Add(completeLine);

                    _currentLine.Clear();

                    UpdateScrollView();

                    Terminal.Invalidate();

                    DispatcherQueue.TryEnqueue(() =>
                    {
                        TerminalScrollView.ChangeView(
                            null,
                            TerminalScrollView.ScrollableHeight,
                            null);
                    });

                    eventArgs.Handled = true;
                    break;
                case VirtualKey.Back:
                    int length = _currentLine.Length;

                    if (_currentLine.Length == 0)
                    {
                        return;
                    }

                    _currentLine.Remove(length - 1, 1);

                    Terminal.Invalidate();
                    break;
                default:
                    break;
            }
        }

        private void TerminalUserControl_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            bool isCtrlPressed = (InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Control)
                & Windows.UI.Core.CoreVirtualKeyStates.Down) != 0;

            if (!isCtrlPressed)
            {
                return;
            }

            PointerPoint pointerPoint = e.GetCurrentPoint(Terminal);
            PointerPointProperties pointerPointProperties = pointerPoint.Properties;

            int mouseWheelDelta = pointerPointProperties.MouseWheelDelta;

            if (mouseWheelDelta > 0)
            {
                _textFormat.FontSize += 1;
            }
            else
            {
                _textFormat.FontSize -= 1;
            }

            Terminal.Invalidate();
        }
    }
}
