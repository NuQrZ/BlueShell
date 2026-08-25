using BlueShell.Terminal.Abstractions;
using BlueShell.Terminal.Commands;
using System.Collections.Generic;

namespace BlueShell.Terminal.Infrastructure
{
    public static class TerminalCommandRegistry
    {
        public static IReadOnlyList<ITerminalCommand> CreateDefault() =>
        [
            new ClearCommand(),
            new ExitCommand(),
            new SimulateCommand(),
            new VersionCommand()
        ];
    }
}
