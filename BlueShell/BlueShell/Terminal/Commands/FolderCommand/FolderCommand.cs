using BlueShell.Model;
using BlueShell.Services;
using BlueShell.Services.FileSystem;
using BlueShell.Services.Wrappers;
using BlueShell.Terminal.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlueShell.Terminal.Commands.FolderCommand
{
    public sealed class FolderCommand(IFileSystemService fileSystemService, IPrintService printService)
        : ITerminalCommand
    {
        public string CommandName => "Folder";
        public bool IsCancelling => false;

        public async Task ExecuteAsync(TerminalCommandContext context, string commandLine)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            bool isParsed = FolderCommandParser.TryParse(
                commandLine,
                out List<string> filePaths,
                out string operation,
                out string destination,
                out string extraOperation,
                out List<Tuple<string, TerminalMessageKind>> errorMessages);

            if (!isParsed)
            {
                foreach (var (message, messageKind) in errorMessages)
                {
                    context.TerminalOutput.Write(message, messageKind);
                }

                return;
            }

            await HandleOperation(context, filePaths, operation, destination, extraOperation);
        }

        private async Task HandleOperation(
            TerminalCommandContext context,
            List<string> filePaths,
            string operation,
            string destination,
            string extraOperation)
        {
            switch (operation)
            {
                case "Open":
                    if (filePaths.Count == 1)
                    {
                        await HandleOpenSingleFolder(context, filePaths[0], extraOperation);
                    }
                    else
                    {
                        await HandleOpenMultipleFolders(context, filePaths, extraOperation);
                    }

                    break;
                case "Delete":
                    break;
                case "Properties":
                    break;
                case "Advanced":
                    break;
                case "Rename":
                    break;
                case "Copy":
                    break;
                case "Move":
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

        private async Task HandleOpenSingleFolder(TerminalCommandContext context, string filePath,
            string extraOperation)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            context.TabModel?.SearchLocation = $"Search {filePath}...";

            context.TabModel?.ClearPath();
            context.TabModel?.SetPath(filePath, false);
            context.TabModel?.AddFilePaths([filePath]);

            context.TerminalOutput.WriteLine($">> Opening folder \"{filePath}\"...", TerminalMessageKind.Success);

            List<FileSystemItem> folders = fileSystemService.LoadFolders(filePath, context.CancellationToken);
            context.CancellationToken.ThrowIfCancellationRequested();

            List<FileSystemItem> files = fileSystemService.LoadFiles(filePath, context.CancellationToken);
            context.CancellationToken.ThrowIfCancellationRequested();

            List<FileSystemItem> fileSystemItems = [.. folders, .. files];

            switch (extraOperation)
            {
                case "":
                    context.DataDisplay.Clear();
                    context.DataDisplay.SetHeader(UiClass.CreateFileSystemHeader());

                    int i = 0;
                    foreach (DataDisplayItem dataDisplay in fileSystemItems.Select(UiClass.CreateFileSystemItem))
                    {
                        await UiClass.ConfigureFileSystemDataItem(dataDisplay, context.CancellationToken);
                        context.DataDisplay.Add(dataDisplay);

                        if (++i % 200 == 0)
                        {
                            await Task.Yield();
                        }
                    }

                    break;

                case "Print":
                    string[] lines = printService.PrintFolderContents(fileSystemItems, filePath);
                    await PrintOutput(context, lines);
                    break;
            }
        }

        private async Task HandleOpenMultipleFolders(TerminalCommandContext context, List<string> filePaths,
            string extraOperation)
        {
            context.TabModel?.SearchLocation = $"Search {filePaths.Count} folders...";

            context.TabModel?.ClearPath();
            context.TabModel?.SetPath($"{filePaths.Count} folders", true);
            context.TabModel?.AddFilePaths(filePaths);

            context.DataDisplay.Clear();
            context.DataDisplay.BeginGrouped();

            context.TerminalOutput.WriteLine($">> Opening folders:\n\n{string.Join("\n", filePaths)}",
                TerminalMessageKind.Success);

            foreach (string filePath in filePaths)
            {
                List<FileSystemItem> folders = fileSystemService.LoadFolders(filePath, context.CancellationToken);
                context.CancellationToken.ThrowIfCancellationRequested();

                List<FileSystemItem> files = fileSystemService.LoadFiles(filePath, context.CancellationToken);
                context.CancellationToken.ThrowIfCancellationRequested();

                List<FileSystemItem> fileSystemItems = [.. folders, .. files];

                switch (extraOperation)
                {
                    case "":
                        DataDisplayGroup dataDisplayGroup = new()
                        {
                            Header = filePath
                        };

                        context.DataDisplay.AddGroup(dataDisplayGroup);
                        // Folder ["C:\Program Files", "C:\Program Files (x86)", "C:\ProgramData", "C:\SQL2022", "C:\MyFolder", "C:\Users"] -Open
                        int i = 0;
                        foreach (DataDisplayItem dataDisplay in fileSystemItems.Select(UiClass.CreateFileSystemItem))
                        {
                            //await Task.Delay(1, context.CancellationToken);
                            await UiClass.ConfigureFileSystemDataItem(dataDisplay, context.CancellationToken);
                            dataDisplayGroup.Items.Add(dataDisplay);

                            i++;
                            if (i % 250 == 0)
                            {
                                await Task.Yield();
                            }
                        }

                        break;
                    case "Print":
                        string[] lines = printService.PrintFolderContents(fileSystemItems, filePath);
                        await PrintOutput(context, lines);
                        break;
                }
            }

            context.TerminalOutput.WriteLine("Done!", TerminalMessageKind.Info);
        }
    }
}