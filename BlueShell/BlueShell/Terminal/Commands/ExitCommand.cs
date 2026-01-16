using BlueShell.Terminal.Abstractions;
using System;
using System.Threading.Tasks;

namespace BlueShell.Terminal.Commands
{
    public sealed class ExitCommand : ITerminalCommand
    {
        public string CommandName => "Exit";
        public async Task ExecuteAsync(TerminalCommandContext context, string commandLine)
        {
            context.Output.PrintLine("\n>> Exiting BlueShell...\n", TerminalMessageKind.Info);
            await Task.Delay(500);
            Environment.Exit(0);
        }
    }
}
