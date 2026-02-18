using BlueShell.Terminal.Abstractions;
using System.Threading.Tasks;

namespace BlueShell.Terminal.Commands
{
    public sealed class ClearDisplayCommand : ITerminalCommand
    {
        public string CommandName => "ClearDisplayCommand";
        public Task ExecuteAsync(TerminalCommandContext context, string commandLine)
        {
            context.TerminalOutput.Clear();
            return Task.CompletedTask;
        }
    }
}
