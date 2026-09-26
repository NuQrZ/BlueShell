using BlueShell.Helpers;
using BlueShell.Model.Settings;
using Microsoft.UI;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.ViewManagement;

namespace BlueShell.Services.Settings
{
    public sealed class WindowAppearanceService : IWindowAppearanceService
    {
        private readonly List<Border> _rootElements = [];
        private readonly List<AppWindowTitleBar> _titleBars = [];
        private static readonly Regex ColorRegex = new(
            @"^((?:#[a-fA-F0-9]{6}|#[a-fA-F0-9]{8})|(\d{1,3},\d{1,3},\d{1,3}|\d{1,3},\d{1,3},\d{1,3},\d{1,3}))$",
            RegexOptions.Compiled);

        private static ElementTheme GetElementTheme(AppTheme appTheme)
        {
            return appTheme switch
            {
                AppTheme.Light => ElementTheme.Light,
                AppTheme.Dark => ElementTheme.Dark,
                AppTheme.Default => ElementTheme.Default,
                _ => throw new ArgumentException($"Invalid theme type: {appTheme}")
            };
        }

        private static SystemBackdrop? GetSystemBackdrop(AppBackdrop appBackdrop)
        {
            return appBackdrop switch
            {
                AppBackdrop.Mica => new MicaBackdrop() { Kind = MicaKind.Base },
                AppBackdrop.MicaAlt => new MicaBackdrop() { Kind = MicaKind.BaseAlt },
                AppBackdrop.Acrylic => new DesktopAcrylicBackdrop(),
                AppBackdrop.ThinAcrylic => new ThinAcrylicBackdrop(),
                AppBackdrop.None => null,
                _ => throw new ArgumentException($"Invalid backdrop type: {appBackdrop}")
            };
        }

        private static LinearGradientBrush CreateLinearGradientBrush(GradientBackground gradientBackground)
        {
            LinearGradientBrush linearGradientBrush = new()
            {
                StartPoint = gradientBackground.StartPoint,
                EndPoint = gradientBackground.EndPoint
            };

            int colorCount = gradientBackground.Colors.Count;

            for (int i = 0; i < colorCount; i++)
            {
                double offset = colorCount == 1
                    ? 0
                    : (double)i / (colorCount - 1);

                linearGradientBrush.GradientStops.Add(new GradientStop()
                {
                    Color = gradientBackground.Colors[i],
                    Offset = offset
                });
            }

            return linearGradientBrush;
        }

        private static Color GetColorFromString(string colorValue)
        {
            Match colorMatch = ColorRegex.Match(colorValue);

            if (!colorMatch.Success)
            {
                return Colors.Transparent;
            }

            string value = colorMatch.Groups[0].Value.Replace("#", "");

            if (colorValue.StartsWith('#'))
            {
                string aStringValue;
                string rStringValue;
                string gStringValue;
                string bStringValue;
                if (colorValue.Length == 7)
                {
                    aStringValue = "FF";
                    rStringValue = value[..2];
                    gStringValue = value.Substring(2, 2);
                    bStringValue = value.Substring(4, 2);
                }
                else
                {
                    aStringValue = value[..2];
                    rStringValue = value.Substring(2, 2);
                    gStringValue = value.Substring(4, 2);
                    bStringValue = value.Substring(6, 2);
                }

                byte a = Convert.ToByte(aStringValue, 16);
                byte r = Convert.ToByte(rStringValue, 16);
                byte g = Convert.ToByte(gStringValue, 16);
                byte b = Convert.ToByte(bStringValue, 16);

                return Color.FromArgb(a, r, g, b);
            }
            else
            {
                string[] colorNumbers = colorValue.Split(',');

                int a = colorNumbers.Length == 3 ? 255 : Convert.ToInt32(colorNumbers[0]);
                int r = colorNumbers.Length == 3 ? Convert.ToInt32(colorNumbers[0]) : Convert.ToInt32(colorNumbers[1]);
                int g = colorNumbers.Length == 3 ? Convert.ToInt32(colorNumbers[1]) : Convert.ToInt32(colorNumbers[2]);
                int b = colorNumbers.Length == 3 ? Convert.ToInt32(colorNumbers[2]) : Convert.ToInt32(colorNumbers[3]);

                if (a is < 0 or > 255 ||
                    r is < 0 or > 255 ||
                    g is < 0 or > 255 ||
                    b is < 0 or > 255)
                {
                    return Colors.Transparent;
                }

                return Color.FromArgb((byte)a, (byte)r, (byte)g, (byte)b);
            }
        }

