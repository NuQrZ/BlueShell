using BlueShell.Terminal.Abstractions;
using System;
using System.Threading.Tasks;

namespace BlueShell.Terminal.Commands
{
    public sealed class ExitCommand : ITerminalCommand
    {
        public string CommandName => "Exit";
        public bool NoArguments => true;

        public async Task ExecuteAsync(TerminalCommandContext context, string? commandArguments = null)
        {
            context.TerminalOutput.WriteLine();
            context.TerminalOutput.WriteLine("Exiting BlueShell...", TerminalMessageKind.Error);
            await Task.Delay(1000);
            Environment.Exit(0);
        }
    }
}
