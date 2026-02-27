using BlueShell.Helpers;
using BlueShell.Model;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace BlueShell.View.Pages
{
    public sealed partial class MainPage : Page
    {
        private TabModel? _tabModel;
        private Dictionary<string, NavigationViewItem>? _navItemsByTag;
        public MainPage()
        {
            InitializeComponent();

            BuildNavItemMap();

            NavigationViewControl.IsPaneOpen = false;

            Loaded += async (_, _) =>
            {
                Stopwatch stopwatch = Stopwatch.StartNew();

                await Task.Run(() =>
                {
                    foreach (DriveInfo driveInfo in DriveInfo.GetDrives())
                    {
                        WmiUtilities.GetAllDriveProperties(driveInfo.RootDirectory.FullName);
                    }
                });

                stopwatch.Stop();

                Debug.WriteLine($"WMI preload total time: {stopwatch.ElapsedMilliseconds} ms");
            };
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

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            _tabModel = e.Parameter as TabModel;

            _tabModel!.SelectedNavTag ??= "Terminal";
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

            string? tag = selectedItem.Tag.ToString();

            switch (tag)
            {
                case "Terminal":
                    ToggleTerminalLayout.Visibility = Visibility.Visible;
                    MainFrame.Navigate(typeof(TerminalPage), _tabModel);
                    break;
                default:
                    ToggleTerminalLayout.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        private void ToggleTerminalLayout_OnClick(object sender, RoutedEventArgs e)
        {
            if (MainFrame.Content is TerminalPage terminalPage)
            {
                terminalPage.ToggleLayout();
            }
        }
    }
}
