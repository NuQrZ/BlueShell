using BlueShell.Terminal.Abstractions;
using BlueShell.Terminal.Infrastructure;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BlueShell.ViewModel
{
    public sealed class TerminalViewModel(
        TerminalCommandDispatcher commandDispatcher,
        Func<TerminalCommandContext> contextFactory)
    {
        private CancellationTokenSource? _cancellationTokenSource = new();

        private bool IsRunning { get; set; }
        private bool IsExiting { get; set; }

        public async Task SubmitAsync(string commandLine)
        {
            if (IsExiting)
            {
                return;
            }

            if (commandLine.Trim() == "Exit")
            {
                IsExiting = true;
            }

            if (IsRunning)
            {
                return;
            }

            IsRunning = true;

            if (_cancellationTokenSource != null)
            {
                await _cancellationTokenSource.CancelAsync();
            }

            _cancellationTokenSource = new CancellationTokenSource();

            TerminalCommandContext commandContext = contextFactory();

            try
            {
                Task task = commandDispatcher.ExecuteAsync(
                    new TerminalCommandContext(commandContext.TerminalOutput, commandContext.DataDisplay, _cancellationTokenSource.Token),
                    commandLine);
                await task;
            }
            catch (OperationCanceledException)
            {
                commandContext.TerminalOutput.WriteLine("\n>> Operation Canceled.\n", TerminalMessageKind.Info);

            }
            catch (Exception exception)
            {
                commandContext.TerminalOutput.WriteLine($"\n>> Error: {exception.Message}\n", TerminalMessageKind.Error);
            }
            finally
            {
                IsRunning = false;
            }
        }

        public void Cancel()
        {
            _cancellationTokenSource?.Cancel();
            IsRunning = false;
        }
    }
}
