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
    public sealed class TerminalRenderer(TerminalBuffer terminalBuffer, TerminalViewModel terminalViewModel, Func<ElementTheme> themeProvider)
    {
        private const float MinFontSize = 8;
        private const float MaxFontSize = 48;

        private readonly CanvasTextFormat _textFormat = new()
        {
            FontFamily = "Cascadia Code",
            FontSize = 18,
            WordWrapping = CanvasWordWrapping.NoWrap,
        };
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
                    TerminalLine line = terminalBuffer.Lines[i];

                    float lineY = PaddingTop + i * lineHeight;

                    DrawTerminalLine(sender, drawingSession, line, lineY);
                }

                if (terminalViewModel!.IsCommandRunning)
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
            int completedLinesCount = terminalBuffer.Count;

            return Math.Min(
                completedLinesCount - 1,
                (int)Math.Ceiling((region.Bottom - PaddingTop) / lineHeight) + 1);
        }

        private void DrawCurrentLine(CanvasVirtualControl sender, CanvasDrawingSession drawingSession, Rect region, float lineHeight)
        {
            float caretHeight = _textFormat.FontSize;
            float caretOffsetY = (lineHeight - caretHeight) / 2;

            float promptY = PaddingTop + terminalBuffer.Count * lineHeight;

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

            DrawSelection(sender, drawingSession, promptY, promptWidth, lineHeight);

            DrawText(sender, drawingSession, promptY, promptWidth);

            DrawCaret(sender, drawingSession, promptY, promptWidth, caretOffsetY, caretHeight);
        }

        private void DrawPrompt(CanvasDrawingSession drawingSession, float promptY)
        {
            drawingSession.DrawText(Prompt, PaddingLeft, promptY, DefaultColor, _textFormat);
        }

        private void DrawSelection(CanvasVirtualControl sender, CanvasDrawingSession drawingSession, float promptY, float promptWidth, float lineHeight)
        {
            if (!terminalViewModel.HasSelection)
            {
                return;
            }

            string currentInput = terminalViewModel.CurrentLine;

            string textBeforeSelection = currentInput[..terminalViewModel.SelectionStart];
            string selectedText = currentInput[terminalViewModel.SelectionStart..terminalViewModel.SelectionEnd];

            float textBeforeSelectionWidth = MeasureTextWidth(sender, textBeforeSelection);
            float selectedTextWidth = MeasureTextWidth(sender, selectedText);

            float selectionX = PaddingLeft + promptWidth + textBeforeSelectionWidth;

            Rect selectionRect = new(selectionX, promptY, selectedTextWidth, lineHeight);

            drawingSession.DrawRectangle(selectionRect, Colors.DodgerBlue);

            drawingSession.FillRectangle(selectionRect, Colors.DodgerBlue);
        }

        private void DrawText(CanvasVirtualControl sender, CanvasDrawingSession drawingSession, float promptY, float promptWidth)
        {
            string currentInput = terminalViewModel!.CurrentLine;
            float currentX = PaddingLeft + promptWidth;

            if (terminalViewModel.HasSelection)
            {
                string textBeforeSelection = currentInput[..terminalViewModel.SelectionStart];
                string selectedText = currentInput[terminalViewModel.SelectionStart..terminalViewModel.SelectionEnd];
                string textAfterSelection = currentInput[terminalViewModel.SelectionEnd..];

                float textBeforeSelectionWidth = MeasureTextWidth(sender, textBeforeSelection);
                float selectedTextWidth = MeasureTextWidth(sender, selectedText);

                drawingSession.DrawText(textBeforeSelection, currentX, promptY, DefaultColor, _textFormat);
                currentX += textBeforeSelectionWidth;

                drawingSession.DrawText(selectedText, currentX, promptY, GetSelectionColor(), _textFormat);
                currentX += selectedTextWidth;

                drawingSession.DrawText(textAfterSelection, currentX, promptY, DefaultColor, _textFormat);
            }
            else
            {
                drawingSession.DrawText(
                    currentInput,
                    PaddingLeft + promptWidth,
                    promptY,
                    TerminalUtilities.GetCommandColor(currentInput, GetCurrentTheme(), DefaultColor),
                    _textFormat);
            }
        }

        private void DrawCaret(CanvasVirtualControl sender, CanvasDrawingSession drawingSession, float promptY, float promptWidth, float caretOffsetY, float caretHeight)
        {
            if (!CaretVisible)
            {
                return;
            }

            string currentInput = terminalViewModel!.CurrentLine;

            string textBeforeCaret = currentInput[..terminalViewModel!.CaretPosition];

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
            ElementTheme theme = themeProvider();

            if (theme == ElementTheme.Default)
            {
                theme = Application.Current.RequestedTheme == ApplicationTheme.Dark
                    ? ElementTheme.Dark
                    : ElementTheme.Light;
            }

            return theme;
        }

        private Color GetSelectionColor()
        {
            ElementTheme theme = GetCurrentTheme();

            if (theme == ElementTheme.Default)
            {
                theme = Application.Current.RequestedTheme == ApplicationTheme.Dark
                    ? ElementTheme.Dark
                    : ElementTheme.Light;
            }

            return theme == ElementTheme.Dark ? Colors.Black : Colors.White;
        }
    }
}
