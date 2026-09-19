using BlueShell.Helpers;
using BlueShell.Model;
using Microsoft.UI;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using Windows.UI;
using Windows.UI.ViewManagement;

namespace BlueShell.Services
{
    public sealed class WindowAppearanceService : IWindowAppearanceService
    {
        public void ApplyBackdrop(AppBackdrop backdropType)
        {
            SystemBackdrop? systemBackdrop = backdropType switch
            {
                AppBackdrop.Mica => new MicaBackdrop() { Kind = MicaKind.Base },
                AppBackdrop.MicaAlt => new MicaBackdrop() { Kind = MicaKind.BaseAlt },
                AppBackdrop.Acrylic => new DesktopAcrylicBackdrop(),
                AppBackdrop.ThinAcrylic => new ThinAcrylicBackdrop(),
                AppBackdrop.None => null,
                _ => throw new ArgumentException($"Invalid backdrop type: {backdropType}")
            };

            if (App.MainWindow!.Content is not Border rootBorder)
            {
                return;
            }

            if (systemBackdrop == null)
            {
                ElementTheme currentTheme = rootBorder.ActualTheme;
                rootBorder.Background = currentTheme == ElementTheme.Light ? new SolidColorBrush(Colors.White) : new SolidColorBrush(Colors.Black);
            }
            else
            {
                rootBorder.Background = new SolidColorBrush(Colors.Transparent);
            }

            App.MainWindow!.SystemBackdrop = systemBackdrop;
        }

        public void ApplyTheme(AppTheme themeType)
        {
            ElementTheme elementTheme = themeType switch
            {
                AppTheme.Light => ElementTheme.Light,
                AppTheme.Dark => ElementTheme.Dark,
                AppTheme.Default => ElementTheme.Default,
                _ => throw new ArgumentException($"Invalid theme type: {themeType}")
            };

            if (App.MainWindow!.Content is not Border rootBorder)
            {
                return;
            }

            rootBorder.RequestedTheme = elementTheme;

            if (App.MainWindow!.SystemBackdrop == null)
            {
                if (elementTheme == ElementTheme.Default)
                {
                    UISettings uiSettings = new();
                    Color color = uiSettings.GetColorValue(UIColorType.Background);
                    rootBorder.Background = new SolidColorBrush(color == Colors.Black ? Colors.Black : Colors.White);
                }
                else
                {
                    rootBorder.Background = new SolidColorBrush(elementTheme == ElementTheme.Dark ? Colors.Black : Colors.White);
                }
            }
        }
    }
}
