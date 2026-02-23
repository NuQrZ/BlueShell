using BlueShell.Model;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;

namespace BlueShell.View.Controls
{
    public sealed class TerminalLineView : UserControl
    {
        private readonly TextBlock _textBlock;

        public TerminalLineView()
        {
            _textBlock = new TextBlock
            {
                FontFamily = new FontFamily("Cascadia Code"),
                FontSize = 18,
                TextWrapping = TextWrapping.NoWrap,
                IsTextSelectionEnabled = false
            };

            Content = _textBlock;

            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            _textBlock.Inlines.Clear();

            if (args.NewValue is not OutputLine line)
                return;

            foreach (OutputSegment seg in line.Segments)
            {
                var run = new Run
                {
                    Text = seg.Text ?? string.Empty,
                    FontStyle = seg.FontStyle,
                    FontWeight = seg.FontWeight
                };

                if (seg.Color is not null)
                    run.Foreground = seg.Color;

                _textBlock.Inlines.Add(run);
            }
        }
    }
}