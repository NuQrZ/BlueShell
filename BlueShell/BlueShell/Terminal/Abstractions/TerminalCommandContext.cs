using System.Threading;

namespace BlueShell.Terminal.Abstractions
{
    public sealed class TerminalCommandContext(
        ITerminalOutput terminalOutput,
        IDataDisplay dataDisplay,
        CancellationToken cancellationToken)
    {
        public ITerminalOutput TerminalOutput = terminalOutput;
        public IDataDisplay DataDisplay = dataDisplay;
        public CancellationToken CancellationToken = cancellationToken;
    }
}
