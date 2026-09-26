using System.Collections.Generic;
using Windows.Foundation;
using Windows.UI;

namespace BlueShell.Model.Settings
{
    public sealed class GradientBackground : AppBackground
    {
        public required List<Color> Colors { get; init; }
        public required Point StartPoint { get; init; }
        public required Point EndPoint { get; init; }
    }
}
