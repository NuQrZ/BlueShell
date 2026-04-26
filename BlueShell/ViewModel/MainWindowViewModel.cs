using BlueShell.Model;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;

namespace BlueShell.ViewModel
{
    public sealed partial class MainWindowViewModel : ObservableObject
    {
        private int _tabIndex = 1;
        public ObservableCollection<TabViewModel> Tabs { get; set; } = [];

        [ObservableProperty]
        public partial TabViewModel? SelectedTab { get; set; }

        [ObservableProperty]
        public partial bool IsSettingsOpen { get; set; }

        public MainWindowViewModel()
        {
            AddTab();
            SelectedTab = Tabs[0];
        }

        [RelayCommand]
        private void AddTab()
        {
            TabModel tabModel = new()
            {
                TabHeader = $"Terminal {_tabIndex++}",
            };

            TabViewModel tabViewModel = new(tabModel, "Terminal");
            Tabs.Add(tabViewModel);
            SelectedTab = tabViewModel;
        }

        public void RemoveTab(TabViewModel? tab)
        {
            if (tab is null)
            {
                return;
            }

            if (Tabs.Count == 1)
            {
                return;
            }

            bool wasSelected = SelectedTab == tab;

            Tabs.Remove(tab);

            if (wasSelected)
            {
                SelectedTab = Tabs.LastOrDefault();
            }
        }

        [RelayCommand]
        private void ToggleSettingsPage()
        {
            IsSettingsOpen = !IsSettingsOpen;
        }
    }
}
