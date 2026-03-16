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
            if (string.IsNullOrWhiteSpace(commandLine) || IsExiting)
            {
                return;
            }

            if (commandDispatcher.IsExitCommand(commandLine))
            {
                IsExiting = true;
            }

            TerminalCommandContext commandContext = contextFactory();

            if (IsRunning)
            {
                if (!commandDispatcher.IsInterruptCommand(commandLine))
                {
                    return;
                }

                Cancel();

                try
                {
                    await commandDispatcher.ExecuteAsync(
                        new TerminalCommandContext(
                            commandContext.TerminalOutput,
                            commandContext.DataDisplay,
                            commandContext.TabModel,
                            CancellationToken.None),
                        commandLine);
                }
                catch (Exception exception)
                {
                    commandContext.TerminalOutput.WriteLine(
                        $">> Error: {exception.Message}\n",
                        TerminalMessageKind.Error);
                }

                return;
            }

            if (_cancellationTokenSource != null)
            {
                await _cancellationTokenSource.CancelAsync();
            }
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = new CancellationTokenSource();

            IsRunning = true;

            try
            {
                await commandDispatcher.ExecuteAsync(
                    new TerminalCommandContext(
                        commandContext.TerminalOutput,
                        commandContext.DataDisplay,
                        commandContext.TabModel,
                        _cancellationTokenSource.Token),
                    commandLine);
            }
            catch (OperationCanceledException)
            {
                commandContext.TerminalOutput.WriteLine(
                    ">> Operation Canceled.\n",
                    TerminalMessageKind.Info);
            }
            catch (Exception exception)
            {
                commandContext.TerminalOutput.WriteLine(
                    $">> Error: {exception.Message}\n",
                    TerminalMessageKind.Error);
            }
            finally
            {
                IsRunning = false;
            }
        }

        public void Cancel()
        {
            _cancellationTokenSource?.Cancel();
        }
    }
}