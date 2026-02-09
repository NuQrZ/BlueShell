using BlueShell.Model;
using BlueShell.Services;
using BlueShell.Services.FileSystem;
using BlueShell.Services.Wrappers;
using BlueShell.Terminal.Abstractions;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BlueShell.Terminal.Commands.DriveCommand
{
    public sealed class DriveCommand(IDriveService driveService, IFileSystemService fileSystemService, IPrintService printService) : ITerminalCommand
    {
        public string CommandName => "Drive";
        public async Task ExecuteAsync(TerminalCommandContext context, string commandLine)
        {
            bool isParsed = DriveCommandParser.TryParse(commandLine, out string driveTarget,
                out string operation, out string extraOperation, out List<Tuple<string, TerminalMessageKind>> errorMessages);

            if (!isParsed)
            {
                if (errorMessages.Count > 0)
                {
                    context.Output.Print("\n\n");
                }

                foreach (var errorMessage in errorMessages)
                {
                    string message = errorMessage.Item1;
                    TerminalMessageKind messageKind = errorMessage.Item2;
                    context.Output.PrintLine(message, messageKind);
                }

                context.Output.PrintLine();
                return;
            }

            switch (operation)
            {
                case "GetDrives":
                    context.Output.PrintLine("\n\n>> Displaying all available drives!\n", TerminalMessageKind.Success);
                    List<DriveItem> drives = await driveService.GetDrives();

                    switch (extraOperation)
                    {
                        case "Print":
                            context.Output.SetTextWrap(true);
                            string[] lines = printService.PrintDrives(drives);

                            for (int i = 0; i < lines.Length; i++)
                            {
                                if (i == 0)
                                {
                                    context.Output.PrintLine(lines[i], TerminalMessageKind.PrintOutput);
                                }
                                else
                                {
                                    context.Output.PrintLine("\n" + lines[i], TerminalMessageKind.PrintOutput);
                                }
                            }

                            context.Output.SetTextWrap(false);
                            break;
                        case "":
                            {
                                context.DataDisplay.Clear();
                                context.DataDisplay.SetHeader(DriveCommandUI.CreateDriveHeader());
                                foreach (DataDisplayItem dataDisplayItem in drives.Select(DriveCommandUI.CreateDriveItem))
                                {
                                    await DriveCommandUI.ConfigureDriveDataItem(dataDisplayItem);
                                    context.DataDisplay.Add(dataDisplayItem);
                                }

                                break;
                            }
                    }
                    break;
                case "Open":
                    {
                        (int returnValue, string message) = ConvertIndexToDrivePath(driveTarget);

                        if (returnValue == -1)
                        {
                            context.Output.PrintLine($"\n\n>> {message}\n", TerminalMessageKind.Error);
                            break;
                        }

                        context.Output.PrintLine($"\n\nOpening {driveTarget}...\n", TerminalMessageKind.Info);
                        List<FileSystemItem> folders = fileSystemService.LoadFolders(driveTarget);
                        List<FileSystemItem> files = fileSystemService.LoadFiles(driveTarget);
                        switch (extraOperation)
                        {
                            case "Print":
                                context.Output.SetTextWrap(true);
                                List<FileSystemItem> fileSystemItems = [.. folders, .. files];
                                foreach (string line in printService.PrintFolderContents(fileSystemItems))
                                {
                                    context.Output.PrintLine("\n" + line, TerminalMessageKind.PrintOutput);
                                }
                                context.Output.SetTextWrap(false);
                                break;
                            case "":
                                context.DataDisplay.Clear();
                                context.DataDisplay.SetHeader(DriveCommandUI.CreateFileSystemHeader());

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
                        break;
                    }
                case "Properties":
                    {
                        driveTarget = ConvertIndexToDrivePath(driveTarget).Item2;
                        context.Output.PrintLine($"\n\n>> Properties not implemented yet for {driveTarget}.\n", TerminalMessageKind.Info);
                        break;
                    }
                case "Advanced":
                    {
                        driveTarget = ConvertIndexToDrivePath(driveTarget).Item2;
                        context.Output.PrintLine($"\n\n>> Advanced not implemented yet for {driveTarget}.\n", TerminalMessageKind.Info);
                        break;
                    }
            }
        }

        private static (int, string) ConvertIndexToDrivePath(string driveIndex)
        {
            driveIndex = driveIndex.Replace("[", "").Replace("]", "");

            bool ok = int.TryParse(driveIndex, out int index);

            DriveInfo[] drives = DriveInfo.GetDrives();

            if (!ok)
            {
                string[] driveNames = [.. drives.Select(drive => drive.RootDirectory.FullName)];

                if ((driveIndex.Length > 3 && driveNames.Any(driveIndex.Contains)) || driveIndex.Length < 3)
                {
                    return (-1, $"There is no drive with name \"{driveIndex}\"");
                }
            }

            index %= drives.Length;

            return (0, drives[index].RootDirectory.FullName);
        }
    }
}
