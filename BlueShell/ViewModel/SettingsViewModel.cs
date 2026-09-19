using BlueShell.Model;
using BlueShell.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BlueShell.ViewModel
{
    public sealed partial class SettingsViewModel(IWindowAppearanceService windowAppearanceService) : ObservableObject
    {
        [ObservableProperty]
        public partial AppBackdrop SelectedBackdrop { get; set; } = AppBackdrop.MicaAlt;

        [ObservableProperty]
        public partial AppTheme SelectedTheme { get; set; } = AppTheme.Default;

        [RelayCommand]
        private void SetSelectedBackdrop(AppBackdrop backdrop)
        {
            SelectedBackdrop = backdrop;
            windowAppearanceService.ApplyBackdrop(backdrop);
        }

        [RelayCommand]
        private void SetSelectedTheme(AppTheme theme)
        {
            SelectedTheme = theme;
            windowAppearanceService.ApplyTheme(theme);
        }
    }
}
