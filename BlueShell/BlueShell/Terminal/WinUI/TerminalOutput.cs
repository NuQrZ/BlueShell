using BlueShell.Model;
using BlueShell.Terminal.Abstractions;
using Microsoft.UI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Text;

namespace BlueShell.Terminal.WinUI
{
    public sealed class TerminalOutput : ITerminalOutput
    {
        private readonly ObservableCollection<OutputLine> _lines;
        private readonly ScrollViewer _scrollViewer;

        private readonly DispatcherQueue _dispatcherQueue;

        private readonly Func<ElementTheme> _themeProvider;

        private bool _scrollScheduled;
        private bool _isAutoScrolling;
        private bool _stickToBottom = true;

        private readonly SolidColorBrush?[] _messageKindBrushCache;
        private readonly Dictionary<uint, SolidColorBrush> _extraBrushCache;
        private ElementTheme? _cachedTheme;

        public TerminalOutput(
            ItemsRepeater terminalOutput,
            ScrollViewer scrollViewer,
            Func<ElementTheme> themeProvider)
        {
            _lines = [];

            _scrollViewer = scrollViewer;
            _themeProvider = themeProvider;

            _messageKindBrushCache = new SolidColorBrush?[64];
            _extraBrushCache = [];

            _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

            terminalOutput.ItemsSource = _lines;

            _scrollViewer.ViewChanged += ScrollViewer_ViewChanged;
        }

        private void ScrollViewer_ViewChanged(object? sender, ScrollViewerViewChangedEventArgs e)
        {
            if (sender is not ScrollViewer scrollViewer)
            {
                return;
            }

            if (_isAutoScrolling)
            {
                _stickToBottom = true;
                if (!e.IsIntermediate)
                {
                    _isAutoScrolling = false;

                    _dispatcherQueue.TryEnqueue(() =>
                    {
                        double maxOffset = Math.Max(0.0, _scrollViewer.ExtentHeight - _scrollViewer.ViewportHeight);
                        _scrollViewer.ChangeView(null, maxOffset, null, true);
                    });
                }

                return;
            }

            double maxOffset = Math.Max(0.0, scrollViewer.ExtentHeight - scrollViewer.ViewportHeight);
            double distance = maxOffset - scrollViewer.VerticalOffset;

            _stickToBottom = distance <= 40.0;
        }

        private void ScheduleAutoScroll()
        {
            if (_scrollScheduled || !_stickToBottom)
            {
                return;
            }

            _scrollScheduled = true;

            _dispatcherQueue.TryEnqueue(async void () =>
            {
                try
                {
                    await Task.Yield();

                    await Task.Yield();

                    double maxOffset = Math.Max(0.0, _scrollViewer.ExtentHeight - _scrollViewer.ViewportHeight);

                    _isAutoScrolling = true;
                    _scrollViewer.ChangeView(null, maxOffset, null, true);
                }
                catch (Exception)
                {
                    //
                }
                finally
                {
                    _scrollScheduled = false;
                }
            });
        }

        public LineBuilder Line()
        {
            return new LineBuilder(this);
        }

        internal void AddLine(OutputLine outputLine)
        {
            _lines.Add(outputLine);

            ScheduleAutoScroll();
        }

        public void Write(string text, TerminalMessageKind messageKind = TerminalMessageKind.Output,
            FontStyle fontStyle = FontStyle.Normal, FontWeight? fontWeight = null)
        {
            OutputSegment outputSegment = new()
            {
                Text = text,
                Color = new SolidColorBrush(GetColor(messageKind)),
                FontStyle = fontStyle,
                FontWeight = fontWeight ?? FontWeights.Normal
            };

            OutputLine outputLine = new();
            outputLine.AddSegment(outputSegment);

            AddLine(outputLine);
        }

        public void WriteLine(string text = "", TerminalMessageKind messageKind = TerminalMessageKind.Output,
            FontStyle fontStyle = FontStyle.Normal, FontWeight? fontWeight = null)
        {
            OutputSegment outputSegment = new()
            {
                Text = "\n" + text,
                Color = new SolidColorBrush(GetColor(messageKind)),
                FontStyle = fontStyle,
                FontWeight = fontWeight ?? FontWeights.Normal
            };

            OutputLine outputLine = new();
            outputLine.AddSegment(outputSegment);

            AddLine(outputLine);
        }

        public void Clear()
        {
            _lines.Clear();
        }

        private Color GetColor(TerminalMessageKind kind)
        {
            ElementTheme theme = _themeProvider();

            if (theme == ElementTheme.Default)
            {
                theme = Application.Current.RequestedTheme == ApplicationTheme.Dark
                    ? ElementTheme.Dark
                    : ElementTheme.Light;
            }

            return theme switch
            {
                ElementTheme.Dark => kind switch
                {
                    TerminalMessageKind.PrintOutput => Color.FromArgb(255, 110, 190, 220),
                    TerminalMessageKind.Error => Color.FromArgb(255, 220, 80, 80),
                    TerminalMessageKind.Warning => Color.FromArgb(255, 210, 160, 90),
                    TerminalMessageKind.Success => Color.FromArgb(255, 80, 180, 130),
                    TerminalMessageKind.Info => Color.FromArgb(255, 160, 170, 185),
                    _ => Colors.White
                },

                ElementTheme.Light => kind switch
                {
                    TerminalMessageKind.PrintOutput => Color.FromArgb(255, 60, 120, 155),
                    TerminalMessageKind.Error => Color.FromArgb(255, 190, 55, 55),
                    TerminalMessageKind.Warning => Color.FromArgb(255, 170, 115, 40),
                    TerminalMessageKind.Success => Color.FromArgb(255, 45, 135, 90),
                    TerminalMessageKind.Info => Color.FromArgb(255, 90, 100, 115),
                    _ => Colors.Black
                },

                _ => Colors.Gray
            };
        }

        internal SolidColorBrush GetBrush(TerminalMessageKind messageKind)
        {
            ElementTheme theme = _themeProvider();

            if (_cachedTheme != theme)
            {
                _cachedTheme = theme;

                _extraBrushCache.Clear();
                Array.Clear(_messageKindBrushCache, 0, _messageKindBrushCache.Length);
            }

            int index = (int)messageKind;

            SolidColorBrush? cachedColorBrush = _messageKindBrushCache[index];
            if (cachedColorBrush != null)
            {
                return cachedColorBrush;
            }

            SolidColorBrush created = new(GetColor(messageKind));
            _messageKindBrushCache[index] = created;

            return created;
        }

        internal SolidColorBrush GetBrush(Color extraColor)
        {
            uint key = ((uint)extraColor.A << 24) | ((uint)extraColor.R << 16) | ((uint)extraColor.G << 8) | extraColor.B;

            if (_extraBrushCache.TryGetValue(key, out SolidColorBrush? brush))
            {
                return brush;
            }

            brush = new SolidColorBrush(extraColor);
            _extraBrushCache[key] = brush;
            return brush;
        }
    }
}
