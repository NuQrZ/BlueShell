using BlueShell.Core;
using Microsoft.UI;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Windows.UI;

namespace BlueShell.Terminal.Infrastructure
{
    public enum HighlightMode
    {
        CurrentInput,
        CurrentToken,
        AllCommands
    }

    public sealed record HighlightContext(
        RichEditTextDocument Document,
        HighlightMode Mode,
        ElementTheme ElementTheme,
        int InputStart = 0,
        string Prompt = "");

    public static class TerminalUtilities
    {
        private static readonly Regex QuoteRegex = new("\"(.*?)\"", RegexOptions.Compiled);

        public static void Highlight(HighlightContext highlightContext)
        {
            RichEditTextDocument document = highlightContext.Document;
            HighlightMode mode = highlightContext.Mode;
            ElementTheme elementTheme = highlightContext.ElementTheme;
            int inputStart = highlightContext.InputStart;
            string prompt = highlightContext.Prompt;

            switch (mode)
            {
                case HighlightMode.CurrentInput:
                    HighlightCurrentInput(document, inputStart, elementTheme);
                    return;
                case HighlightMode.CurrentToken:
                    HighlightCurrentTokenOnly(document, inputStart, elementTheme);
                    return;
                case HighlightMode.AllCommands:
                    if (string.IsNullOrEmpty(prompt))
                    {
                        throw new ArgumentException("Prompt must be provided for HighlightMode.AllCommands", nameof(highlightContext));
                    }
                    HighlightAllCommandsInDocument(document, prompt, elementTheme);
                    return;
                default:
                    throw new ArgumentOutOfRangeException(nameof(highlightContext), mode, null);
            }
        }

        private static void HighlightCurrentInput(RichEditTextDocument document, int inputStart, ElementTheme elementTheme)
        {
            int end = document.GetRange(0, int.MaxValue).EndPosition;
            if (end <= inputStart)
            {
                return;
            }

            ITextRange textRange = document.GetRange(inputStart, end);
            textRange.GetText(TextGetOptions.None, out string text);
            text = text.TrimEnd('\r', '\n');

            ApplySyntaxHighlighting(document, inputStart, text, elementTheme);
        }

        private static void HighlightAllCommandsInDocument(RichEditTextDocument document, string prompt, ElementTheme elementTheme)
        {
            int end = document.GetRange(0, int.MaxValue).EndPosition;
            if (end <= 0)
            {
                return;
            }

            ITextRange allRange = document.GetRange(0, end);
            allRange.GetText(TextGetOptions.None, out string fullText);

            int searchFrom = 0;

            while (true)
            {
                int promptIndex = fullText.IndexOf(prompt, searchFrom, StringComparison.Ordinal);
                if (promptIndex < 0)
                {
                    break;
                }

                int commandStart = promptIndex + prompt.Length;

                int lineEnd = fullText.IndexOf('\r', commandStart);
                if (lineEnd < 0)
                {
                    lineEnd = fullText.Length;
                }

                int length = lineEnd - commandStart;
                if (length > 0)
                {
                    string commandText = fullText.Substring(commandStart, length);
                    ApplySyntaxHighlighting(document, commandStart, commandText, elementTheme);
                }

                searchFrom = commandStart;
            }
        }

        private static void HighlightCurrentTokenOnly(RichEditTextDocument document, int inputStart, ElementTheme elementTheme)
        {
            ITextSelection selection = document.Selection;
            int end = document.GetRange(0, int.MaxValue).EndPosition;
            if (end <= inputStart)
            {
                return;
            }

            int caret = selection.StartPosition;

            ITextRange inputRange = document.GetRange(inputStart, end);
            inputRange.GetText(TextGetOptions.None, out string currentText);
            currentText = currentText.TrimEnd('\r', '\n');

            int selectorIndex = Math.Min(currentText.Length, caret - inputStart);
            if (selectorIndex < 0)
            {
                selectorIndex = 0;
            }

            int lineStart = currentText.LastIndexOf('\n', Math.Max(0, selectorIndex - 1));
            lineStart = lineStart < 0 ? 0 : lineStart + 1;

            int wordStart = selectorIndex;
            while (wordStart > lineStart && !char.IsWhiteSpace(currentText[wordStart - 1]))
            {
                wordStart--;
            }

            int wordEnd = selectorIndex;
            while (wordEnd < currentText.Length && !char.IsWhiteSpace(currentText[wordEnd]))
            {
                wordEnd++;
            }

            if (wordEnd <= wordStart)
            {
                return;
            }

            string token = currentText[wordStart..wordEnd];

            if (token.Contains(">>", StringComparison.Ordinal) || token.Contains('"', StringComparison.Ordinal))
            {
                return;
            }

            ApplyTokenHighlight(document, inputStart + wordStart, token, elementTheme);
        }

