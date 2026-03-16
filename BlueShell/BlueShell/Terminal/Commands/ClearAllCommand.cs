using BlueShell.Terminal.Abstractions;
using System.Threading.Tasks;

namespace BlueShell.Terminal.Commands
{
    public sealed class ClearAllCommand : ITerminalCommand
    {
        public string CommandName => "ClearAll";
        public bool IsCancelling => true;

        public Task ExecuteAsync(TerminalCommandContext context, string commandLine)
        {
            context.DataDisplay.Clear();
            context.TerminalOutput.Clear();
            context.TabModel?.ClearPath();
            return Task.CompletedTask;
        }
    }
}