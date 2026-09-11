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

        public (ITerminalCommand?, int) ResolveCommand(string commandLine)
        {
            commandLine = commandLine.Trim();

            if (string.IsNullOrEmpty(commandLine))
            {
                return (null, -1);
            }

            Match match = Regex.Match(commandLine, @"^(\S+)");
            if (!match.Success)
            {
                return (null, -2);
            }

            string commandName = match.Groups[1].Value;

            ITerminalCommand? terminalCommand = _allCommands.GetValueOrDefault(commandName);

            if (terminalCommand == null)
            {
                return (null, -3);
            }

            match = Regex.Match(commandLine, NoArgumentsRegex);
            string remainder = match.Groups[2].Value;

            if (terminalCommand.NoArguments && remainder != "")
            {
                return (terminalCommand, -4);
            }

            return (_allCommands.GetValueOrDefault(commandName), 0);
        }

        public bool IsExitCommand(string commandLine)
        {
            (ITerminalCommand?, int) result = ResolveCommand(commandLine);

            ITerminalCommand? terminalCommand = result.Item1;
            int returnValue = result.Item2;

            if (terminalCommand is null)
            {
                return false;
            }

            bool isExitCommand = terminalCommand.CommandName == "Exit";

            if (isExitCommand && returnValue == -4)
            {
                return false;
            }

            return isExitCommand;
        }

        public async Task ExecuteAsync(TerminalCommandContext context, string commandLine)
        {
            (ITerminalCommand?, int) result = ResolveCommand(commandLine);

            ITerminalCommand? terminalCommand = result.Item1;
            int returnValue = result.Item2;

            if (terminalCommand == null)
            {
                context.TerminalOutput.WriteLine("");
                context.TerminalOutput.WriteLine($">> Unknown command: {commandLine}!",
                    TerminalMessageKind.Error);
                context.TerminalOutput.WriteLine("");
                return;
            }
            if (returnValue == -4)
            {
                context.TerminalOutput.WriteLine("");
                context.TerminalOutput.WriteLine($">> Command does not take arguments, but argument: \"{commandLine.Replace(terminalCommand!.CommandName, "").Trim()}\" was passed!",
                    TerminalMessageKind.Error);
                context.TerminalOutput.WriteLine("");
                return;
            }

            await terminalCommand!.ExecuteAsync(context, commandLine);
        }
    }
}
