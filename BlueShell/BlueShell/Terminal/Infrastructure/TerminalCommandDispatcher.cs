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

        private ITerminalCommand? ResolveCommand(string commandLine)
        {
            commandLine = commandLine.Trim();

            if (string.IsNullOrEmpty(commandLine))
            {
                return null;
            }

            commandLine = commandLine.Replace("Shell > ", "", StringComparison.OrdinalIgnoreCase);

            Match match = Regex.Match(commandLine, @"^(\S+)");
            if (!match.Success)
            {
                return null;
            }

            string commandName = match.Groups[1].Value;

            if (!_commands.TryGetValue(commandName, out ITerminalCommand? terminalCommand))
            {
                return null;
            }

            return terminalCommand;
        }

        public bool IsInterruptCommand(string commandLine)
        {
            ITerminalCommand? terminalCommand = ResolveCommand(commandLine);

            if (terminalCommand == null)
            {
                return false;
            }

            return terminalCommand.IsCancelling;
        }

        public bool IsExitCommand(string commandLine)
        {
            ITerminalCommand? terminalCommand = ResolveCommand(commandLine);

            if (terminalCommand == null)
            {
                return false;
            }

            return terminalCommand.CommandName == "Exit";
        }

        public async Task ExecuteAsync(TerminalCommandContext context, string commandLine)
        {
            ITerminalCommand? terminalCommand = ResolveCommand(commandLine);

            if (terminalCommand == null)
            {
                context.TerminalOutput.WriteLine($">> Unknown command: {commandLine}!",
                    TerminalMessageKind.Error);
                return;
            }

            await terminalCommand.ExecuteAsync(context, commandLine);
        }
    }
}