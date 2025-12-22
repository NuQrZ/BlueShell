using BlueShell.Model;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using Microsoft.UI.Xaml;

namespace BlueShell.View.Pages
{
    public sealed partial class MainPage : Page
    {
        private TabModel? _tabModel;
        private Dictionary<string, NavigationViewItem>? _navItemsByTag;
        public MainPage()
        {
            InitializeComponent();
            NavigationViewControl.IsPaneOpen = false;
            NavigationViewControl.SelectedItem = TerminalItem;

            BuildNavItemMap();
            Loaded += MainPage_Loaded;
        }

        private void BuildNavItemMap()
        {
            _navItemsByTag = new Dictionary<string, NavigationViewItem>
            {
                ["Terminal"] = TerminalItem,
                ["Help"] = HelpItem,
                ["SystemProcesses"] = SystemProcessesItem,
                ["WebPage"] = WebPageItem,
                ["SystemInfo"] = SystemInfoItem,
                ["GraphicsCard"] = GraphicsCardInfoItem,
                ["Motherboard"] = MotherboardInfoItem,
                ["NetworkInterface"] = NetworkInfoItem,
                ["OperatingSystem"] = OperatingSystemInfoItem,
                ["Processor"] = ProcessorInfoItem
            };
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            _tabModel = DataContext as TabModel;
            string? itemTag = _tabModel?.SelectedNavTag;

            if (itemTag == null || _navItemsByTag is null)
            {
                return;
            }

            if (!_navItemsByTag.TryGetValue(itemTag, out var item))
            {
                return;
            }

            NavigationViewControl.SelectedItem = item;

            if (item == SystemInfoItem || item == GraphicsCardInfoItem || item == MotherboardInfoItem || item == NetworkInfoItem || item == OperatingSystemInfoItem || item == ProcessorInfoItem)
            {
                SystemInfoItem.IsExpanded = true;
            }
        }

        private void NavigationViewControl_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (sender.SelectedItem is not NavigationViewItem selectedItem)
            {
                return;
            }

            string? itemTag = selectedItem.Tag.ToString();

            _tabModel?.SelectedNavTag = itemTag;
            _tabModel?.ApplyNavSelection(itemTag);

            if (selectedItem.Tag.ToString() == "Terminal")
            {
                //MainFrame.Navigate(typeof(MainPage));
            }
        }
    }
}
