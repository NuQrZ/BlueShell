using BlueShell.Model;
using BlueShell.Services;
using BlueShell.Services.FileSystem;
using BlueShell.Services.Wrappers;
using BlueShell.Terminal.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlueShell.Terminal.Commands.DriveCommand
{
    public sealed class DriveCommand(IDriveService driveService, IPrintService printService) : ITerminalCommand
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

        private async Task HandleOperation(TerminalCommandContext context, string driveTarget, string operation, string extraOperation)
        {
            switch (operation)
            {
                case "GetDrives":
                    await HandleGetDrivesOperation(context, extraOperation);
                    break;
                case "Open":
                    context.TerminalOutput.WriteLine($">> Open not implemented yet.", TerminalMessageKind.Info);
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
                await Task.Delay(15, context.CancellationToken);
                context.TerminalOutput.Write(line, TerminalMessageKind.PrintOutput);
            }
        }

        private async Task HandleGetDrivesOperation(TerminalCommandContext context, string extraOperation)
        {
            context.TerminalOutput.WriteLine(">> Displaying all available drives!", TerminalMessageKind.Success);
            List<DriveItem> drives = await driveService.GetDrives();

            switch (extraOperation)
            {
                case "Print":
                    string[] lines = printService.PrintDrives(drives);
                    await PrintOutput(context, lines);
                    break;
                case "":
                    context.DataDisplay.Clear();
                    context.DataDisplay.SetHeader(DriveCommandUi.CreateDriveHeader());
                    foreach (DataDisplayItem dataDisplayItem in drives.Select(DriveCommandUi.CreateDriveDisplayItem))
                    {
                        await DriveCommandUi.ConfigureDriveDisplayItem(dataDisplayItem);
                        context.DataDisplay.Add(dataDisplayItem);
                    }

                    break;
            }
        }
    }
}
