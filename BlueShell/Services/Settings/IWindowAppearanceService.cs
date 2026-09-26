using BlueShell.Model.Settings;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation;
using Windows.UI;

namespace BlueShell.Services.Settings
{
    public interface IWindowAppearanceService
    {
        void ApplyBackdrop(AppBackdrop appBackdrop);
        void ApplyTheme(AppTheme appTheme);
        void ApplyBackground(AppBackground appBackground, AppTheme appTheme);
        void RemoveBackground();
        void AddBorder(Border border);
        void AddTitleBar(AppWindowTitleBar titleBar);
        bool BackgroundExists();
        (Point, Point) GetGradientPoints(GradientDirection gradientDirection);
        (Color, Color, Color) GetColorsFromInput(string colorValue1, string colorValue2, string colorValue3);
    }
}
