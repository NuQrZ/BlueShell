using BlueShell.Terminal.Abstractions;
using BlueShell.Terminal.Commands;
using System.Collections.Generic;

namespace BlueShell.Terminal.Infrastructure
{
    public static class TerminalCommandRegistry
    {
        private static readonly IReadOnlyList<ITerminalCommand> _commands =
        [
            new ClearCommand(),
            new VersionCommand(),
            new ExitCommand(),
            new SimulateCommand()
        ];

        public static IReadOnlyList<ITerminalCommand> Commands => _commands;
    }
}
