using BlueShell.Model;
using BlueShell.Model.Properties;
using BlueShell.Services;
using BlueShell.Services.FileSystem;
using BlueShell.Services.Properties;
using BlueShell.Services.Wrappers;
using BlueShell.Terminal.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlueShell.Terminal.Commands.DriveCommand
{
    public sealed class DriveCommand(
        IDriveService driveService,
        IFileSystemService fileSystemService,
        IPrintService printService) : ITerminalCommand
    {
        public string CommandName => "Drive";
        public bool IsCancelling => false;

        public async Task ExecuteAsync(TerminalCommandContext context, string commandLine)
        {
            bool isParsed = DriveCommandParser.TryParse(commandLine, out List<string> drives,
                out string operation, out string extraOperation,
                out List<Tuple<string, TerminalMessageKind>> errorMessages);

            if (!isParsed)
            {
                foreach (var (message, messageKind) in errorMessages)
                {
                    context.TerminalOutput.Write(message, messageKind);
                }

                return;
            }

            await HandleOperation(context, drives, operation, extraOperation);
        }

        private async Task HandleOperation(TerminalCommandContext context, List<string> drives, string operation,
            string extraOperation)
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
                context.CancellationToken.ThrowIfCancellationRequested();

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
            context.TabModel?.ClearPath();
            context.TabModel?.SetPath("System", false);
            context.TerminalOutput.WriteLine(">> Displaying all available drives!", TerminalMessageKind.Success);
            List<DriveItem> drives = await driveService.GetDrives();

            switch (extraOperation)
            {
                case "":
                    context.TabModel?.SearchLocation = "Search System...";

                    context.DataDisplay.Clear();
                    context.DataDisplay.SetHeader(UiClass.CreateDriveHeader());
                    foreach (DataDisplayItem dataDisplayItem in drives.Select(UiClass.CreateDriveDisplayItem))
                    {
                        await UiClass.ConfigureDriveDisplayItem(dataDisplayItem, context.CancellationToken);
                        context.DataDisplay.Add(dataDisplayItem);
                    }

                    break;
                case "Print":
                    string[] lines = printService.PrintDrives(drives);
                    await PrintOutput(context, lines);
                    break;
            }
        }

        private async Task HandleOpenOperation(TerminalCommandContext context, List<string> drives,
            string extraOperation)
        {
            if (drives.Count == 1)
            {
                context.TabModel?.ClearPath();
                context.TabModel?.SetPath(drives[0], false);
                context.TabModel?.AddFilePaths(drives);
                context.TerminalOutput.WriteLine($">> Opening \"{drives[0]}\"...", TerminalMessageKind.Success);
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

        private async Task HandleOpenSingleDrive(TerminalCommandContext context, string driveTarget,
            string extraOperation)
        {
            List<FileSystemItem> folders = fileSystemService.LoadFolders(driveTarget, context.CancellationToken);
            List<FileSystemItem> files = fileSystemService.LoadFiles(driveTarget, context.CancellationToken);
            List<FileSystemItem> fileSystemItems = [.. folders, .. files];

            switch (extraOperation)
            {
                case "":
                    context.TabModel?.SearchLocation = $"Search {driveTarget}...";

                    context.DataDisplay.Clear();
                    context.DataDisplay.SetHeader(UiClass.CreateFileSystemHeader());
                    foreach (DataDisplayItem dataDisplay in fileSystemItems.Select(UiClass.CreateFileSystemItem))
                    {
                        await UiClass.ConfigureFileSystemDataItem(dataDisplay, context.CancellationToken);
                        context.DataDisplay.Add(dataDisplay);
                    }

                    break;
                case "Print":
                    string[] lines = printService.PrintFolderContents(fileSystemItems, driveTarget);
                    await PrintOutput(context, lines);
                    break;
            }
        }

        private async Task HandleOpenMultipleDrives(TerminalCommandContext context, List<string> drives,
            string extraOperation)
        {
            context.TabModel?.ClearPath();
            context.TabModel?.SetPath(drives[0], true);
            context.TabModel?.AddFilePaths(drives);

            if (extraOperation == "")
            {
                context.DataDisplay.Clear();
                context.DataDisplay.SetHeader(UiClass.CreateFileSystemHeader());
            }

            int driveCount = driveService.GetDriveCount();

            context.TabModel?.SearchLocation =
                driveCount == drives.Count ? "Search System" : $"Search {driveCount} drives";

            context.DataDisplay.BeginGrouped();

            foreach (string filePath in drives)
            {
                List<FileSystemItem> folders =
                    fileSystemService.LoadFolders(filePath, context.CancellationToken);
                List<FileSystemItem> files = fileSystemService.LoadFiles(filePath, context.CancellationToken);
                List<FileSystemItem> fileSystemItems = [.. folders, .. files];

                switch (extraOperation)
                {
                    case "":
                        DataDisplayGroup dataDisplayGroup = new()
                        {
                            Header = filePath
                        };

                        context.DataDisplay.AddGroup(dataDisplayGroup);

                        int i = 0;
                        foreach (DataDisplayItem dataDisplay in fileSystemItems.Select(UiClass.CreateFileSystemItem))
                        {
                            await UiClass.ConfigureFileSystemDataItem(dataDisplay, context.CancellationToken);
                            dataDisplayGroup.Items.Add(dataDisplay);

                            if (++i % 200 == 0)
                            {
                                await Task.Yield();
                            }
                        }

                        break;
                    case "Print":
                        string[] allLines = printService.PrintFolderContents(fileSystemItems, filePath);
                        await PrintOutput(context, allLines);
                        break;
                }
            }
        }

        private async Task HandlePropertiesOperation(TerminalCommandContext context, string operation,
            List<string> drives, string extraOperation)
        {
            Dictionary<string, object> properties;

            List<PropertyItem> propertyItems = [];
            string message;

            switch (operation)
            {
                case "Properties":

                    message = ">> Displaying general properties for: [";
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

                    foreach (string drive in drives)
                    {
                        string displayName = await driveService.GetDriveDisplayName(drive);

                        properties = driveService.GetDriveProperties(drive);

                        PropertyItem propertyItem = await DrivePropertiesBuilder.BuildDrivePropertyItem(
                            displayName, drive, properties, false);

                        propertyItems.Add(propertyItem);
                    }

                    break;
                case "Advanced":
                    message = ">> Displaying advanced properties for: [";
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

                    foreach (string drive in drives)
                    {
                        string displayName = await driveService.GetDriveDisplayName(drive);

                        properties = driveService.GetAdvancedDriveProperties(drive);

                        PropertyItem propertyItem = await DrivePropertiesBuilder.BuildDrivePropertyItem(
                            displayName, drive, properties, true);

                        propertyItems.Add(propertyItem);
                    }

                    break;
            }

            switch (extraOperation)
            {
                case "":
                    UiClass.InitializeDriveProperties(propertyItems);
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