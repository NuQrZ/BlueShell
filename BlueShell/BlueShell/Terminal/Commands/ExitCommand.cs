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
            context.TerminalOutput.WriteLine("Exiting BlueShell...", TerminalMessageKind.Info);
            await Task.Delay(50000);
            Environment.Exit(0);
        }
    }
}
