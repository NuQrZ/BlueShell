using BlueShell.Terminal.Abstractions;
using System.Threading.Tasks;

namespace BlueShell.Terminal.Commands
{
    public sealed class ClearDisplayCommand : ITerminalCommand
    {
        public string CommandName => "ClearDisplay";
        public Task ExecuteAsync(TerminalCommandContext context, string commandLine)
        {
            context.DataDisplay.Clear();
            context.Output.PrintLine("\n\n>> Data Display Cleared!\n", TerminalMessageKind.Success);
            return Task.CompletedTask;
        }
    }
}