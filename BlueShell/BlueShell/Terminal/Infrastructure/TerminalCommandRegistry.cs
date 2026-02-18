using BlueShell.Services;
using BlueShell.Services.FileSystem;
using BlueShell.Terminal.Abstractions;
using BlueShell.Terminal.Commands;
using BlueShell.Terminal.Commands.DriveCommand;
using System.Collections.Generic;

namespace BlueShell.Terminal.Infrastructure
{
    public static class TerminalCommandRegistry
    {
        public static IReadOnlyList<ITerminalCommand> CreateDefault() =>
        [
            new ExitCommand(),
            new ClearCommand(),
            new ClearDisplayCommand(),
            new DriveCommand(new DriveService(), new PrintService()),
            new VersionCommand()
        ];
    }
}
