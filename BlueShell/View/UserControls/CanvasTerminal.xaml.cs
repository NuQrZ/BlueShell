using BlueShell.Helpers;
using BlueShell.Model;
using BlueShell.Model.Terminal;
using BlueShell.Services;
using BlueShell.Terminal;
using BlueShell.Terminal.Abstractions;
using BlueShell.Terminal.Implementations;
using BlueShell.Terminal.Infrastructure;
using BlueShell.ViewModel;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.UI.Input;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Text;

namespace BlueShell.View.UserControls
{
    public sealed partial class CanvasTerminal : UserControl
    {
        private const double PageScrollFactor = 2;

        private bool _refreshPending;
        private bool _scrollToBottomPending;
        private bool _followingOutput = true;
        private bool _updatingScrollView;
        private bool _isPointerSelecting = false;

        private readonly DispatcherTimer _dispatcherTimer = new()
        {
            Interval = TimeSpan.FromMilliseconds(400)
        };

        private TabModel? _tabModel;

        private readonly ITerminalOutput _terminalOutput;
        private readonly TerminalViewModel _terminalViewModel;
        private readonly TerminalRenderer _terminalRenderer;
        private readonly TerminalBuffer _terminalBuffer = new();

        public CanvasTerminal()
        {
            InitializeComponent();

            ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.IBeam);

            _terminalOutput = new TerminalOutput(_terminalBuffer, () => ActualTheme);

            TerminalCommandDispatcher dispatcher = new(TerminalCommandRegistry.CreateDefault());

            _terminalViewModel = new TerminalViewModel(
                dispatcher,
                () => new TerminalCommandContext(_terminalOutput, _tabModel, CancellationToken.None),
                new ClipboardService());

            _terminalRenderer = new TerminalRenderer(_terminalBuffer, _terminalViewModel, () => ActualTheme);

            _dispatcherTimer.Tick += DispatcherTimer_Tick;
            _terminalBuffer.Changed += TerminalBuffer_Changed;
        }

        public void BuildTabModel(TabModel? tabModel)
        {
            _tabModel = tabModel;
        }

        private void UpdateScrollView(bool isCommandRunning)
        {
            float lineHeight = _terminalRenderer.LineHeight;

            int visibleLineCount = _terminalBuffer.Count + (isCommandRunning ? 0 : 1);
            double contentHeight = TerminalRenderer.PaddingTop + visibleLineCount * lineHeight + TerminalRenderer.PaddingTop;

            Terminal.Height = Math.Max(TerminalScrollView.ViewportHeight, contentHeight);
        }

        private void RefreshTerminal(bool scrollToBottom)
        {
            bool shouldScrollToBottom = scrollToBottom && _followingOutput;

            _updatingScrollView = true;

            try
            {
                UpdateScrollView(_terminalViewModel.IsCommandRunning);

                Terminal.Invalidate();
                TerminalScrollView.UpdateLayout();

                if (!shouldScrollToBottom)
                {
                    return;
                }

                TerminalScrollView.ChangeView(null, TerminalScrollView.ScrollableHeight, null, true);

                _followingOutput = true;
            }
            finally
            {
                _updatingScrollView = false;
            }
        }

        private void RequestRefresh(bool scrollToBottom)
        {
            _scrollToBottomPending |= scrollToBottom;

            if (_refreshPending)
            {
                return;
            }

            _refreshPending = true;

            DispatcherQueue.TryEnqueue(() =>
            {
                _refreshPending = false;

                bool shouldScrollToBottom = _scrollToBottomPending;
                _scrollToBottomPending = false;

                RefreshTerminal(shouldScrollToBottom);
            });
        }

        private async Task SubmitLineAsync()
        {
            string currentLine = _terminalViewModel.TakeCurrentLine();

            TerminalLine terminalLine = new();

            terminalLine.AddSegment(
                new TerminalLineSegment(
                    TerminalRenderer.Prompt,
                    _terminalRenderer.DefaultColor,
                    FontWeights.Normal,
                    FontStyle.Normal));

            terminalLine.AddSegment(
                new TerminalLineSegment(
                    currentLine,
                    TerminalUtilities.GetCommandColor(currentLine, ActualTheme, _terminalRenderer.DefaultColor),
                    FontWeights.Normal,
                    FontStyle.Normal));

            _terminalBuffer.AddTerminalLine(terminalLine);

            _terminalRenderer.CaretVisible = false;

            try
            {
                await _terminalViewModel.SubmitAsync(currentLine);
            }
            finally
            {
                _terminalRenderer.CaretVisible = true;
                RefreshTerminal(true);
            }
        }

