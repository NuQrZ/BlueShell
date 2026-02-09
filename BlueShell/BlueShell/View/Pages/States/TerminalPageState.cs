using BlueShell.Model;
using System.Collections.Generic;

namespace BlueShell.View.Pages.States
{
    public sealed class TerminalPageState
    {
        public List<TerminalLine> Lines { get; set; } = [];
        public List<DataDisplayItem> DisplayItems { get; set; } = [];

        public int InputStart { get; set; }
        public int EnterCount { get; set; }
        public int TextSelectorPosition { get; set; }

        public double TerminalWidthStar { get; set; } = 2;
        public double DisplayWidthStar { get; set; } = 3;

        public bool SplitView { get; set; } = true;
    }
}
