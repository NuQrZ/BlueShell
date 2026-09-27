using BlueShell.Terminal.Abstractions;
using BlueShell.Terminal.Commands;
using System.Collections.Generic;

namespace BlueShell.Terminal.Infrastructure
{
    public static class TerminalCommandRegistry
    {
        public static IReadOnlyList<ITerminalCommand> Commands { get; } =
        [
            new ClearCommand(),
            new VersionCommand(),
            new ExitCommand(),
            new SimulateCommand()
        ];
    }
}
