using BlueShell.Services;
using BlueShell.Services.FileSystem;
using BlueShell.Terminal.Abstractions;
using BlueShell.Terminal.Commands;
using BlueShell.Terminal.Commands.DriveCommand;
using BlueShell.Terminal.Commands.FolderCommand;
using System.Collections.Generic;

namespace BlueShell.Terminal.Infrastructure
{
    public static class TerminalCommandRegistry
    {
        public static IReadOnlyList<ITerminalCommand> CreateDefault() =>
        [
            new ClearAllCommand(),
            new ClearCommand(),
            new ClearDisplayCommand(),
            new DriveCommand(new DriveService(), new FileSystemService(), new PrintService()),
            new ExitCommand(),
            new FolderCommand(new FileSystemService(), new PrintService()),
            new SimulateCommand(),
            new VersionCommand()
        ];
    }
}