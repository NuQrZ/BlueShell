using BlueShell.Terminal.Abstractions;
using System.Threading.Tasks;

namespace BlueShell.Terminal.Commands
{
    public sealed class ClearCommand : ITerminalCommand
    {
        public string CommandName => "Clear";
        public Task ExecuteAsync(TerminalCommandContext context, string commandLine)
        {
            context.TerminalOutput.Clear();
            return Task.CompletedTask;
        }
    }
}
