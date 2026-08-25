using BlueShell.Terminal.Abstractions;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BlueShell.Terminal.Infrastructure
{
    public sealed class TerminalCommandDispatcher(IEnumerable<ITerminalCommand> allCommands)
    {
        private const string NoArgumentsRegex = @"\A(?<command>(?:--)?[a-zA-Z]+)(?<remainder>.*)\z";
        private readonly Dictionary<string, ITerminalCommand> _allCommands =
            allCommands.ToDictionary(command => command.CommandName, command => command);

        private ITerminalCommand? ResolveCommand(string commandLine)
        {
            commandLine = commandLine.Trim();

            if (string.IsNullOrEmpty(commandLine))
            {
                return null;
            }

            Match match = Regex.Match(commandLine, @"^(\S+)");
            if (!match.Success)
            {
                return null;
            }

            string commandName = match.Groups[1].Value;

            return _allCommands.GetValueOrDefault(commandName);
        }

        public bool IsInterruptCommand(string commandLine)
        {
            ITerminalCommand? terminalCommand = ResolveCommand(commandLine);

            return terminalCommand?.IsInterrupting ?? true;
        }

        public bool IsExitCommand(string commandLine)
        {
            ITerminalCommand? terminalCommand = ResolveCommand(commandLine);

            if (terminalCommand is null)
            {
                return false;
            }

            return terminalCommand.CommandName == "Exit";
        }

        public async Task ExecuteAsync(TerminalCommandContext context,
                                       string commandLine)
        {
            ITerminalCommand? terminalCommand = ResolveCommand(commandLine);

            if (terminalCommand is null)
            {
                context.TerminalOutput.WriteLine("");
                context.TerminalOutput.WriteLine($">> Unknown command: {commandLine}!",
                    TerminalMessageKind.Error);
                context.TerminalOutput.WriteLine("");
                return;
            }

            await terminalCommand.Execute(context, commandLine);
        }
    }
}
