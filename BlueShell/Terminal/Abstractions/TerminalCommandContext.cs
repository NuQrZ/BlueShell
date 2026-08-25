using BlueShell.Model;
using System.Threading;

namespace BlueShell.Terminal.Abstractions
{
    public sealed class TerminalCommandContext(
        ITerminalOutput terminalOutput,
        TabModel? tabModel,
        CancellationToken cancellationToken)
    {
        public readonly ITerminalOutput TerminalOutput = terminalOutput;
        public readonly TabModel? TabModel = tabModel;
        public CancellationToken CancellationToken = cancellationToken;
    }
}
