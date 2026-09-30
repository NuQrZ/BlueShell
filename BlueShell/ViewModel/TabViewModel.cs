using BlueShell.Model;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BlueShell.ViewModel
{
    public sealed partial class TabViewModel(TabModel tab, string navTag) : ObservableObject
    {
        public TabModel Tab { get; } = tab;

        [ObservableProperty]
        public partial string SelectedNavItemTag { get; set; } = navTag;

        public int TabIndex
        {
            get => Tab.TabIndex;
            set
            {
                Tab.TabIndex = value;
                OnPropertyChanged();
            }
        }

        public string TabHeader
        {
            get => Tab.TabHeader;
            set
            {
                Tab.TabHeader = value;
                OnPropertyChanged();
            }
        }

        public string IconPath
        {
            get => Tab.IconPath;
            set
            {
                Tab.IconPath = value;
                OnPropertyChanged();
            }
        }

        public void NavigationItemSelected(int tabIndex, string navTag, string iconPath)
        {
            TabIndex = tabIndex;
            TabHeader = navTag;
            SelectedNavItemTag = navTag;
            IconPath = iconPath;
        }
    }
}
