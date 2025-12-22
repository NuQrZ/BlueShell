using BlueShell.Terminal.Abstractions;
using BlueShell.Terminal.Commands;
using System.Collections.Generic;

namespace BlueShell.Terminal.Infrastructure
{
    public static class TerminalCommandRegistry
    {
        public static IReadOnlyList<ITerminalCommand> CreateDefault() =>
        [
            new ExitCommand(),
            new ClearCommand(),
            new VersionCommand(),
        ];
    }
}
