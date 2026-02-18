using System.Threading.Tasks;

namespace BlueShell.Terminal.Abstractions
{
    public interface ITerminalCommand
    {
        string CommandName { get; }
        Task ExecuteAsync(TerminalCommandContext context, string commandLine);
    }
}
