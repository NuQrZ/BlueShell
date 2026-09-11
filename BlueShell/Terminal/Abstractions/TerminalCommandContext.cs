using BlueShell.Model;
using System.Threading;

namespace BlueShell.Terminal.Abstractions
{
    public sealed class TerminalCommandContext(
        ITerminalOutput terminalOutput,
        TabModel? tabModel,
        CancellationToken cancellationToken)
    {
        public ITerminalOutput TerminalOutput { get; } = terminalOutput;
        public TabModel? TabModel { get; } = tabModel;
        public CancellationToken CancellationToken { get; } = cancellationToken;
    }
}
