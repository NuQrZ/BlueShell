using BlueShell.Model;
using BlueShell.Services;
using BlueShell.Terminal.Abstractions;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BlueShell.Terminal.Commands.DriveCommand
{
    public sealed class DriveCommand(IDriveService driveService) : ITerminalCommand
    {
        public string CommandName => "Drive";
        public async Task ExecuteAsync(TerminalCommandContext context, string commandLine)
        {
            string[] patterns =
            [
                @"^Drive\s+-(?<Operation>\S+)\s*$",
                "^Drive\\s+\"(?<Drive>[^\"]+)\"\\s+-(?<Operation>\\S+)\\s*$",
                @"^Drive\s+(?<Drive>\[-?\d+\])\s+-(?<Operation>\S+)\s*$"
            ];


            string drive = "";
            string operation = "";

            foreach (string pattern in patterns)
            {
                Match match = Regex.Match(commandLine, pattern);
                if (!match.Success)
                {
                    continue;
                }

                drive = match.Groups["Drive"].Value;
                operation = match.Groups["Operation"].Value;
                break;
            }

            operation = operation.Trim();

            drive = NormalizeDriveToken(drive);

            (string, bool, TerminalMessageKind) returnValue = DriveCommandParser.Validate(drive, operation);

            if (!returnValue.Item2)
            {
                context.Output.PrintLine(returnValue.Item1, returnValue.Item3);
                return;
            }

            switch (operation)
            {
                case "GetDrives":
                    context.Output.PrintLine("\nDisplaying all available drives!\n", TerminalMessageKind.Success);
                    context.DataDisplay.SetHeader(DriveCommandUI.CreateHeaderType1());
                    List<DriveEntry> drives = await driveService.GetDrives();
                    foreach (DataDisplayItem dataDisplayItem in drives.Select(DriveCommandUI.CreateDriveItem))
                    {
                        await DriveCommandUI.ConfigureDataItem(dataDisplayItem);
                        context.DataDisplay.Add(dataDisplayItem);
                    }
                    break;
                case "Open":
                    {
                        string driveTarget = await ResolveDriveTarget(drive);

                        if (driveTarget.Contains("Invalid index"))
                        {
                            context.Output.PrintLine($"\n>> {driveTarget}\n", TerminalMessageKind.Error);
                            break;
                        }

                        context.Output.PrintLine($"\n>> Open not implemented yet for {driveTarget}.\n", TerminalMessageKind.Info);
                        break;
                    }
                case "Properties":
                    {
                        string driveTarget = await ResolveDriveTarget(drive);
                        context.Output.PrintLine($"\n>> Properties not implemented yet for {driveTarget}.\n", TerminalMessageKind.Info);
                        break;
                    }
                case "Advanced":
                    {
                        string driveTarget = await ResolveDriveTarget(drive);
                        context.Output.PrintLine($"\n>> Advanced not implemented yet for {driveTarget}.\n", TerminalMessageKind.Info);
                        break;
                    }
            }
        }

        private static string NormalizeDriveToken(string driveToken)
        {
            if (string.IsNullOrWhiteSpace(driveToken))
            {
                return "";
            }

            driveToken = driveToken.Trim();

            if (driveToken.Length >= 2 && driveToken.StartsWith('\"') && driveToken.EndsWith('\"'))
            {
                return driveToken[1..^1];
            }

            return driveToken;
        }

        private async Task<string> ResolveDriveTarget(string driveToken)
        {
            if (!DriveCommandParser.TryParseIndex(driveToken, out int index))
            {
                return index < 0 ? $"Invalid index: {index}!" : driveToken;
            }

            List<DriveEntry> drives = await driveService.GetDrives();
            int driveCount = drives.Count;

            return drives[index % driveCount].DriveInfo.Name;
        }
    }
}
