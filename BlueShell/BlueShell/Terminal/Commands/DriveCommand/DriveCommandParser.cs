using System.Text.RegularExpressions;
using BlueShell.Terminal.Abstractions;

namespace BlueShell.Terminal.Commands.DriveCommand
{
    public static class DriveCommandParser
    {
        private static readonly Regex IndexRegex = new(@"^\[(?<Index>-?\d+)\]$", RegexOptions.Compiled);
        private static readonly Regex PathRegex = new(@"^[A-Za-z]:[\\/].*$", RegexOptions.Compiled);

        public static (string message, bool ok, TerminalMessageKind kind) Validate(string drive, string operation)
        {
            if (string.IsNullOrEmpty(drive) && string.IsNullOrEmpty(operation))
            {
                return ("\n>> Drive and operation parameters cannot be empty!\n", false, TerminalMessageKind.Warning);
            }

            if (string.IsNullOrEmpty(drive))
            {
                return ("\n>> Drive cannot be empty!\n", false, TerminalMessageKind.Warning);
            }

            if (string.IsNullOrWhiteSpace(operation))
            {
                return ("\n>> Operation cannot be empty!\n", false, TerminalMessageKind.Warning);
            }

            switch (operation)
            {
                case "GetDrives":
                    return !string.IsNullOrWhiteSpace(drive) ? ("\n>> Operation \"-GetDrives\" does not take any drive parameters!\n", false, TerminalMessageKind.Warning) : ("", true, TerminalMessageKind.Success);

                case "Open":
                case "Properties":
                case "Advanced":
                    if (string.IsNullOrWhiteSpace(drive))
                    {
                        return ("\n>> Missing drive. Use for example: Drive \"C:\\\" -Open OR Drive [1] -Open\n", false, TerminalMessageKind.Warning);
                    }

                    if (IndexRegex.IsMatch(drive) || PathRegex.IsMatch(drive))
                    {
                        return ("", true, TerminalMessageKind.Success);
                    }

                    return ("\n>> Drive must be a quoted path like \"C:\\\" or an index like [1].\n", false, TerminalMessageKind.Warning);

                default:
                    return ($"\n>> Operation \"{operation}\" is unknown!\n", false, TerminalMessageKind.Error);
            }
        }


        public static bool TryParseIndex(string driveToken, out int index1Based)
        {
            index1Based = 0;
            var m = IndexRegex.Match(driveToken);
            if (!m.Success)
            {
                return false;
            }

            return int.TryParse(m.Groups["Index"].Value, out index1Based) && index1Based >= 0;
        }
    }
}
