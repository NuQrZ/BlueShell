using System.Collections.Generic;

namespace BlueShell.Model
{
    public sealed class TerminalLine
    {
        public readonly List<TerminalLineSegment> Segments = [];

        public void AddSegment(TerminalLineSegment lineSegment)
        {
            Segments.Add(lineSegment);
        }
    }
}