        private async Task<bool> HandleKeyActionAsync(TerminalKeyAction terminalKeyAction)
        {
            switch (terminalKeyAction)
            {
                case TerminalKeyAction.None:
                    return false;

                case TerminalKeyAction.BlockInput:
                    return true;

                case TerminalKeyAction.ZoomIn:
                    _terminalRenderer.ZoomIn();
                    RefreshTerminal(true);
                    return true;

                case TerminalKeyAction.ZoomOut:
                    _terminalRenderer.ZoomOut();
                    RefreshTerminal(true);
                    return true;

                case TerminalKeyAction.Cancel:
                    _terminalViewModel.Cancel();
                    return true;

                case TerminalKeyAction.MoveCaretLeft:
                    _terminalViewModel.MoveCaretLeft();
                    Terminal.Invalidate();
                    return true;

                case TerminalKeyAction.MoveCaretRight:
                    _terminalViewModel.MoveCaretRight();
                    Terminal.Invalidate();
                    return true;

                case TerminalKeyAction.MoveCaretWordLeft:
                    _terminalViewModel.MoveCaretWordLeft();
                    Terminal.Invalidate();
                    return true;

                case TerminalKeyAction.MoveCaretWordRight:
                    _terminalViewModel.MoveCaretWordRight();
                    Terminal.Invalidate();
                    return true;

                case TerminalKeyAction.Delete:
                    _terminalViewModel.Delete();
                    Terminal.Invalidate();
                    return true;

                case TerminalKeyAction.ControlDelete:
                    _terminalViewModel.ControlDelete();
                    Terminal.Invalidate();
                    return true;

                case TerminalKeyAction.SelectCaretLeft:
                    _terminalViewModel.SelectCaretLeft();
                    Terminal.Invalidate();
                    return true;

                case TerminalKeyAction.SelectCaretRight:
                    _terminalViewModel.SelectCaretRight();
                    Terminal.Invalidate();
                    return true;

                case TerminalKeyAction.SelectWordLeft:
                    _terminalViewModel.SelectWordLeft();
                    Terminal.Invalidate();
                    return true;

                case TerminalKeyAction.SelectWordRight:
                    _terminalViewModel.SelectWordRight();
                    Terminal.Invalidate();
                    return true;

                case TerminalKeyAction.SelectHome:
                    _terminalViewModel.SelectHome();
                    Terminal.Invalidate();
                    return true;

                case TerminalKeyAction.SelectEnd:
                    _terminalViewModel.SelectEnd();
                    Terminal.Invalidate();
                    return true;

                case TerminalKeyAction.SelectAll:
                    _terminalViewModel.SelectAll();
                    Terminal.Invalidate();
                    return true;

                case TerminalKeyAction.ClearSelection:
                    _terminalViewModel.ClearSelection();
                    Terminal.Invalidate();
                    return true;

                case TerminalKeyAction.Copy:
                    _terminalViewModel.Copy();
                    return true;

                case TerminalKeyAction.Paste:
                    await _terminalViewModel.PasteAsync();
                    Terminal.Invalidate();
                    return true;

                case TerminalKeyAction.Cut:
                    _terminalViewModel.Cut();
                    return true;

                case TerminalKeyAction.GoToHome:
                    _terminalViewModel.GoToHome();
                    Terminal.Invalidate();
                    return true;

                case TerminalKeyAction.GoToEnd:
                    _terminalViewModel.GoToEnd();
                    Terminal.Invalidate();
                    return true;

                case TerminalKeyAction.PageUp:
                    ScrollPageUp();
                    return true;

                case TerminalKeyAction.PageDown:
                    ScrollPageDown();
                    return true;

                case TerminalKeyAction.ArrowUp:
                    _terminalViewModel.HistoryPrevious();
                    Terminal.Invalidate();
                    return true;

                case TerminalKeyAction.ArrowDown:
                    _terminalViewModel.HistoryNext();
                    Terminal.Invalidate();
                    return true;

                case TerminalKeyAction.Backspace:
                    _terminalViewModel.Backspace();
                    Terminal.Invalidate();
                    return true;

                case TerminalKeyAction.ControlBackspace:
                    _terminalViewModel.ControlBackspace();
                    Terminal.Invalidate();
                    return true;

                case TerminalKeyAction.Undo:
                    _terminalViewModel.Undo();
                    Terminal.Invalidate();
                    return true;

                case TerminalKeyAction.Redo:
                    _terminalViewModel.Redo();
                    Terminal.Invalidate();
                    return true;

                case TerminalKeyAction.Submit:
                    await SubmitLineAsync();
                    return true;
                default:
                    return false;
            }
        }

        private void ScrollPageUp()
        {
            double scrollAmount = TerminalScrollView.ViewportHeight * PageScrollFactor;

            double targetOffset = Math.Max(
                0,
                TerminalScrollView.VerticalOffset - scrollAmount);

            TerminalScrollView.ChangeView(null, targetOffset, null, true);
        }

        private void ScrollPageDown()
        {
            double scrollAmount = TerminalScrollView.ViewportHeight * PageScrollFactor;

            double targetOffset = Math.Min(
                TerminalScrollView.ScrollableHeight,
                TerminalScrollView.VerticalOffset + scrollAmount);

            TerminalScrollView.ChangeView(null, targetOffset, null, true);
        }

