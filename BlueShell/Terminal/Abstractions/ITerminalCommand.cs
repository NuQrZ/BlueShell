using System.Threading.Tasks;

namespace BlueShell.Terminal.Abstractions
{
    public interface ITerminalCommand
    {
        public string CommandName { get; }
        public bool NoArguments { get; }
        public bool IsInterrupting { get; }
        public Task Execute(TerminalCommandContext context, string? commandArguments = null);
    }
}
