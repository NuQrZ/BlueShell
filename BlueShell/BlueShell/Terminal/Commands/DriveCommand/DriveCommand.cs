using BlueShell.Model;
using BlueShell.Services;
using BlueShell.Services.FileSystem;
using BlueShell.Services.Wrappers;
using BlueShell.Terminal.Abstractions;
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
            bool returnValue = DriveCommandParser.TryParse(commandLine, out string driveTarget,
                out string operation, out string extraOperation, out List<Tuple<string, TerminalMessageKind>> errorMessages);

            if (!returnValue)
            {
                if (errorMessages.Count > 0)
                {
                    context.Output.Print("\n");
                }

                for (int i = 0; i < errorMessages.Count; i++)
                {
                    Tuple<string, TerminalMessageKind> errorMessage = errorMessages[i];
                    string message = errorMessage.Item1;
                    TerminalMessageKind messageKind = errorMessage.Item2;
                    if (i == errorMessages.Count - 1)
                    {
                        context.Output.PrintLine("\n" + message, messageKind);
                    }
                    else
                    {
                        context.Output.PrintLine(message, messageKind);
                    }
                }
                return;
            }

            switch (operation)
            {
                case "GetDrives":
                    context.DataDisplay.Clear();
                    context.Output.PrintLine("\n>> Displaying all available drives!\n", TerminalMessageKind.Success);
                    List<DriveItem> drives = await driveService.GetDrives();

                    switch (extraOperation)
                    {
                        case "Print":
                            context.Output.SetTextWrap(true);
                            context.Output.PrintLine(printService.PrintDrives(drives), default, "Cascadia Code");
                            context.Output.SetTextWrap(false);
                            break;
                        case "":
                            {
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
                        if (driveTarget.Contains("Invalid index"))
                        {
                            context.Output.PrintLine($"\n>> {driveTarget}\n", TerminalMessageKind.Error);
                            break;
                        }

                        driveTarget = ConvertIndexToDrivePath(driveTarget);

                        context.DataDisplay.Clear();
                        context.Output.PrintLine($"\nOpening {driveTarget}...\n", TerminalMessageKind.Info);
                        List<FileSystemItem> folders = fileSystemService.LoadFolders(driveTarget);
                        List<FileSystemItem> files = fileSystemService.LoadFiles(driveTarget);
                        switch (extraOperation)
                        {
                            case "Print":
                                context.Output.SetTextWrap(true);
                                List<FileSystemItem> fileSystemItems = [.. folders, .. files];
                                context.Output.PrintLine(printService.PrintFolderContents(fileSystemItems), default, "Cascadia Code");
                                context.Output.SetTextWrap(false);
                                break;
                            case "":
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
                        driveTarget = ConvertIndexToDrivePath(driveTarget);
                        context.Output.PrintLine($"\n>> Properties not implemented yet for {driveTarget}.\n", TerminalMessageKind.Info);
                        break;
                    }
                case "Advanced":
                    {
                        driveTarget = ConvertIndexToDrivePath(driveTarget);
                        context.Output.PrintLine($"\n>> Advanced not implemented yet for {driveTarget}.\n", TerminalMessageKind.Info);
                        break;
                    }
            }
        }

        private static string ConvertIndexToDrivePath(string driveIndex)
        {
            bool ok = int.TryParse(driveIndex, out int index);

            if (!ok)
            {
                return driveIndex;
            }

            DriveInfo[] drives = DriveInfo.GetDrives();

            index %= drives.Length;

            return drives[index].RootDirectory.FullName;
        }
    }
}
