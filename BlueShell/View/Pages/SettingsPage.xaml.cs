using BlueShell.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

namespace BlueShell.View.Pages
{
    public sealed partial class SettingsPage : Page
    {
        private readonly SettingsViewModel? _settingsViewModel;

        public SettingsPage()
        {
            InitializeComponent();

            _settingsViewModel = App.ServiceProvider!.GetRequiredService<SettingsViewModel>();
        }
    }
}
