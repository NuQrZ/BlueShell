using BlueShell.Model;
using BlueShell.Terminal.Abstractions;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml.Media;
using Windows.UI;
using Windows.UI.Text;

namespace BlueShell.Terminal.WinUI
{
    public sealed class LineBuilder
    {
        private readonly TerminalOutput _terminalOutput;
        private readonly OutputLine _outputLine = new();

        internal LineBuilder(TerminalOutput terminalOutput)
        {
            _terminalOutput = terminalOutput;
        }

        private LineBuilder Add(
            string text,
            SolidColorBrush foregroundColor,
            FontStyle fontStyle = FontStyle.Normal,
            FontWeight? fontWeight = null)
        {
            OutputSegment outputSegment = new()
            {
                Text = text,
                Color = foregroundColor,
                FontStyle = fontStyle,
                FontWeight = fontWeight ?? FontWeights.Normal
            };

            _outputLine.AddSegment(outputSegment);

            return this;
        }

        public LineBuilder Error(
            string text,
            FontWeight? fontWeight = null,
            FontStyle fontStyle = FontStyle.Normal)
        {
            SolidColorBrush foregroundColor = _terminalOutput.GetBrush(TerminalMessageKind.Error);
            return Add(text, foregroundColor, fontStyle, fontWeight);
        }

        public LineBuilder Warning(
            string text,
            FontWeight? fontWeight = null,
            FontStyle fontStyle = FontStyle.Normal)
        {
            SolidColorBrush foregroundColor = _terminalOutput.GetBrush(TerminalMessageKind.Warning);
            return Add(text, foregroundColor, fontStyle, fontWeight);
        }

        public LineBuilder Success(
            string text,
            FontWeight? fontWeight = null,
            FontStyle fontStyle = FontStyle.Normal)
        {
            SolidColorBrush foregroundColor = _terminalOutput.GetBrush(TerminalMessageKind.Success);
            return Add(text, foregroundColor, fontStyle, fontWeight);
        }

        public LineBuilder Info(
            string text,
            FontWeight? fontWeight = null,
            FontStyle fontStyle = FontStyle.Normal)
        {
            SolidColorBrush foregroundColor = _terminalOutput.GetBrush(TerminalMessageKind.Info);
            return Add(text, foregroundColor, fontStyle, fontWeight);
        }

        public LineBuilder PrintOutput(
            string text,
            FontWeight? fontWeight = null,
            FontStyle fontStyle = FontStyle.Normal)
        {
            SolidColorBrush foregroundColor = _terminalOutput.GetBrush(TerminalMessageKind.PrintOutput);

            return Add(text, foregroundColor, fontStyle, fontWeight);
        }

        public LineBuilder PrintOutput(
            string text,
            Color extraColor,
            FontWeight? fontWeight = null,
            FontStyle fontStyle = FontStyle.Normal)
        {
            SolidColorBrush foregroundColor = _terminalOutput.GetBrush(extraColor);

            return Add(text, foregroundColor, fontStyle, fontWeight);
        }

        public void Commit()
        {
            _terminalOutput.AddLine(_outputLine);
        }
    }
}
