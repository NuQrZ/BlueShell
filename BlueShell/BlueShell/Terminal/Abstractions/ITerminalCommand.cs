using System.Threading.Tasks;

namespace BlueShell.Terminal.Abstractions
{
    public interface ITerminalCommand
    {
        string CommandName { get; }
        bool IsCancelling { get; }
        Task ExecuteAsync(TerminalCommandContext context, string commandLine);
    }
}