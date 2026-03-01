using BlueShell.Terminal.Abstractions;
using Microsoft.UI;
using System.Threading.Tasks;
using Windows.UI.Text;

namespace BlueShell.Terminal.Commands
{
    public sealed class SimulateCommand : ITerminalCommand
    {
        public string CommandName => "Simulate";
        public async Task ExecuteAsync(TerminalCommandContext context, string commandLine)
        {
            for (int i = 0; i <= 100_000; i++)
            {
                context.TerminalOutput.Line()
                    .PrintOutput($"Simulating long-running operation: ")
                    .Info($"{i} ")
                    .PrintOutput("/ 100.000\n", extraColor: Colors.Purple)
                    .Commit();
            }

            context.TerminalOutput.Line()
                .Info("Simulate command done!\n", new FontWeight(200))
                .Success("Success!\n", new FontWeight(900))
                .Commit();
        }
    }
}
