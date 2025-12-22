using System.Collections.Generic;
using Windows.UI;

namespace BlueShell.Core
{
    public static class Utilities
    {
        public static Dictionary<string, Color> DarkThemeKeywordColors { get; } = new()
        {
            { "Exit", Color.FromArgb(255, 255, 69, 58) },
            { "Clear", Color.FromArgb(255, 0, 206, 209) },
            { "ClearDisplay", Color.FromArgb(255, 169, 169, 169) },
            { "--Version", Color.FromArgb(255, 255, 193, 37) },
            { "Drive", Color.FromArgb(255, 30, 144, 255) },
            { "-GetDrives", Color.FromArgb(255, 255, 239, 161) },
            { "-Properties", Color.FromArgb(255, 186, 85, 211) },
            { "-Advanced", Color.FromArgb(255, 123, 104, 238) },
            { "-Open", Color.FromArgb(255, 60, 179, 113) }
        };

        public static Dictionary<string, Color> LightThemeKeywordColors { get; } = new()
        {
            { "Exit", Color.FromArgb(255, 255, 69, 0) },
            { "Clear", Color.FromArgb(255, 0, 139, 139) },
            { "ClearDisplay", Color.FromArgb(255, 105, 105, 105) },
            { "--Version", Color.FromArgb(255, 184, 134, 11) },
            { "Drive", Color.FromArgb(255, 25, 55, 212) },
            { "-GetDrives", Color.FromArgb(255, 189, 169, 60) },
            { "-Properties", Color.FromArgb(255, 186, 85, 211) },
            { "-Advanced", Color.FromArgb(255, 65, 105, 225) },
            { "-Open", Color.FromArgb(255, 50, 205, 50) }
        };
    }
}