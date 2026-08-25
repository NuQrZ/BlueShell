using BlueShell.Terminal.Abstractions;
using System.Threading.Tasks;

namespace BlueShell.Terminal.Commands
{
    public sealed class VersionCommand : ITerminalCommand
    {
        public string CommandName => "--Version";
        public bool NoArguments => true;
        public bool IsInterrupting => true;

        public Task Execute(TerminalCommandContext context, string? commandArguments)
        {
            context.TerminalOutput.WriteLine("");
            context.TerminalOutput.WriteLine(">> Currently installed version: [3.0.0.3].", TerminalMessageKind.Info);
            context.TerminalOutput.WriteLine("");
            return Task.FromResult(0);
        }
    }
}
