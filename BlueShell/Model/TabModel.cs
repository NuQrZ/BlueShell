using System;

namespace BlueShell.Model
{
    public sealed class TabModel
    {
        public Guid Guid { get; set; } = Guid.NewGuid();
        public int TabIndex { get; set; }
        public string TabHeader { get; set; } = "Terminal";
        public string IconPath { get; set; } = "ms-appx:///Assets/Icons/Terminal.ico";
    }
}
