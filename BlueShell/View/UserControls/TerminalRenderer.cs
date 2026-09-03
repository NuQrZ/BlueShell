using BlueShell.Helpers;
using BlueShell.Model;
using BlueShell.Terminal;
using BlueShell.ViewModel;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.UI;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using System;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Text;

namespace BlueShell.View.UserControls
{
    public sealed class TerminalRenderer
    {
        private const float MinFontSize = 8;
        private const float MaxFontSize = 48;

        private readonly CanvasTextFormat _textFormat = new()
        {
            FontFamily = "Cascadia Code",
            FontSize = 18,
            WordWrapping = CanvasWordWrapping.NoWrap,
        };
        private readonly TerminalBuffer _terminalBuffer;
        private readonly TerminalViewModel _terminalViewModel;
        private readonly Func<ElementTheme> _themeProvider;

        public const float PaddingLeft = 8;
        public const float PaddingTop = 8;
        public const string Prompt = "Shell > ";

        public bool CaretVisible { get; set; } = true;
        public float FontSize
        {
            get => _textFormat.FontSize;
            set => _textFormat.FontSize = value;
        }
        public float LineHeight => _textFormat.FontSize + 4;
        public Color DefaultColor
        {
            get
            {
                ElementTheme theme = GetCurrentTheme();

                return theme == ElementTheme.Light ? Colors.Black : Colors.White;
            }
        }

        public TerminalRenderer(TerminalBuffer terminalBuffer, TerminalViewModel terminalViewModel, Func<ElementTheme> themeProvider)
        {
            _terminalBuffer = terminalBuffer;
            _terminalViewModel = terminalViewModel;
            _themeProvider = themeProvider;
        }

        public void Draw(CanvasVirtualControl sender, CanvasRegionsInvalidatedEventArgs eventArgs)
        {
            float lineHeight = _textFormat.FontSize + 4;

            foreach (var region in eventArgs.InvalidatedRegions)
            {
                using var drawingSession = sender.CreateDrawingSession(region);

                int firstVisibleLine = GetFirstVisibleLine(region, lineHeight);
                int lastVisibleLine = GetLastVisibleLine(region, lineHeight);

                for (int i = firstVisibleLine; i <= lastVisibleLine; i++)
                {
                    TerminalLine line = _terminalBuffer.Lines[i];

                    float lineY = PaddingTop + i * lineHeight;

                    DrawTerminalLine(sender, drawingSession, line, lineY);
                }

                if (_terminalViewModel!.IsCommandRunning)
                {
                    continue;
                }

                DrawCurrentLine(sender, drawingSession, region, lineHeight);
            }
        }

        public void ZoomIn()
        {
            FontSize = Math.Min(FontSize + 1, MaxFontSize);
        }

        public void ZoomOut()
        {
            FontSize = Math.Max(FontSize - 1, MinFontSize);
        }

        private void DrawTerminalLine(CanvasVirtualControl sender, CanvasDrawingSession drawingSession, TerminalLine line, float lineY)
        {
            float currentX = PaddingLeft;

            foreach (TerminalLineSegment segment in line.Segments)
            {
                _textFormat.FontWeight = segment.FontWeight;
                _textFormat.FontStyle = segment.FontStyle;

                drawingSession.DrawText(
                    segment.Text,
                    currentX,
                    lineY,
                    segment.Color ?? DefaultColor,
                    _textFormat);

                float segmentWidth = MeasureTextWidth(sender, segment.Text);

                currentX += segmentWidth;
            }
        }

        private float MeasureTextWidth(CanvasVirtualControl sender, string text)
        {
            using CanvasTextLayout layout = new(sender.Device, text, _textFormat, 0, 0);

            return (float)layout.LayoutBoundsIncludingTrailingWhitespace.Width;
        }

        private static int GetFirstVisibleLine(Rect region, float lineHeight)
        {
            return Math.Max(
                0,
                (int)Math.Floor((region.Top - PaddingTop) / lineHeight) - 1);
        }

        private int GetLastVisibleLine(Rect region, float lineHeight)
        {
            int completedLinesCount = _terminalBuffer.Count;

            return Math.Min(
                completedLinesCount - 1,
                (int)Math.Ceiling((region.Bottom - PaddingTop) / lineHeight) + 1);
        }

        private void DrawCurrentLine(CanvasVirtualControl sender, CanvasDrawingSession drawingSession, Rect region, float lineHeight)
        {
            float caretHeight = _textFormat.FontSize;
            float caretOffsetY = (lineHeight - caretHeight) / 2;

            float promptY = PaddingTop + _terminalBuffer.Count * lineHeight;

            _textFormat.FontWeight = FontWeights.Normal;
            _textFormat.FontStyle = FontStyle.Normal;

            bool promptIntersectsRegion =
                promptY + lineHeight >= region.Top &&
                promptY <= region.Bottom;

            if (!promptIntersectsRegion)
            {
                return;
            }

            float promptWidth = MeasureTextWidth(sender, Prompt);

            DrawPrompt(drawingSession, promptY);

            DrawText(drawingSession, promptY, promptWidth);

            DrawCaret(sender, drawingSession, promptY, promptWidth, caretOffsetY, caretHeight);
        }

        private void DrawPrompt(CanvasDrawingSession drawingSession, float promptY)
        {
            drawingSession.DrawText(Prompt, PaddingLeft, promptY, DefaultColor, _textFormat);
        }

        private void DrawText(CanvasDrawingSession drawingSession, float promptY, float promptWidth)
        {
            ElementTheme theme = _themeProvider();

            if (theme == ElementTheme.Default)
            {
                theme = Application.Current.RequestedTheme == ApplicationTheme.Dark
                    ? ElementTheme.Dark
                    : ElementTheme.Light;
            }

            string currentInput = _terminalViewModel!.CurrentLine;

            drawingSession.DrawText(
                currentInput,
                PaddingLeft + promptWidth,
                promptY,
                TerminalUtilities.GetCommandColor(currentInput, GetCurrentTheme(), DefaultColor),
                _textFormat);
        }

        private void DrawCaret(CanvasVirtualControl sender, CanvasDrawingSession drawingSession, float promptY, float promptWidth, float caretOffsetY, float caretHeight)
        {
            if (!CaretVisible)
            {
                return;
            }

            string currentInput = _terminalViewModel!.CurrentLine;

            string textBeforeCaret = currentInput[.._terminalViewModel!.CaretPosition];

            float textBeforeCaretWidth = MeasureTextWidth(sender, textBeforeCaret);

            float caretX = PaddingLeft + promptWidth + textBeforeCaretWidth;

            drawingSession.DrawLine(
                caretX,
                promptY + caretOffsetY,
                caretX,
                promptY + caretOffsetY + caretHeight,
                DefaultColor);
        }

        private ElementTheme GetCurrentTheme()
        {
            ElementTheme theme = _themeProvider();

            if (theme == ElementTheme.Default)
            {
                theme = Application.Current.RequestedTheme == ApplicationTheme.Dark
                    ? ElementTheme.Dark
                    : ElementTheme.Light;
            }

            return theme;
        }
    }
}
