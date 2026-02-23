using Microsoft.UI.Xaml.Media;
using System.Collections.Generic;
using System.Linq;

namespace BlueShell.Model
{
    public sealed class OutputLine
    {
        public IReadOnlyList<OutputSegment> Segments => _segments;
        private readonly List<OutputSegment> _segments = [];

        public string Text => string.Concat(_segments.Select(s => s.Text));

        public SolidColorBrush? Foreground => _segments.Count > 0 ? _segments[0].Color : null;

        public bool IsRich => _segments.Count > 1;

        public void AddSegment(OutputSegment outputSegment)
        {
            _segments.Add(outputSegment);
        }
    }
}
