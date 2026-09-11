using BlueShell.Model.Terminal;
using System;
using System.Collections.Generic;
using Windows.UI;
using Windows.UI.Text;

namespace BlueShell.Terminal
{
    public sealed class TerminalBuffer
    {
        private const int MaxLineCount = 100_000;
        private const int TrimLineCount = 10_000;

        private bool _startNewOutputLine = true;

        private readonly List<TerminalLine> _lines = [];

        public event EventHandler? Changed;

        public int Count => _lines.Count;

        public IReadOnlyList<TerminalLine> Lines => _lines;

        public void AddTerminalLine(TerminalLine line)
        {
            _lines.Add(line);
            _startNewOutputLine = true;
            TrimBuffer();

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

            TrimBuffer();

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

            TrimBuffer();

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

                _lines.Add(line);
            }

            _startNewOutputLine = true;

            TrimBuffer();

            OnChanged();
        }

        public void Clear()
        {
            _lines.Clear();

            _startNewOutputLine = true;

            OnChanged();
        }

        private void TrimBuffer()
        {
            if (_lines.Count <= MaxLineCount)
            {
                return;
            }

            int overflow = _lines.Count - MaxLineCount;
            int removeCount = Math.Max(overflow, TrimLineCount);

            _lines.RemoveRange(0, Math.Min(removeCount, _lines.Count));
        }

        private TerminalLine GetCurrentOutputLine()
        {
            if (_startNewOutputLine || _lines.Count == 0)
            {
                TerminalLine line = new();

                _lines.Add(line);
                _startNewOutputLine = false;

                return line;
            }

            return _lines[^1];
        }

        private void OnChanged()
        {
            Changed?.Invoke(this, EventArgs.Empty);
        }
    }
}
