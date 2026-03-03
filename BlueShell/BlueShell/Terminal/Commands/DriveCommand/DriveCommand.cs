using BlueShell.Model;
using BlueShell.Model.Properties;
using BlueShell.Services;
using BlueShell.Services.FileSystem;
using BlueShell.Services.Properties;
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
            bool isParsed = DriveCommandParser.TryParse(commandLine, out List<string> drives,
                out string operation, out string extraOperation, out List<Tuple<string, TerminalMessageKind>> errorMessages);

            if (!isParsed)
            {
                foreach (var errorMessage in errorMessages)
                {
                    string message = errorMessage.Item1;
                    TerminalMessageKind messageKind = errorMessage.Item2;
                    context.TerminalOutput.Write(message, messageKind);
                }

                return;
            }

            await HandleOperation(context, drives, operation, extraOperation);
        }

        private static (int, string) ConvertIndexToDrivePath(string driveIndex)
        {
            driveIndex = driveIndex.Replace("[", "").Replace("]", "");

            bool ok = int.TryParse(driveIndex, out int index);

            DriveInfo[] drives = DriveInfo.GetDrives();

            return ok ? (0, drives[index].RootDirectory.FullName) : (0, driveIndex);
        }

        private async Task HandleOperation(TerminalCommandContext context, List<string> drives, string operation, string extraOperation)
        {
            switch (operation)
            {
                case "GetDrives":
                    await HandleGetDrivesOperation(context, extraOperation);
                    break;
                case "Open":
                    await HandleOpenOperation(context, drives, extraOperation);
                    break;
                case "Properties" or "Advanced":
                    await HandlePropertiesOperation(context, operation, drives, extraOperation);
                    break;
            }
        }

        private static async Task PrintOutput(TerminalCommandContext context, string[] outputLines)
        {
            for (int i = 0; i < outputLines.Length; i++)
            {
                string line = outputLines[i];

                context.TerminalOutput.Write(line, TerminalMessageKind.PrintOutput);

                if (i % 300 == 0)
                {
                    await Task.Yield();
                }
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

        private async Task HandleOpenOperation(TerminalCommandContext context, List<string> drives, string extraOperation)
        {
            context.DataDisplay.SetHeader(DriveCommandUi.CreateFileSystemHeader());

            if (drives.Count == 1)
            {
                context.TerminalOutput.WriteLine($">> Opening {drives[0]}...", TerminalMessageKind.Success);
                await HandleOpenSingleDrive(context, drives[0], extraOperation);
            }
            else
            {
                string message = ">> Opening: [";
                for (int i = 0; i < drives.Count; i++)
                {
                    if (i == drives.Count - 1)
                    {
                        message += $"{drives[i]}]...";
                    }
                    else
                    {
                        message += $"{drives[i]}, ";
                    }
                }
                context.TerminalOutput.WriteLine(message, TerminalMessageKind.Success);
                await HandleOpenMultipleDrives(context, drives, extraOperation);
            }
        }

        private async Task HandleOpenSingleDrive(TerminalCommandContext context, string driveTarget, string extraOperation)
        {
            (int returnValue, string message) = ConvertIndexToDrivePath(driveTarget);

            if (returnValue == -1)
            {
                context.TerminalOutput.WriteLine($">> {message}", TerminalMessageKind.Error);
                return;
            }

            driveTarget = message;


            List<FileSystemItem> folders = fileSystemService.LoadFolders(driveTarget);
            List<FileSystemItem> files = fileSystemService.LoadFiles(driveTarget);
            List<FileSystemItem> fileSystemItems = [.. folders, .. files];

            switch (extraOperation)
            {
                case "":
                    context.DataDisplay.Clear();
                    foreach (DataDisplayItem dataDisplay in fileSystemItems.Select(DriveCommandUi.CreateFileSystemItem))
                    {
                        await DriveCommandUi.ConfigureFileSystemDataItem(dataDisplay);
                        context.DataDisplay.Add(dataDisplay);
                    }
                    break;
                case "Print":

                    string[] lines = printService.PrintFolderContents(fileSystemItems, driveTarget);
                    await PrintOutput(context, lines);
                    break;
            }
        }

        private async Task HandleOpenMultipleDrives(TerminalCommandContext context, List<string> drives, string extraOperation)
        {
            List<string> resolvedDrivePaths = [];
            foreach (string drive in drives)
            {
                (int returnValue, string message) = ConvertIndexToDrivePath(drive);
                if (returnValue == -1)
                {
                    context.TerminalOutput.WriteLine($">> {message}", TerminalMessageKind.Error);
                    return;
                }

                resolvedDrivePaths.Add(message);
            }

            context.DataDisplay.BeginGrouped();

            foreach (string resolvedDrivePath in resolvedDrivePaths)
            {
                List<FileSystemItem> folders = fileSystemService.LoadFolders(resolvedDrivePath);
                List<FileSystemItem> files = fileSystemService.LoadFiles(resolvedDrivePath);
                List<FileSystemItem> fileSystemItems = [.. folders, .. files];

                switch (extraOperation)
                {
                    case "":
                        context.DataDisplay.Clear();
                        DataDisplayGroup dataDisplayGroup = new()
                        {
                            Header = resolvedDrivePath
                        };

                        context.DataDisplay.AddGroup(dataDisplayGroup);

                        int i = 0;
                        foreach (DataDisplayItem dataDisplay in fileSystemItems.Select(DriveCommandUi.CreateFileSystemItem))
                        {
                            await DriveCommandUi.ConfigureFileSystemDataItem(dataDisplay);
                            dataDisplayGroup.Items.Add(dataDisplay);

                            if (++i % 200 == 0)
                            {
                                await Task.Yield();
                            }
                        }
                        break;
                    case "Print":
                        string[] allLines = printService.PrintFolderContents(fileSystemItems, resolvedDrivePath);
                        await PrintOutput(context, allLines);
                        break;
                }
            }
        }

        private async Task HandlePropertiesOperation(TerminalCommandContext context, string operation, List<string> drives, string extraOperation)
        {
            foreach (string drive in drives)
            {
                (int returnValue, string message) = ConvertIndexToDrivePath(drive);

                if (returnValue != -1)
                {
                    continue;
                }

                context.TerminalOutput.WriteLine($">> {message}", TerminalMessageKind.Error);
                return;
            }

            Dictionary<string, object> properties;

            List<PropertyItem> propertyItems = [];

            switch (operation)
            {
                case "Properties":
                    foreach (string drive in drives)
                    {
                        string displayName = await driveService.GetDriveDisplayName(drive);

                        properties = driveService.GetDriveProperties(drive);
                        context.TerminalOutput.WriteLine($">> Displaying properties for drive: {drive}\n", TerminalMessageKind.Success);

                        PropertyItem propertyItem = await DrivePropertiesBuilder.BuildDrivePropertyItem(
                            displayName, drive, properties, false);

                        propertyItems.Add(propertyItem);
                    }
                    break;
                case "Advanced":
                    foreach (string drive in drives)
                    {
                        string displayName = await driveService.GetDriveDisplayName(drive);

                        properties = driveService.GetAdvancedDriveProperties(drive);
                        context.TerminalOutput.WriteLine($">> Displaying advanced properties for drive: {drive}\n", TerminalMessageKind.Success);

                        PropertyItem propertyItem = await DrivePropertiesBuilder.BuildDrivePropertyItem(
                            displayName, drive, properties, true);

                        propertyItems.Add(propertyItem);
                    }
                    break;
            }

            switch (extraOperation)
            {
                case "":
                    DriveCommandUi.InitializeDriveProperties(propertyItems);
                    break;
                case "Print":
                    foreach (PropertyItem propertyItem in propertyItems)
                    {
                        string[] lines = printService.PrintGeneralProperties(propertyItem);
                        await PrintOutput(context, lines);
                    }
                    break;
            }
        }
    }
}
