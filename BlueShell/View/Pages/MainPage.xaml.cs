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
    private Dictionary<string, string> itemTagsFilePaths = [];
    private Dictionary<string, NavigationViewItem> navItemTags = [];
    public MainPage()
    {
        InitializeComponent();

        NavigationViewControl.IsPaneOpen = false;

        itemTagsFilePaths["Terminal"] = "ms-appx:///Assets/Icons/Terminal.ico";
        itemTagsFilePaths["Help"] = "ms-appx:///Assets/Icons/Help.ico";
        itemTagsFilePaths["SystemProcesses"] = "ms-appx:///Assets/Icons/Processes.ico";
        itemTagsFilePaths["WebSearch"] = "ms-appx:///Assets/Icons/Web.ico";
        itemTagsFilePaths["SystemInfo"] = "ms-appx:///Assets/Icons/System Info.ico";
        itemTagsFilePaths["GraphicsCard"] = "ms-appx:///Assets/Icons/Graphics Card.ico";
        itemTagsFilePaths["Motherboard"] = "ms-appx:///Assets/Icons/Motherboard.ico";
        itemTagsFilePaths["NetworkInterface"] = "ms-appx:///Assets/Icons/Wifi.ico";
        itemTagsFilePaths["OperatingSystem"] = "ms-appx:///Assets/Icons/Windows 11.ico";
        itemTagsFilePaths["Processor"] = "ms-appx:///Assets/Icons/CPU.ico";

        navItemTags["Terminal"] = TerminalItem;
        navItemTags["Help"] = HelpItem;
        navItemTags["SystemProcesses"] = SystemProcessesItem;
        navItemTags["WebSearch"] = WebSearchItem;
        navItemTags["SystemInfo"] = SystemInfoItem;
        navItemTags["GraphicsCard"] = GraphicsCardInfoItem;
        navItemTags["Motherboard"] = MotherboardInfoItem;
        navItemTags["NetworkInterface"] = NetworkInfoItem;
        navItemTags["OperatingSystem"] = OperatingSystemInfoItem;
        navItemTags["Processor"] = ProcessorInfoItem;
    }

    private void MainWindowViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainWindowViewModel.SelectedTab))
        {
            TabViewModel? currentTab = _mainWindowViewModel?.SelectedTab;
            NavigationViewItem navigationViewItem = navItemTags[currentTab!.SelectedNavItemTag];
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

        if (navItemTags.TryGetValue(navTag, out var item))
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
            _mainWindowViewModel.SelectedTab.IconPath = itemTagsFilePaths[itemTag];
            _mainWindowViewModel.SelectedTab.SelectedNavItemTag = itemTag;
        }
    }
}
