using BlueShell.Model.Terminal;
using System;
using System.Collections.Generic;
using Windows.UI;
using Windows.UI.Text;

namespace BlueShell.Terminal
{
    public sealed class TerminalBuffer
    {
        private const int MaxHistorySize = 100_010;

        private bool _startNewOutputLine = true;

        private readonly List<TerminalLine> _completedLines = [];

        public event EventHandler? Changed;

        public int Count => _completedLines.Count;

        public IReadOnlyList<TerminalLine> Lines => _completedLines;

        public void AddTerminalLine(TerminalLine line)
        {
            _completedLines.Add(line);
            _startNewOutputLine = true;
            TrimHistory();

            OnChanged();
        }

        public void Write(string text, Color? textColor, FontWeight fontWeight, FontStyle fontStyle)
        {
            TerminalLine line = GetCurrentOutputLine();

            TerminalLineSegment segment = new(
                text,
                textColor,
                fontWeight,
                fontStyle);

            line.AddSegment(segment);

            TrimHistory();

            OnChanged();
        }

        public void WriteLine(string text, Color? textColor, FontWeight fontWeight, FontStyle fontStyle)
        {
            TerminalLine line = GetCurrentOutputLine();

            TerminalLineSegment segment = new(
                text,
                textColor,
                fontWeight,
                fontStyle);

            line.AddSegment(segment);
            _startNewOutputLine = true;

            TrimHistory();

            OnChanged();
        }

        public void WriteLines(IEnumerable<string> lines, Color? textColor, FontWeight fontWeight, FontStyle fontStyle)
        {
            foreach (string text in lines)
            {
                TerminalLine line = new();

                line.AddSegment(
                    new TerminalLineSegment(
                        text,
                        textColor,
                        fontWeight,
                        fontStyle));

                _completedLines.Add(line);
            }

            _startNewOutputLine = true;

            TrimHistory();

            OnChanged();
        }

        public void Clear()
        {
            _completedLines.Clear();

            _startNewOutputLine = true;

            OnChanged();
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

        private void OnChanged()
        {
            Changed?.Invoke(this, EventArgs.Empty);
        }
    }
}
