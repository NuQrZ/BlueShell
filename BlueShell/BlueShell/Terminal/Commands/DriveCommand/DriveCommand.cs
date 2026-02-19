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
            bool isParsed = DriveCommandParser.TryParse(commandLine, out string driveTarget,
                out string operation, out string extraOperation, out List<Tuple<string, TerminalMessageKind>> errorMessages);


            if (!isParsed)
            {
                foreach (var errorMessage in errorMessages)
                {
                    string message = errorMessage.Item1;
                    TerminalMessageKind messageKind = errorMessage.Item2;
                    context.TerminalOutput.Write(message, messageKind);
                }

                context.TerminalOutput.WriteLine();
                return;
            }

            await HandleOperation(context, driveTarget, operation, extraOperation);
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

        private async Task HandleOperation(TerminalCommandContext context, string driveTarget, string operation, string extraOperation)
        {
            switch (operation)
            {
                case "GetDrives":
                    await HandleGetDrivesOperation(context, extraOperation);
                    break;
                case "Open":
                    await HandleOpenOperation(context, driveTarget, extraOperation);
                    break;
                case "Properties":
                    context.TerminalOutput.WriteLine($">> Properties not implemented yet.", TerminalMessageKind.Info);
                    break;
                case "Advanced":
                    context.TerminalOutput.WriteLine($">> Advanced not implemented yet.", TerminalMessageKind.Info);
                    break;
            }
        }

        private static async Task PrintOutput(TerminalCommandContext context, string[] outputLines)
        {
            foreach (string line in outputLines)
            {
                await Task.Delay(2, context.CancellationToken);
                context.TerminalOutput.Write(line, TerminalMessageKind.PrintOutput);
            }
        }

        private async Task HandleGetDrivesOperation(TerminalCommandContext context, string extraOperation)
        {
            context.TerminalOutput.WriteLine(">> Displaying all available drives!", TerminalMessageKind.Success);
            List<DriveItem> drives = await driveService.GetDrives();

            switch (extraOperation)
            {
                case "":
                    context.DataDisplay.Clear();
                    context.DataDisplay.SetHeader(DriveCommandUi.CreateDriveHeader());
                    foreach (DataDisplayItem dataDisplayItem in drives.Select(DriveCommandUi.CreateDriveDisplayItem))
                    {
                        await DriveCommandUi.ConfigureDriveDisplayItem(dataDisplayItem);
                        context.DataDisplay.Add(dataDisplayItem);
                    }

                    break;
                case "Print":
                    string[] lines = printService.PrintDrives(drives);
                    await PrintOutput(context, lines);
                    break;
            }
        }

        private async Task HandleOpenOperation(TerminalCommandContext context, string driveTarget, string extraOperation)
        {
            (int returnValue, string message) = ConvertIndexToDrivePath(driveTarget);

            if (returnValue == -1)
            {
                context.TerminalOutput.WriteLine($">> {message}", TerminalMessageKind.Error);
                return;
            }

            context.TerminalOutput.WriteLine($">> Opening {driveTarget}...", TerminalMessageKind.Info);

            List<FileSystemItem> folders = fileSystemService.LoadFolders(driveTarget);
            List<FileSystemItem> files = fileSystemService.LoadFiles(driveTarget);

            switch (extraOperation)
            {
                case "":
                    context.DataDisplay.Clear();
                    context.DataDisplay.SetHeader(DriveCommandUi.CreateFileSystemHeader());

                    foreach (DataDisplayItem folder in folders.Select(DriveCommandUi.CreateFileSystemItem))
                    {
                        await DriveCommandUi.ConfigureFileSystemDataItem(folder);
                        context.DataDisplay.Add(folder);
                    }

                    foreach (DataDisplayItem file in files.Select(DriveCommandUi.CreateFileSystemItem))
                    {
                        await DriveCommandUi.ConfigureFileSystemDataItem(file);
                        context.DataDisplay.Add(file);
                    }
                    break;
                case "Print":
                    List<FileSystemItem> fileSystemItems = [.. folders, .. files];
                    string[] lines = printService.PrintFolderContents(fileSystemItems);
                    await PrintOutput(context, lines);
                    break;
            }
        }
    }
}
