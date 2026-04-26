using BlueShell.Model;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BlueShell.ViewModel
{
    public sealed partial class TabViewModel(TabModel tab, string navTag) : ObservableObject
    {
        public TabModel Tab { get; } = tab;

        [ObservableProperty]
        public partial string SelectedNavItemTag { get; set; } = navTag;

        public string TabHeader
        {
            get => Tab.TabHeader;
            set
            {
                if (Tab.TabHeader != value)
                {
                    Tab.TabHeader = value;
                    OnPropertyChanged();
                }
            }
        }

        public string IconPath
        {
            get => Tab.IconPath;
            set
            {
                if (Tab.IconPath != value)
                {
                    Tab.IconPath = value;
                    OnPropertyChanged();
                }
            }
        }
    }
}
