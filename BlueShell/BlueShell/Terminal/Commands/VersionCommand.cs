using BlueShell.Terminal.Abstractions;
using System.Threading.Tasks;

namespace BlueShell.Terminal.Commands
{
    public sealed class VersionCommand : ITerminalCommand
    {
        public string CommandName => "--Version";
        public Task ExecuteAsync(TerminalCommandContext context, string commandLine)
        {
            context.Output.PrintLine("\nCurrently installed version: [3.0.0.3].\n", TerminalMessageKind.Info);
            return Task.CompletedTask;
        }
    }
}
