using Microsoft.UI;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Windows.UI;

namespace BlueShell.Helpers
{
    public static partial class TerminalUtilities
    {
        private static readonly Regex QuoteRegex = GeneratedQuoteRegex();

        [GeneratedRegex("\"(.*?)\"", RegexOptions.Compiled)]
        private static partial Regex GeneratedQuoteRegex();

        private static Color GetStringColor(ElementTheme elementTheme)
        {
            return elementTheme == ElementTheme.Light
                ? Color.FromArgb(255, 186, 85, 211)
                : Colors.DarkOrange;
        }

        private static Color GetDefaultColor(ElementTheme elementTheme)
        {
            return elementTheme == ElementTheme.Light
                ? Colors.Black
                : Colors.White;
        }

        private static Dictionary<string, Color> GetKeywordColors(ElementTheme elementTheme)
        {
            return elementTheme == ElementTheme.Light
                ? Utilities.LightThemeKeywordColors
                : Utilities.DarkThemeKeywordColors;
        }

        private static bool IsInsideQuotes(int index, List<(int start, int end)> ranges)
        {
            foreach (var (start, end) in ranges)
            {
                if (index >= start && index < end)
                {
                    return true;
                }
            }

            return false;
        }

        public static void HighlightCurrentInput(RichEditTextDocument textDocument, int inputStart, ElementTheme elementTheme)
        {
            int end = textDocument.GetRange(0, int.MaxValue).EndPosition;

            if (end <= inputStart)
            {
                return;
            }

            ITextRange inputRange = textDocument.GetRange(inputStart, end);

            inputRange.GetText(TextGetOptions.None, out string inputText);
            inputText = inputText.TrimEnd('\r', '\n');

            if (string.IsNullOrEmpty(inputText))
            {
                return;
            }

            inputRange.CharacterFormat.ForegroundColor = GetDefaultColor(elementTheme);
            inputRange.CharacterFormat.Bold = FormatEffect.Off;

            Dictionary<string, Color> keywordColors = GetKeywordColors(elementTheme);
            Color stringColor = GetStringColor(elementTheme);

            List<(int start, int end)> quotedRanges = [];
            foreach (Match match in QuoteRegex.Matches(inputText))
            {
                quotedRanges.Add((match.Index, match.Index + match.Length));

                ITextRange stringRange =
                    textDocument.GetRange(inputStart + match.Index, inputStart + match.Index + match.Length);
                stringRange.CharacterFormat.ForegroundColor = stringColor;
                stringRange.CharacterFormat.Bold = FormatEffect.On;
            }

            foreach (KeyValuePair<string, Color> keywordColor in keywordColors)
            {
                string keyword = keywordColor.Key;
                Color color = keywordColor.Value;

                string pattern = keyword.StartsWith('-')
                    ? @$"(?<!\S){Regex.Escape(keyword)}(?!\S)"
                    : @$"\b{Regex.Escape(keyword)}\b";

                foreach (Match match in Regex.Matches(inputText, pattern))
                {
                    if (IsInsideQuotes(match.Index, quotedRanges))
                    {
                        continue;
                    }

                    ITextRange hitRange = textDocument.GetRange(inputStart + match.Index,
                        inputStart + match.Index + match.Length);
                    hitRange.CharacterFormat.ForegroundColor = color;
                    hitRange.CharacterFormat.Bold = FormatEffect.Off;
                }
            }
        }
    }
}
