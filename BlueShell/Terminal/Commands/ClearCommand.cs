using BlueShell.Terminal.Abstractions;
using System.Threading.Tasks;

namespace BlueShell.Terminal.Commands
{
    public sealed class ClearCommand : ITerminalCommand
    {
        public string CommandName => "Clear";
        public bool NoArguments => true;
        public bool IsInterrupting => true;

        public Task Execute(TerminalCommandContext context, string? commandArguments = null)
        {
            context.TerminalOutput.Clear();
            return Task.FromResult(0);
        }
    }
}
