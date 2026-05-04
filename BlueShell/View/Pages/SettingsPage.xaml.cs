using BlueShell.Helpers;
using BlueShell.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System.ComponentModel;
using Windows.UI;
using Windows.UI.ViewManagement;

namespace BlueShell.View.Pages
{
    public sealed partial class SettingsPage : Page
    {
        private readonly SettingsViewModel? _settingsViewModel;

        public SettingsPage()
        {
            InitializeComponent();

            _settingsViewModel = App.ServiceProvider!.GetRequiredService<SettingsViewModel>();

            _settingsViewModel.PropertyChanged += SettingsViewModel_PropertyChanged;
        }

        private void SettingsViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (App.MainWindow!.Content is FrameworkElement frameworkElement)
            {
                SetBackground(frameworkElement, _settingsViewModel!.SelectedTheme, _settingsViewModel.SelectedBackdrop);
            }
        }

        private static void SetBackground(FrameworkElement frameworkElement, string theme, string backdrop)
        {
            if (backdrop != "None")
            {
                return;
            }

            if (frameworkElement is Grid rootGrid)
            {
                Brush brush;
                if (theme == "Light")
                {
                    brush = new SolidColorBrush(Colors.White);
                }
                else
                {
                    brush = new SolidColorBrush(Colors.Black);
                }

                rootGrid.Background = brush;
            }
        }

        private static void RemoveBackground(FrameworkElement frameworkElement)
        {
            if (frameworkElement is Grid rootGrid)
            {
                rootGrid.Background = new SolidColorBrush(Colors.Transparent);
            }
        }

        private void MicaAltBackdrop_Checked(object sender, RoutedEventArgs e)
        {
            if (App.MainWindow!.SystemBackdrop is MicaBackdrop micaBackdrop && micaBackdrop.Kind == MicaKind.BaseAlt)
            {
                return;
            }

            if (App.MainWindow.Content is FrameworkElement frameworkElement)
            {
                RemoveBackground(frameworkElement);
            }

            App.MainWindow!.SystemBackdrop = new MicaBackdrop()
            {
                Kind = MicaKind.BaseAlt
            };
        }

        private void MicaBackdrop_Checked(object sender, RoutedEventArgs e)
        {
            if (App.MainWindow!.SystemBackdrop is MicaBackdrop micaBackdrop && micaBackdrop.Kind == MicaKind.Base)
            {
                return;
            }

            if (App.MainWindow.Content is FrameworkElement frameworkElement)
            {
                RemoveBackground(frameworkElement);
            }

            App.MainWindow!.SystemBackdrop = new MicaBackdrop()
            {
                Kind = MicaKind.Base
            };
        }

        private void AcrylicDefaultBackdrop_Checked(object sender, RoutedEventArgs e)
        {
            if (App.MainWindow!.SystemBackdrop is DesktopAcrylicBackdrop)
            {
                return;
            }

            if (App.MainWindow.Content is FrameworkElement frameworkElement)
            {
                RemoveBackground(frameworkElement);
            }

            App.MainWindow!.SystemBackdrop = new DesktopAcrylicBackdrop();
        }

        private void AcrylicThinBackdrop_Checked(object sender, RoutedEventArgs e)
        {
            if (App.MainWindow!.SystemBackdrop is ThinAcrylicBackdrop)
            {
                return;
            }

            if (App.MainWindow.Content is FrameworkElement frameworkElement)
            {
                RemoveBackground(frameworkElement);
            }

            App.MainWindow!.SystemBackdrop = new ThinAcrylicBackdrop();
        }

        private void NullBackdrop_Checked(object sender, RoutedEventArgs e)
        {
            App.MainWindow!.SystemBackdrop = null;
        }

        private void DefaultColor_Checked(object sender, RoutedEventArgs e)
        {
            if (App.MainWindow!.Content is FrameworkElement frameworkElement)
            {
                frameworkElement.RequestedTheme = ElementTheme.Default;

                UISettings uiSettings = new();
                Color color = uiSettings.GetColorValue(UIColorType.Background);
                string theme = color == Colors.White ? "Light" : "Dark";
                SetBackground(frameworkElement, theme, _settingsViewModel!.SelectedBackdrop);
            }
        }

        private void DarkColor_Checked(object sender, RoutedEventArgs e)
        {
            if (App.MainWindow!.Content is FrameworkElement frameworkElement)
            {
                frameworkElement.RequestedTheme = ElementTheme.Dark;
            }
        }

        private void LightColor_Checked(object sender, RoutedEventArgs e)
        {
            if (App.MainWindow!.Content is FrameworkElement frameworkElement)
            {
                frameworkElement.RequestedTheme = ElementTheme.Light;
            }
        }
    }
}
