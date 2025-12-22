using BlueShell.Terminal.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BlueShell.Terminal.Infrastructure
{
    public sealed class TerminalCommandDispatcher
    {
        private readonly Dictionary<string, ITerminalCommand> _commands;

        public TerminalCommandDispatcher(IEnumerable<ITerminalCommand> commands)
        {
            _commands = commands.ToDictionary(command => command.CommandName, command => command,
                StringComparer.OrdinalIgnoreCase);
        }

        public async Task ExecuteAsync(TerminalCommandContext context, string rawLine)
        {
            rawLine = rawLine.Trim();

            if (string.IsNullOrEmpty(rawLine))
            {
                return;
            }

            rawLine = rawLine.Replace("BlueShell > ", "", StringComparison.OrdinalIgnoreCase);

            Match match = Regex.Match(rawLine, @"^(\S+)");
            if (!match.Success)
            {
                return;
            }

            string commandName = match.Groups[1].Value;

            if (!_commands.TryGetValue(commandName, out var command))
            {
                context.Output.Print($"\n>> Unknown command: {commandName}\n\n", TerminalMessageKind.Error);
                return;
            }

            await command.ExecuteAsync(context, rawLine);
        }
    }
}
