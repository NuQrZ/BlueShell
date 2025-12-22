using BlueShell.Terminal.Abstractions;
using Microsoft.UI.Xaml.Controls;

namespace BlueShell.Terminal.WinUI
{
    public sealed class DataDisplay(ListView listView) : IDataDisplay
    {
        public void Clear()
        {
            listView.Items.Clear();
        }

        public void Add(object item)
        {
            listView.Items.Add(item);
        }

        public void SetHeader(object header)
        {
            listView.Header = header;
        }
    }
}
