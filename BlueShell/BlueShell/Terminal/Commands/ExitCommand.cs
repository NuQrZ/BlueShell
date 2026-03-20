using BlueShell.Terminal.Abstractions;
using System;
using System.Threading.Tasks;

namespace BlueShell.Terminal.Commands
{
    public sealed class ExitCommand : ITerminalCommand
    {
        public string CommandName => "Exit";
        public bool IsInterrupting => true;

        public async Task ExecuteAsync(TerminalCommandContext context, string commandLine)
        {
            context.TerminalOutput.WriteLine("Exiting BlueShell...", TerminalMessageKind.Error);
            await Task.Delay(50);
            Environment.Exit(0);
        }
    }
}