using BlueShell.Terminal.Abstractions;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using System.Collections.ObjectModel;

namespace BlueShell.Terminal.WinUI
{
    public sealed class DataDisplay(
        ListView listView) : IDataDisplay
    {
        private readonly ObservableCollection<object> _flatItems = [];
        private readonly CollectionViewSource _groupedSource = new()
        {
            IsSourceGrouped = true,
            ItemsPath = new PropertyPath("Items")
        };

        private readonly ObservableCollection<object> _groupCollection = [];
        private bool _isGrouped;

        public void Add(object item)
        {
            if (_isGrouped)
            {
                return;
            }

            _flatItems.Add(item);
        }

        public void SetHeader(object header)
        {
            listView.Header = header;
        }

        public void BeginGrouped()
        {
            Clear();
            _isGrouped = true;
            _groupedSource.Source = _groupCollection;
            listView.ItemsSource = _groupedSource.View;
        }

        public void AddGroup(object group)
        {
            _groupCollection.Add(group);
        }

        public void Clear()
        {
            _isGrouped = false;

            _flatItems.Clear();
            _groupCollection.Clear();
            _groupedSource.Source = null;

            listView.Header = null;
            listView.ItemsSource = _flatItems;
        }
    }
}
