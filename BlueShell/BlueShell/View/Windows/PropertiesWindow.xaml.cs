using BlueShell.Model.Properties;
using BlueShell.ViewModel;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace BlueShell.View.Windows
{
    public sealed partial class PropertiesWindow : Window
    {
        private readonly PropertiesWindowViewModel _propertiesWindowViewModel;
        public PropertiesWindow(PropertiesWindowViewModel propertiesWindowViewModel)
        {
            _propertiesWindowViewModel = propertiesWindowViewModel;

            InitializeComponent();

            ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar);

            SystemBackdrop = new MicaBackdrop()
            {
                Kind = MicaKind.BaseAlt
            };
        }

        private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem is PropertyItem propertyItem)
            {
                _propertiesWindowViewModel.SelectedItem = propertyItem;
            }
        }
    }
}
