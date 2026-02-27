using BlueShell.Model.Properties;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace BlueShell.ViewModel
{
    public sealed partial class PropertiesWindowViewModel : ObservableObject
    {
        public ObservableCollection<PropertyItem> PropertyItems { get; } = [];

        [ObservableProperty]
        public partial PropertyItem? SelectedItem { get; set; }

        public PropertiesWindowViewModel()
        {
            SelectedItem = null;
        }

        public void AddItem(PropertyItem propertyItem)
        {
            PropertyItems.Add(propertyItem);
        }

        public void ClearItems()
        {
            PropertyItems.Clear();
        }
    }
}
