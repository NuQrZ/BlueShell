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
        _itemTagsFilePaths["System Processes"] = "ms-appx:///Assets/Icons/Processes.ico";
        _itemTagsFilePaths["Web Search"] = "ms-appx:///Assets/Icons/Web.ico";
        _itemTagsFilePaths["System Info"] = "ms-appx:///Assets/Icons/System Info.ico";
        _itemTagsFilePaths["Graphics Card"] = "ms-appx:///Assets/Icons/Graphics Card.ico";
        _itemTagsFilePaths["Motherboard"] = "ms-appx:///Assets/Icons/Motherboard.ico";
        _itemTagsFilePaths["Network Interface"] = "ms-appx:///Assets/Icons/Wifi.ico";
        _itemTagsFilePaths["Operating System"] = "ms-appx:///Assets/Icons/Windows 11.ico";
        _itemTagsFilePaths["Processor"] = "ms-appx:///Assets/Icons/CPU.ico";

        _navItemTagsNavViewItems["Terminal"] = TerminalItem;
        _navItemTagsNavViewItems["Help"] = HelpItem;
        _navItemTagsNavViewItems["System Processes"] = SystemProcessesItem;
        _navItemTagsNavViewItems["Web Search"] = WebSearchItem;
        _navItemTagsNavViewItems["System Info"] = SystemInfoItem;
        _navItemTagsNavViewItems["Graphics Card"] = GraphicsCardInfoItem;
        _navItemTagsNavViewItems["Motherboard"] = MotherboardInfoItem;
        _navItemTagsNavViewItems["Network Interface"] = NetworkInfoItem;
        _navItemTagsNavViewItems["Operating System"] = OperatingSystemInfoItem;
        _navItemTagsNavViewItems["Processor"] = ProcessorInfoItem;

        _navItemTagsPageTypes["Terminal"] = typeof(TerminalPage);
        _navItemTagsPageTypes["Help"] = typeof(HelpPage);
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is not MainWindowViewModel mainWindowViewModel)
        {
            return;
        }

        if (_mainWindowViewModel is not null)
        {
            _mainWindowViewModel?.PropertyChanged -= MainWindowViewModel_PropertyChanged;
        }

        _mainWindowViewModel = mainWindowViewModel;
        _mainWindowViewModel?.PropertyChanged += MainWindowViewModel_PropertyChanged;

        UpdateNavigationSelection();
    }

    private void MainWindowViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(MainWindowViewModel.SelectedTab))
        {
            return;
        }

        TabViewModel? currentTab = _mainWindowViewModel?.SelectedTab;
        NavigationViewItem navigationViewItem = _navItemTagsNavViewItems[currentTab!.SelectedNavItemTag];
        NavigationViewControl.SelectedItem = navigationViewItem;
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

        if (itemTag is null)
        {
            return;
        }

        int tabIndex = _mainWindowViewModel.SelectedTab.TabIndex;

        _mainWindowViewModel.SelectedTab.NavigationItemSelected(tabIndex, itemTag, _itemTagsFilePaths[itemTag]);

        _navItemTagsPageTypes.TryGetValue(itemTag, out Type? pageType);

        if (pageType == null)
        {
            return;
        }

        MainFrame.Navigate(pageType, _mainWindowViewModel.SelectedTab);
    }
}
