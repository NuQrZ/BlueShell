using BlueShell.Services;
using BlueShell.Terminal.Abstractions;
using BlueShell.Terminal.Infrastructure;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BlueShell.ViewModel
{
    public sealed class TerminalViewModel(
        TerminalCommandDispatcher commandDispatcher,
        Func<TerminalCommandContext> contextFactory,
        IClipboardService clipboardService)
    {
        private CancellationTokenSource? _cancellationTokenSource;
        private readonly StringBuilder _currentLine = new();
        private bool _isExiting = false;

        public int? SelectionAnchor { get; private set; }
        public int CaretPosition { get; private set; } = 0;
        public int SelectionStart => SelectionAnchor.HasValue ? Math.Min(SelectionAnchor.Value, CaretPosition) : CaretPosition;
        public int SelectionEnd => SelectionAnchor.HasValue ? Math.Max(SelectionAnchor.Value, CaretPosition) : CaretPosition;
        public int Length => _currentLine.Length;

        public bool IsCommandRunning { get; private set; } = false;
        public bool HasSelection => SelectionAnchor.HasValue && SelectionAnchor.Value != CaretPosition;

        public string CurrentLine => _currentLine.ToString();

        private void DeleteSelection()
        {
            if (!HasSelection)
            {
                SelectionAnchor = null;
                return;
            }

            int selectionLength = SelectionEnd - SelectionStart;

            _currentLine.Remove(SelectionStart, selectionLength);

            CaretPosition = SelectionStart;
            SelectionAnchor = null;
        }

        public void InsertText(string text)
        {
            if (HasSelection)
            {
                DeleteSelection();
            }
            else
            {
                SelectionAnchor = null;
            }

            _currentLine.Insert(CaretPosition, text);
            CaretPosition += text.Length;
        }

        public void MoveCaretLeft()
        {
            if (HasSelection)
            {
                CaretPosition = SelectionStart;
                SelectionAnchor = null;
                return;
            }

            SelectionAnchor = null;

            if (CaretPosition > 0)
            {
                CaretPosition--;
            }
        }

        public void MoveCaretRight()
        {
            if (HasSelection)
            {
                CaretPosition = SelectionEnd;
                SelectionAnchor = null;
                return;
            }

            SelectionAnchor = null;

            if (CaretPosition < Length)
            {
                CaretPosition++;
            }
        }

        public void SelectCaretLeft()
        {
            if (CaretPosition <= 0)
            {
                return;
            }

            SelectionAnchor ??= CaretPosition;

            CaretPosition--;
        }

        public void SelectCaretRight()
        {
            if (CaretPosition >= Length)
            {
                return;
            }

            SelectionAnchor ??= CaretPosition;

            CaretPosition++;
        }

        public void MoveCaretWordLeft()
        {
            if (HasSelection)
            {
                CaretPosition = SelectionStart;
                SelectionAnchor = null;
                return;
            }

            SelectionAnchor = null;

            if (CaretPosition == 0)
            {
                return;
            }

            while (CaretPosition > 0 && char.IsWhiteSpace(_currentLine[CaretPosition - 1]))
            {
                CaretPosition--;
            }

            while (CaretPosition > 0 && !char.IsWhiteSpace(_currentLine[CaretPosition - 1]))
            {
                CaretPosition--;
            }
        }

        public void MoveCaretWordRight()
        {
            if (HasSelection)
            {
                CaretPosition = SelectionEnd;
                SelectionAnchor = null;
                return;
            }

            SelectionAnchor = null;

            if (CaretPosition >= Length)
            {
                return;
            }

            while (CaretPosition < Length && char.IsWhiteSpace(_currentLine[CaretPosition]))
            {
                CaretPosition++;
            }

            while (CaretPosition < Length && !char.IsWhiteSpace(_currentLine[CaretPosition]))
            {
                CaretPosition++;
            }
        }

        public void Delete()
        {
            if (HasSelection)
            {
                DeleteSelection();
                return;
            }

            SelectionAnchor = null;

            if (CaretPosition >= Length)
            {
                return;
            }

            _currentLine.Remove(CaretPosition, 1);
        }

        public void ControlDelete()
        {
            if (HasSelection)
            {
                DeleteSelection();
                return;
            }

            SelectionAnchor = null;

            if (CaretPosition >= _currentLine.Length)
            {
                return;
            }

            int endPosition = CaretPosition;

            if (char.IsWhiteSpace(_currentLine[endPosition]))
            {
                while (endPosition < _currentLine.Length && char.IsWhiteSpace(_currentLine[endPosition]))
                {
                    endPosition++;
                }
            }
            else
            {
                while (endPosition < _currentLine.Length && !char.IsWhiteSpace(_currentLine[endPosition]))
                {
                    endPosition++;
                }
            }

            _currentLine.Remove(CaretPosition, endPosition - CaretPosition);
        }

        public void SelectWordLeft()
        {
            if (CaretPosition == 0)
            {
                return;
            }

            SelectionAnchor ??= CaretPosition;

            while (CaretPosition > 0 && char.IsWhiteSpace(_currentLine[CaretPosition - 1]))
            {
                CaretPosition--;
            }

            while (CaretPosition > 0 && !char.IsWhiteSpace(_currentLine[CaretPosition - 1]))
            {
                CaretPosition--;
            }
        }

        public void SelectWordRight()
        {
            if (CaretPosition >= Length)
            {
                return;
            }

            SelectionAnchor ??= CaretPosition;

            while (CaretPosition < Length && char.IsWhiteSpace(_currentLine[CaretPosition]))
            {
                CaretPosition++;
            }

            while (CaretPosition < Length && !char.IsWhiteSpace(_currentLine[CaretPosition]))
            {
                CaretPosition++;
            }
        }

        public void SelectAll()
        {
            if (Length == 0)
            {
                SelectionAnchor = null;
                CaretPosition = 0;
                return;
            }

            SelectionAnchor = 0;
            CaretPosition = Length;
        }

        public void ClearSelection()
        {
            SelectionAnchor = null;
        }

        public void Backspace()
        {
            if (HasSelection)
            {
                DeleteSelection();
                return;
            }

            SelectionAnchor = null;

            if (CaretPosition > 0)
            {
                _currentLine.Remove(CaretPosition - 1, 1);
                CaretPosition--;
            }
        }

        public void ControlBackspace()
        {
            if (HasSelection)
            {
                DeleteSelection();
                return;
            }

            SelectionAnchor = null;

            if (CaretPosition == 0)
            {
                return;
            }

            int endPosition = CaretPosition;

            if (char.IsWhiteSpace(_currentLine[CaretPosition - 1]))
            {
                while (CaretPosition > 0 && char.IsWhiteSpace(_currentLine[CaretPosition - 1]))
                {
                    CaretPosition--;
                }
            }
            else
            {
                while (CaretPosition > 0 && !char.IsWhiteSpace(_currentLine[CaretPosition - 1]))
                {
                    CaretPosition--;
                }
            }

            int length = endPosition - CaretPosition;
            _currentLine.Remove(CaretPosition, length);
        }

        public void GoToHome()
        {
            SelectionAnchor = null;
            CaretPosition = 0;
        }

        public void SelectHome()
        {
            if (CaretPosition <= 0)
            {
                return;
            }

            SelectionAnchor ??= CaretPosition;
            CaretPosition = 0;
        }

        public void GoToEnd()
        {
            SelectionAnchor = null;
            CaretPosition = Length;
        }

        public void SelectEnd()
        {
            if (CaretPosition >= Length)
            {
                return;
            }

            SelectionAnchor ??= CaretPosition;
            CaretPosition = Length;
        }

        public void Copy()
        {
            if (!HasSelection)
            {
                return;
            }

            string text = CurrentLine[SelectionStart..SelectionEnd];

            clipboardService.Copy(text);
        }

        public async Task PasteAsync()
        {
            string copiedText = await clipboardService.GetTextAsync();

            copiedText = copiedText.Replace("\r", "").Replace("\n", " ");

            InsertText(copiedText);
        }

        public void Cut()
        {
            if (!HasSelection)
            {
                return;
            }

            string text = CurrentLine[SelectionStart..SelectionEnd];

            clipboardService.Copy(text);

            DeleteSelection();
        }

        public void StartPointerSelection(int caretPosition)
        {
            CaretPosition = Math.Clamp(caretPosition, 0, Length);
            SelectionAnchor = CaretPosition;
        }

        public void UpdatePointerSelection(int caretPosition)
        {
            CaretPosition = Math.Clamp(caretPosition, 0, Length);
        }

        public void ReleasePointerSelection()
        {
            if (!HasSelection)
            {
                SelectionAnchor = null;
            }
        }

        public string TakeCurrentLine()
        {
            string currentLine = _currentLine.ToString();

            _currentLine.Clear();

            SelectionAnchor = null;
            CaretPosition = 0;

            return currentLine;
        }

        public async Task SubmitAsync(string commandLine)
        {
            if (string.IsNullOrWhiteSpace(commandLine) || _isExiting || IsCommandRunning)
            {
                return;
            }

            if (commandDispatcher.IsExitCommand(commandLine))
            {
                _isExiting = true;
            }

            TerminalCommandContext commandContext = contextFactory();

            if (_cancellationTokenSource != null)
            {
                await _cancellationTokenSource.CancelAsync();
            }

            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = new CancellationTokenSource();

            IsCommandRunning = true;

            try
            {
                await commandDispatcher.ExecuteAsync(
                    new TerminalCommandContext(
                        commandContext.TerminalOutput,
                        commandContext.TabModel,
                        _cancellationTokenSource.Token),
                    commandLine);
            }
            catch (OperationCanceledException)
            {
                commandContext.TerminalOutput.WriteLine("");
                commandContext.TerminalOutput.WriteLine(
                    ">> Operation Canceled.",
                    TerminalMessageKind.Info);
                commandContext.TerminalOutput.WriteLine("");
            }
            catch (Exception exception)
            {
                commandContext.TerminalOutput.WriteLine("");
                commandContext.TerminalOutput.WriteLine(
                    $">> Error: {exception.Message}",
                    TerminalMessageKind.Error);
                commandContext.TerminalOutput.WriteLine("");
            }
            finally
            {
                IsCommandRunning = false;
            }
        }

        public void Cancel()
        {
            _cancellationTokenSource?.Cancel();
        }
    }
}
