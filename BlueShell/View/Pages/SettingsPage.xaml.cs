using BlueShell.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Windows.UI;

namespace BlueShell.View.Pages
{
    public sealed partial class SettingsPage : Page
    {
        private readonly SettingsViewModel _settingsViewModel;
        public SettingsPage()
        {
            InitializeComponent();

            _settingsViewModel = App.ServiceProvider!.GetRequiredService<SettingsViewModel>();
        }

        private void FirstColorPicker_ColorChanged(ColorPicker sender, ColorChangedEventArgs args)
        {
            Color color = args.NewColor;
            string colorValue = $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";
            _settingsViewModel.Color1 = colorValue;
        }

        private void SecondColorPicker_ColorChanged(ColorPicker sender, ColorChangedEventArgs args)
        {
            Color color = args.NewColor;
            string colorValue = $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";
            _settingsViewModel.Color2 = colorValue;
        }

        private void ThirdColorPicker_ColorChanged(ColorPicker sender, ColorChangedEventArgs args)
        {
            Color color = args.NewColor;
            string colorValue = $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";
            _settingsViewModel.Color3 = colorValue;
        }

        private void FirstColorBox_GotFocus(object sender, RoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }

        private void SecondColorBox_GotFocus(object sender, RoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }

        private void ThirdColorBox_GotFocus(object sender, RoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }
    }
}
