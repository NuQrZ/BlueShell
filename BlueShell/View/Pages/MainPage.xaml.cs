using BlueShell.ViewModel;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace BlueShell.View.Pages;

public sealed partial class MainPage : Page
{
    private MainWindowViewModel? _mainWindowViewModel;
    private readonly Dictionary<string, string> _itemTagsFilePaths = [];
    private readonly Dictionary<string, NavigationViewItem> _navItemTagsNavViewItems = [];
    private readonly Dictionary<string, Type> _navItemTagsPageTypes = [];

    public MainPage()
    {
        InitializeComponent();

        NavigationViewControl.IsPaneOpen = false;

        _itemTagsFilePaths["Terminal"] = "ms-appx:///Assets/Icons/Terminal.ico";
        _itemTagsFilePaths["Help"] = "ms-appx:///Assets/Icons/Help.ico";
        _itemTagsFilePaths["SystemProcesses"] = "ms-appx:///Assets/Icons/Processes.ico";
        _itemTagsFilePaths["WebSearch"] = "ms-appx:///Assets/Icons/Web.ico";
        _itemTagsFilePaths["SystemInfo"] = "ms-appx:///Assets/Icons/System Info.ico";
        _itemTagsFilePaths["GraphicsCard"] = "ms-appx:///Assets/Icons/Graphics Card.ico";
        _itemTagsFilePaths["Motherboard"] = "ms-appx:///Assets/Icons/Motherboard.ico";
        _itemTagsFilePaths["NetworkInterface"] = "ms-appx:///Assets/Icons/Wifi.ico";
        _itemTagsFilePaths["OperatingSystem"] = "ms-appx:///Assets/Icons/Windows 11.ico";
        _itemTagsFilePaths["Processor"] = "ms-appx:///Assets/Icons/CPU.ico";

        _navItemTagsNavViewItems["Terminal"] = TerminalItem;
        _navItemTagsNavViewItems["Help"] = HelpItem;
        _navItemTagsNavViewItems["SystemProcesses"] = SystemProcessesItem;
        _navItemTagsNavViewItems["WebSearch"] = WebSearchItem;
        _navItemTagsNavViewItems["SystemInfo"] = SystemInfoItem;
        _navItemTagsNavViewItems["GraphicsCard"] = GraphicsCardInfoItem;
        _navItemTagsNavViewItems["Motherboard"] = MotherboardInfoItem;
        _navItemTagsNavViewItems["NetworkInterface"] = NetworkInfoItem;
        _navItemTagsNavViewItems["OperatingSystem"] = OperatingSystemInfoItem;
        _navItemTagsNavViewItems["Processor"] = ProcessorInfoItem;

        _navItemTagsPageTypes["Terminal"] = typeof(TerminalPage);
        _navItemTagsPageTypes["Help"] = typeof(HelpPage);
    }

    private void MainWindowViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainWindowViewModel.SelectedTab))
        {
            TabViewModel? currentTab = _mainWindowViewModel?.SelectedTab;
            NavigationViewItem navigationViewItem = _navItemTagsNavViewItems[currentTab!.SelectedNavItemTag];
            NavigationViewControl.SelectedItem = navigationViewItem;
        }
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is MainWindowViewModel mainWindowViewModel)
        {
            if (_mainWindowViewModel is not null)
            {
                _mainWindowViewModel?.PropertyChanged -= MainWindowViewModel_PropertyChanged;
            }

            _mainWindowViewModel = mainWindowViewModel;
            _mainWindowViewModel?.PropertyChanged += MainWindowViewModel_PropertyChanged;

            UpdateNavigationSelection();
        }
    }

    private void UpdateNavigationSelection()
    {
        if (_mainWindowViewModel?.SelectedTab is null)
        {
            return;
        }

        string navTag = _mainWindowViewModel.SelectedTab.SelectedNavItemTag;

        if (_navItemTagsNavViewItems.TryGetValue(navTag, out var item))
        {
            NavigationViewControl.SelectedItem = item;
        }
    }

    private void NavigationViewControl_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (_mainWindowViewModel is null)
        {
            return;
        }

        if (_mainWindowViewModel.SelectedTab is null)
        {
            return;
        }

        if (args.SelectedItem is not NavigationViewItem navigationViewItem)
        {
            return;
        }

        string? itemTag = navigationViewItem.Tag.ToString();

        if (itemTag is not null)
        {
            string tabHeader = _mainWindowViewModel.SelectedTab.TabHeader;
            string[] headerParts = tabHeader.Split(' ');

            int tabIndex = Convert.ToInt32(headerParts[1]);

            _mainWindowViewModel.SelectedTab.TabHeader = $"{itemTag} {tabIndex}";
            _mainWindowViewModel.SelectedTab.IconPath = _itemTagsFilePaths[itemTag];
            _mainWindowViewModel.SelectedTab.SelectedNavItemTag = itemTag;

            _navItemTagsPageTypes.TryGetValue(itemTag, out Type? pageType);

            MainFrame.Navigate(pageType);
        }
    }
}
