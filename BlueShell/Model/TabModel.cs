using System;

namespace BlueShell.Model
{
    public class TabModel
    {
        public Guid Guid { get; set; } = Guid.NewGuid();
        public string TabHeader { get; set; } = "Terminal";
        public string IconPath { get; set; } = "ms-appx:///Assets/Icons/Terminal.ico";
    }
}
