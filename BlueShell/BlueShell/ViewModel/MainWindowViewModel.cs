using BlueShell.Model;
using BlueShell.View.Pages;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Collections.ObjectModel;

namespace BlueShell.ViewModel
{
    public sealed partial class MainWindowViewModel : ObservableObject
    {
        private int _tabIndex = 1;
        public ObservableCollection<TabModel> Tabs { get; } = [];

        [ObservableProperty]
        public partial TabModel SelectedTab { get; set; }

        public MainWindowViewModel()
        {
            Tabs.Add(CreateTab());
            SelectedTab = Tabs[0];
        }

        private TabModel CreateTab()
        {
            Uri imagePathUri = new("ms-appx:///Assets/Terminal.ico");
            Frame tabFrame = new();

            TabModel newTab = new()
            {
                TabHeader = $"Terminal {_tabIndex}",
                TabIcon = new BitmapIconSource()
                {
                    UriSource = imagePathUri,
                    ShowAsMonochrome = false
                },
                TabNumber = _tabIndex
            };
            tabFrame.Navigate(typeof(MainPage), newTab, new SlideNavigationTransitionInfo()
            {
                Effect = SlideNavigationTransitionEffect.FromLeft
            });

            newTab.TabFrame = tabFrame;

            _tabIndex++;
            return newTab;
        }

        [RelayCommand]
        private void AddTab()
        {
            TabModel newTab = CreateTab();
            Tabs.Add(newTab);
            SelectedTab = newTab;
        }

        [RelayCommand]
        private void RemoveTab(TabModel tabModel)
        {
            if (Tabs.Count <= 1)
            {
                return;
            }

            Tabs.Remove(tabModel);
            bool wasSelected = tabModel == SelectedTab;
            if (wasSelected)
            {
                SelectedTab = Tabs[^1];
            }
        }

        [RelayCommand]
        private void ToggleSettingsPage(bool isChecked)
        {
            SelectedTab.TabFrame?.Navigate(isChecked ? typeof(SettingsPage) : typeof(MainPage), SelectedTab, new SlideNavigationTransitionInfo()
            {
                Effect = SlideNavigationTransitionEffect.FromLeft
            });
        }
    }
}
