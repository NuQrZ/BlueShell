using BlueShell.Terminal.Abstractions;
using BlueShell.Terminal.Infrastructure;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BlueShell.ViewModel
{
    public sealed class TerminalViewModel(
        TerminalCommandDispatcher commandDispatcher,
        TerminalCommandContext commandContext)
    {
        private CancellationTokenSource? _cancellationTokenSource = new();

        private bool IsRunning { get; set; }
        private bool IsExiting { get; set; }

        public async Task<(bool, bool)> SubmitAsync(string commandLine)
        {
            if (IsExiting)
            {
                return (false, true);
            }

            if (commandLine == "Exit")
            {
                IsExiting = true;
            }

            if (IsRunning)
            {
                return (true, false);
            }

            IsRunning = true;

            if (_cancellationTokenSource != null)
            {
                await _cancellationTokenSource.CancelAsync();
            }

            _cancellationTokenSource = new CancellationTokenSource();

            try
            {
                Task task = commandDispatcher.ExecuteAsync(
                    commandContext, commandLine);
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

            return (IsRunning, IsExiting);
        }

        public void Cancel()
        {
            _cancellationTokenSource?.Cancel();
        }
    }
}