        private static void ApplySyntaxHighlighting(RichEditTextDocument document, int start, string text, ElementTheme elementTheme)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            Dictionary<string, Color> keywordColors = GetKeywordColors(elementTheme);
            Color defaultColor = GetDefaultColor(elementTheme);
            Color stringColor = GetStringColor(elementTheme);

            ITextRange baseRange = document.GetRange(start, start + text.Length);
            baseRange.CharacterFormat.ForegroundColor = defaultColor;
            baseRange.CharacterFormat.Bold = FormatEffect.Off;

            List<(int Start, int End)> quotedRanges = [];
            foreach (Match match in QuoteRegex.Matches(text))
            {
                quotedRanges.Add((match.Index, match.Index + match.Length));

                ITextRange stringRange = document.GetRange(start + match.Index, start + match.Index + match.Length);
                stringRange.CharacterFormat.ForegroundColor = stringColor;
            }

            foreach (KeyValuePair<string, Color> keywordColor in keywordColors)
            {
                string keyword = keywordColor.Key;
                Color color = keywordColor.Value;

                string pattern = keyword.StartsWith('-')
                    ? $@"(?<!\S){Regex.Escape(keyword)}(?!\S)"
                    : $@"\b{Regex.Escape(keyword)}\b";

                foreach (Match match in Regex.Matches(text, pattern, RegexOptions.IgnoreCase))
                {
                    if (IsInsideQuotes(match.Index, quotedRanges))
                    {
                        continue;
                    }

                    ITextRange hitRange = document.GetRange(start + match.Index, start + match.Index + match.Length);
                    hitRange.CharacterFormat.ForegroundColor = color;
                    hitRange.CharacterFormat.Bold = FormatEffect.Off;
                }
            }
        }

        private static void ApplyTokenHighlight(RichEditTextDocument document, int tokenStart, string token, ElementTheme elementTheme)
        {
            Dictionary<string, Color> keywordColors = GetKeywordColors(elementTheme);
            Color defaultColor = GetDefaultColor(elementTheme);

            ITextRange tokenRange = document.GetRange(tokenStart, tokenStart + token.Length);

            if (!keywordColors.TryGetValue(token, out Color color))
            {
                tokenRange.CharacterFormat.ForegroundColor = defaultColor;
                tokenRange.CharacterFormat.Bold = FormatEffect.Off;
                return;
            }

            tokenRange.CharacterFormat.ForegroundColor = color;
            tokenRange.CharacterFormat.Bold = FormatEffect.Off;
        }

        private static bool IsInsideQuotes(int index, List<(int Start, int End)> ranges)
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

        private static Dictionary<string, Color> GetKeywordColors(ElementTheme elementTheme)
        {
            return elementTheme == ElementTheme.Light
                ? Utilities.LightThemeKeywordColors
                : Utilities.DarkThemeKeywordColors;
        }

        private static Color GetDefaultColor(ElementTheme elementTheme)
        {
            return elementTheme == ElementTheme.Light ? Colors.Black : Colors.White;
        }

        private static Color GetStringColor(ElementTheme elementTheme)
        {
            return elementTheme == ElementTheme.Light
                ? Color.FromArgb(255, 186, 85, 211)
                : Colors.DarkOrange;
        }
    }
}
