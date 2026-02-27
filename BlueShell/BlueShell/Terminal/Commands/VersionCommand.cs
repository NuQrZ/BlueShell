using BlueShell.Terminal.Abstractions;
using System.Threading.Tasks;

namespace BlueShell.Terminal.Commands
{
    internal class VersionCommand : ITerminalCommand
    {
        public string CommandName => "--Version";
        public Task ExecuteAsync(TerminalCommandContext context, string commandLine)
        {
            context.TerminalOutput.WriteLine(">> Currently installed version: [3.0.0.7].", TerminalMessageKind.Info);
            return Task.CompletedTask;
        }
    }
}
