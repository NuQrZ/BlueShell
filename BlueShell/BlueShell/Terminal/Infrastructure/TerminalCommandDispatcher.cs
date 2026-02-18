using BlueShell.Terminal.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BlueShell.Terminal.Infrastructure
{
    public sealed class TerminalCommandDispatcher(IEnumerable<ITerminalCommand> commands)
    {
        private readonly Dictionary<string, ITerminalCommand> _commands =
            commands.ToDictionary(command => command.CommandName, command => command);

        public async Task ExecuteAsync(TerminalCommandContext context, string commandLine)
        {
            commandLine = commandLine.Trim();

            if (string.IsNullOrEmpty(commandLine))
            {
                return;
            }

            commandLine = commandLine.Replace("Shell > ", "", StringComparison.OrdinalIgnoreCase);

            Match match = Regex.Match(commandLine, @"^(\S+)");
            if (!match.Success)
            {
                return;
            }

            string commandName = match.Groups[1].Value;

            if (!_commands.TryGetValue(commandName, out ITerminalCommand? terminalCommand))
            {
                context.TerminalOutput.WriteLine($">> Unknown command: {commandLine}!",
                    TerminalMessageKind.Error);
                return;
            }

            await terminalCommand.ExecuteAsync(context, commandLine);
        }
    }
}
