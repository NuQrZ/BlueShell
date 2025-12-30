using BlueShell.Model;
using BlueShell.Services;
using BlueShell.Services.Wrappers;
using BlueShell.Terminal.Abstractions;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BlueShell.Terminal.Commands.DriveCommand
{
    public sealed class DriveCommand(IDriveService driveService, FileSystemService fileSystemService) : ITerminalCommand
    {
        private static readonly string[] DrivePatterns =
        [
            @"^Drive\s*$",
            """^Drive\s+(?<Drive>"[^"]+")\s*$""",
            @"^Drive\s+(?<Drive>\[-?\d+\])\s*$",
            @"^Drive\s+-(?<Operation>\S+)\s*$",
            """^Drive\s+(?<Drive>"[^"]+")\s+-(?<Operation>\S+)\s*$""",
            @"^Drive\s+(?<Drive>\[-?\d+\])\s+-(?<Operation>\S+)\s*$"
        ];

        public string CommandName => "Drive";
        public async Task ExecuteAsync(TerminalCommandContext context, string commandLine)
        {
            string drive = string.Empty;
            string operation = string.Empty;

            foreach (string pattern in DrivePatterns)
            {
                Match match = Regex.Match(
                    commandLine,
                    pattern,
                    RegexOptions.IgnoreCase | RegexOptions.Compiled
                );

                if (!match.Success)
                {
                    continue;
                }

                if (match.Groups["Drive"]?.Success == true)
                {
                    drive = match.Groups["Drive"].Value;
                }

                if (match.Groups["Operation"]?.Success == true)
                {
                    operation = match.Groups["Operation"].Value;
                }

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
                    context.DataDisplay.SetHeader(DriveCommandUI.CreateDriveHeader());
                    List<DriveItem> drives = await driveService.GetDrives();
                    foreach (DataDisplayItem dataDisplayItem in drives.Select(DriveCommandUI.CreateDriveItem))
                    {
                        await DriveCommandUI.ConfigureDriveDataItem(dataDisplayItem);
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

                        context.Output.PrintLine($"\nOpening {driveTarget}...\n", TerminalMessageKind.Info);
                        context.DataDisplay.SetHeader(DriveCommandUI.CreateFileSystemHeader());
                        List<FileSystemItem> folders = fileSystemService.LoadFolders(driveTarget);
                        List<FileSystemItem> files = fileSystemService.LoadFiles(driveTarget);

                        foreach (DataDisplayItem dataDisplayItem in folders.Select(DriveCommandUI.CreateFileSystemItem))
                        {
                            await DriveCommandUI.ConfigureFileSystemDataItem(dataDisplayItem);
                            context.DataDisplay.Add(dataDisplayItem);
                        }
                        foreach (DataDisplayItem dataDisplayItem in files.Select(DriveCommandUI.CreateFileSystemItem))
                        {
                            await DriveCommandUI.ConfigureFileSystemDataItem(dataDisplayItem);
                            context.DataDisplay.Add(dataDisplayItem);
                        }
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

            List<DriveItem> drives = await driveService.GetDrives();
            int driveCount = drives.Count;

            return drives[index % driveCount].DriveInfo.Name;
        }
    }
}
