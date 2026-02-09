using System;
using BlueShell.Terminal.Abstractions;
using Microsoft.UI.Xaml.Controls;

namespace BlueShell.Terminal.WinUI
{
    public sealed class DataDisplay(
        ListView listView,
        Action<object> onAdded) : IDataDisplay
    {
        public void Clear()
        {
            listView.Items.Clear();
        }

        public void Add(object item)
        {
            listView.Items.Add(item);
            onAdded(item);
        }

        public void SetHeader(object header)
        {
            listView.Header = header;
        }
    }
}