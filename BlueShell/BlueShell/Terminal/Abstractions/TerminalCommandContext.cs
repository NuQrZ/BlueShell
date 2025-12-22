using System.Threading;

namespace BlueShell.Terminal.Abstractions
{
    public sealed class TerminalCommandContext(
        ITerminalOutput output,
        IDataDisplay dataDisplay,
        CancellationToken cancellationToken)
    {
        public ITerminalOutput Output { get; } = output;
        public IDataDisplay DataDisplay { get; } = dataDisplay;
        public CancellationToken CancellationToken { get; } = cancellationToken;
    }
}