        private void DispatcherTimer_Tick(object? sender, object e)
        {
            if (_terminalViewModel.IsCommandRunning)
            {
                return;
            }

            _terminalRenderer.CaretVisible = _terminalRenderer.CaretVisible;

            Terminal.Invalidate();
        }

        private void TerminalBuffer_Changed(object? sender, EventArgs e)
        {
            RequestRefresh(true);
        }

        private void TerminalScrollView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RefreshTerminal(true);
        }

        private void Terminal_RegionsInvalidated(CanvasVirtualControl sender, CanvasRegionsInvalidatedEventArgs eventArgs)
        {
            _terminalRenderer.Draw(sender, eventArgs);
        }

        private void TerminalUserControl_CharacterReceived(UIElement sender, CharacterReceivedRoutedEventArgs eventArgs)
        {
            if (_terminalViewModel.IsCommandRunning || TerminalKeyHandler.IsKeyDown(VirtualKey.Control))
            {
                return;
            }

            int intChar = (int)eventArgs.Character;

            if (intChar < 32 || intChar == 127)
            {
                return;
            }

            string text = char.ConvertFromUtf32(intChar);

            _terminalViewModel.InsertText(text);

            Terminal.Invalidate();

            eventArgs.Handled = true;
        }

        private void TerminalUserControl_Loaded(object sender, RoutedEventArgs eventArgs)
        {
            Focus(FocusState.Programmatic);

            _terminalRenderer.CaretVisible = true;

            _dispatcherTimer.Start();

            RefreshTerminal(true);
        }

        private void TerminalUserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            _dispatcherTimer.Stop();
        }

        private void TerminalUserControl_PointerPressed(object sender, PointerRoutedEventArgs eventArgs)
        {
            Focus(FocusState.Pointer);

            _terminalRenderer.CaretVisible = true;

            _dispatcherTimer.Start();

            Terminal.Invalidate();

            eventArgs.Handled = true;
        }

        private void TerminalUserControl_LostFocus(object sender, RoutedEventArgs e)
        {
            _dispatcherTimer.Stop();

            _terminalRenderer.CaretVisible = false;

            Terminal.Invalidate();
        }

        private async void TerminalUserControl_KeyDown(object sender, KeyRoutedEventArgs eventArgs)
        {
            TerminalKeyAction terminalKeyAction = TerminalKeyHandler.HandleKey(eventArgs.OriginalKey, eventArgs.Key, _terminalViewModel.IsCommandRunning);

            eventArgs.Handled = await HandleKeyActionAsync(terminalKeyAction);
        }

        private void TerminalUserControl_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            if (TerminalKeyHandler.IsKeyDown(VirtualKey.Control))
            {
                return;
            }

            PointerPoint pointerPoint = e.GetCurrentPoint(Terminal);
            PointerPointProperties pointerPointProperties = pointerPoint.Properties;

            int mouseWheelDelta = pointerPointProperties.MouseWheelDelta;

            if (mouseWheelDelta > 0)
            {
                _terminalRenderer.ZoomIn();
            }
            else if (mouseWheelDelta < 0)
            {
                _terminalRenderer.ZoomOut();
            }

            RefreshTerminal(true);

            e.Handled = true;
        }

        private void TerminalScrollView_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (_updatingScrollView)
            {
                return;
            }

            const double tolerance = 2;

            double distanceFromBottom = TerminalScrollView.ScrollableHeight - TerminalScrollView.VerticalOffset;

            _followingOutput = distanceFromBottom <= tolerance;
        }

        private void Terminal_PointerPressed(object sender, PointerRoutedEventArgs eventArgs)
        {
            PointerPoint pointerPoint = eventArgs.GetCurrentPoint(Terminal);

            double mouseX = pointerPoint.Position.X;
            int caretPosition = _terminalRenderer.GetCaretPositionFromX(Terminal, (int)mouseX);

            _isPointerSelecting = pointerPoint.Properties.IsLeftButtonPressed;

            if (!_isPointerSelecting)
            {
                return;
            }

            _terminalViewModel.StartPointerSelection(caretPosition);
            Terminal.Invalidate();
        }

        private void Terminal_PointerMoved(object sender, PointerRoutedEventArgs eventArgs)
        {
            if (!_isPointerSelecting)
            {
                return;
            }

            double mouseX = eventArgs.GetCurrentPoint(Terminal).Position.X;
            int caretPosition = _terminalRenderer.GetCaretPositionFromX(Terminal, (float)mouseX);

            _terminalViewModel.UpdatePointerSelection(caretPosition);
            Terminal.Invalidate();
        }

        private void Terminal_PointerReleased(object sender, PointerRoutedEventArgs eventArgs)
        {
            if (!_isPointerSelecting)
            {
                return;
            }

            _terminalViewModel.ReleasePointerSelection();
            _isPointerSelecting = false;
            Terminal.Invalidate();
        }
    }
}