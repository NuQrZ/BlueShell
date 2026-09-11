using BlueShell.Terminal.Abstractions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlueShell.Terminal.Commands
{
    public sealed class SimulateCommand : ITerminalCommand
    {
        public string CommandName => "Simulate";

        public bool NoArguments => true;

        public async Task ExecuteAsync(TerminalCommandContext context, string? commandArguments = null)
        {
            const int totalLines = 10_000;
            const int linesPerFrame = 777;

            List<string> batch = [with(linesPerFrame)];

            context.TerminalOutput.WriteLine("");

            for (int i = 0; i <= totalLines; i++)
            {
                context.CancellationToken.ThrowIfCancellationRequested();

                batch.Add(i.ToString());

                if (batch.Count == linesPerFrame)
                {
                    context.TerminalOutput.WriteLines(batch, TerminalMessageKind.PrintOutput);

                    batch.Clear();

                    await Task.Delay(5, context.CancellationToken);
                }
            }

            if (batch.Count > 0)
            {
                context.TerminalOutput.WriteLines(batch, TerminalMessageKind.PrintOutput);
            }

            context.TerminalOutput.WriteLine("");
            context.TerminalOutput.Write("Info: ", TerminalMessageKind.Info);
            context.TerminalOutput.Write("Simulation complete!", TerminalMessageKind.Success);
            context.TerminalOutput.WriteLine("");
            context.TerminalOutput.WriteLine("");
        }
    }
}
