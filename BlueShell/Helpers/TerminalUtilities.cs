using Microsoft.UI.Xaml;
using System.Collections.Generic;
using Windows.UI;

namespace BlueShell.Helpers
{
    public static class TerminalUtilities
    {
        public static Dictionary<string, Color> DarkThemeKeywordColors { get; } = new()
        {
            { "Exit", Color.FromArgb(255, 255, 80, 80) },
            { "Clear", Color.FromArgb(255, 0, 255, 200) },
            { "--Version", Color.FromArgb(255, 255, 185, 0) },
        };

        public static Dictionary<string, Color> LightThemeKeywordColors { get; } = new()
        {
            { "Exit", Color.FromArgb(255, 210, 30, 30) },
            { "Clear", Color.FromArgb(255, 0, 170, 140) },
            { "--Version", Color.FromArgb(255, 190, 125, 0) },
        };

        public static Color GetCommandColor(string command, ElementTheme elementTheme, Color defaultColor)
        {
            Dictionary<string, Color> colors =
                elementTheme == ElementTheme.Light
                    ? LightThemeKeywordColors
                    : DarkThemeKeywordColors;

            return colors.TryGetValue(
                command,
                out Color color)
                    ? color
                    : defaultColor;
        }

        public static string GetCommandName(string input)
        {
            int separatorIndex = input.IndexOf(' ');

            if (separatorIndex < 0)
            {
                return input;
            }

            return input[..separatorIndex];
        }
    }
}
