using BlueShell.Model;

namespace BlueShell.Services
{
    public interface IWindowAppearanceService
    {
        void ApplyBackdrop(AppBackdrop backdropType);
        void ApplyTheme(AppTheme themeType);
    }
}
