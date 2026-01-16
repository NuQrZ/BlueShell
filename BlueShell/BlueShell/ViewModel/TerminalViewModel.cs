using BlueShell.Terminal.Abstractions;
using BlueShell.Terminal.Infrastructure;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BlueShell.ViewModel
{
    public sealed class TerminalViewModel(
        TerminalCommandDispatcher dispatcher,
        Func<TerminalCommandContext> contextFactory)
    {
        private CancellationTokenSource? _cancellationTokenSource;

        private bool IsRunning { get; set; }

        public async Task SubmitAsync(string commandLine)
        {
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

            TerminalCommandContext context = contextFactory();

            try
            {
                await dispatcher.ExecuteAsync(
                    new TerminalCommandContext(context.Output, context.DataDisplay, context.CancellationToken),
                    commandLine);
            }
            catch (OperationCanceledException)
            {
                context.Output.PrintLine("\n>> Operation Canceled.\n", TerminalMessageKind.Info);
            }
            catch (Exception exception)
            {
                context.Output.PrintLine($"\n>> Error: {exception.Message}\n", TerminalMessageKind.Error);
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