        public void RemoveBackground()
        {
            foreach (Border rootElement in _rootElements)
            {
                rootElement.Background = null;
            }

            App.MainWindow!.SystemBackdrop = null;
        }

        public void ApplyBackdrop(AppBackdrop appBackdrop)
        {
            RemoveBackground();

            SystemBackdrop? systemBackdrop = GetSystemBackdrop(appBackdrop);

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

            App.MainWindow.SystemBackdrop = systemBackdrop;
        }

        public void ApplyTheme(AppTheme appTheme)
        {
            ElementTheme elementTheme = GetElementTheme(appTheme);

            if (App.MainWindow!.Content is not Border rootBorder)
            {
                return;
            }

            rootBorder.RequestedTheme = elementTheme;

            if (App.MainWindow.SystemBackdrop != null)
            {
                return;
            }

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

        public void ApplyBackground(AppBackground appBackground, AppTheme appTheme)
        {
            RemoveBackground();

            App.MainWindow!.SystemBackdrop = null;

            Brush brush = appBackground switch
            {
                ImageBackground imageBackground => new ImageBrush
                {
                    ImageSource = new BitmapImage(new Uri(imageBackground.ImageFilePath))
                },
                GradientBackground gradientBackground => CreateLinearGradientBrush(gradientBackground),
                _ => throw new ArgumentOutOfRangeException(nameof(appBackground))
            };

            ElementTheme elementTheme = GetElementTheme(appTheme);

            if (App.MainWindow.Content is Border border)
            {
                border.RequestedTheme = elementTheme;
            }

            foreach (AppWindowTitleBar appWindowTitleBar in _titleBars)
            {
                appWindowTitleBar.ButtonForegroundColor = elementTheme == ElementTheme.Light ? Colors.Black : Colors.White;
            }

            foreach (Border rootElement in _rootElements)
            {
                rootElement.Background = brush;
            }
        }

        public void AddBorder(Border border)
        {
            _rootElements.Add(border);
        }

        public void AddTitleBar(AppWindowTitleBar titleBar)
        {
            _titleBars.Add(titleBar);
        }

        public bool BackgroundExists()
        {
            return _rootElements.All(rootElement => rootElement.Background != null);
        }

        public (Point, Point) GetGradientPoints(GradientDirection gradientDirection)
        {
            return gradientDirection switch
            {
                GradientDirection.TopToBottom => (new Point(0.5, 0), new Point(0.5, 1)),
                GradientDirection.BottomToTop => (new Point(0.5, 1), new Point(0.5, 0)),
                GradientDirection.LeftToRight => (new Point(0, 0.5), new Point(1, 0.5)),
                GradientDirection.RightToLeft => (new Point(1, 0.5), new Point(0, 0.5)),
                GradientDirection.TopLeftToBottomRight => (new Point(0, 0), new Point(1, 1)),
                GradientDirection.BottomRightToTopLeft => (new Point(1, 1), new Point(0, 0)),
                GradientDirection.TopRightToBottomLeft => (new Point(1, 0), new Point(0, 1)),
                GradientDirection.BottomLeftToTopRight => (new Point(0, 1), new Point(1, 0)),
                _ => throw new ArgumentOutOfRangeException(nameof(gradientDirection))
            };
        }

        public (Color, Color, Color) GetColorsFromInput(string colorValue1, string colorValue2, string colorValue3)
        {
            Color color1 = GetColorFromString(colorValue1);
            Color color2 = GetColorFromString(colorValue2);
            Color color3 = GetColorFromString(colorValue3);

            return (color1, color2, color3);
        }
    }
}
