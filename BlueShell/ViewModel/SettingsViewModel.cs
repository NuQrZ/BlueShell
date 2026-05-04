using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BlueShell.ViewModel
{
    public sealed partial class SettingsViewModel : ObservableObject
    {
        [ObservableProperty]
        public partial string SelectedBackdrop { get; set; } = "MicaAlt";

        [ObservableProperty]
        public partial string SelectedTheme { get; set; } = "Default";

        [RelayCommand]
        private void SetSelectedBackdrop(string backdrop)
        {
            SelectedBackdrop = backdrop;
        }

        [RelayCommand]
        private void SetSelectedTheme(string theme)
        {
            SelectedTheme = theme;
        }
    }
}
