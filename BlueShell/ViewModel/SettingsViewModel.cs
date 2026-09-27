using BlueShell.Model.Settings;
using BlueShell.Services.Settings;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;

namespace BlueShell.ViewModel
{
    public sealed partial class SettingsViewModel(IWindowAppearanceService windowAppearanceService,
                                                  IImageService imageService,
                                                  IFilePickerService filePickerService) : ObservableObject
    {
        public IReadOnlyList<GradientDirection> GradientDirections { get; } = Enum.GetValues<GradientDirection>();

        [ObservableProperty]
        public partial AppBackdrop SelectedBackdrop { get; set; } = AppBackdrop.MicaAlt;

        [ObservableProperty]
        public partial AppTheme SelectedTheme { get; set; } = AppTheme.Default;

        [ObservableProperty]
        public partial AppBackgroundType SelectedBackgroundType { get; set; } = AppBackgroundType.Backdrop;

        [ObservableProperty]
        public partial string Color1 { get; set; } = "";

        [ObservableProperty]
        public partial string Color2 { get; set; } = "";

        [ObservableProperty]
        public partial string Color3 { get; set; } = "";

        [ObservableProperty]
        public partial bool IsDesktopWallpaper { get; set; }

        [ObservableProperty]
        public partial GradientDirection SelectedGradientDirection { get; set; } = GradientDirection.TopToBottom;

        [RelayCommand]
        private void SetSelectedBackdrop(AppBackdrop backdrop)
        {
            SelectedBackdrop = backdrop;
        }

        [RelayCommand]
        private void SetSelectedTheme(AppTheme theme)
        {
            SelectedTheme = theme;
            windowAppearanceService.ApplyTheme(theme);
        }

        [RelayCommand]
        private async Task SetSelectedBackgroundType(AppBackgroundType backgroundType)
        {
            SelectedBackgroundType = backgroundType;

            if (!windowAppearanceService.BackgroundExists())
            {
                return;
            }

            switch (backgroundType)
            {
                case AppBackgroundType.Color:
                    SetLinearGradient();
                    return;
                case AppBackgroundType.Image:
                    string imageFilePath = imageService.GetSavedImageFilePath();
                    await SetImage(imageFilePath == string.Empty ? null : await StorageFile.GetFileFromPathAsync(imageFilePath));
                    return;
                case AppBackgroundType.Backdrop:
                    windowAppearanceService.RemoveBackground();
                    windowAppearanceService.ApplyBackdrop(SelectedBackdrop);
                    windowAppearanceService.ApplyTheme(SelectedTheme);
                    return;
                default:
                    throw new ArgumentOutOfRangeException(nameof(backgroundType), backgroundType, null);
            }
        }

        [RelayCommand]
        private async Task BrowseImageAsync()
        {
            StorageFile? imageFile = await filePickerService.PickImageAsync();

            if (imageFile == null)
            {
                return;
            }

            windowAppearanceService.RemoveBackground();

            await SetImage(imageFile);
        }

        [RelayCommand]
        private void SetLinearGradient()
        {
            var (startPoint, endPoint) = windowAppearanceService.GetGradientPoints(SelectedGradientDirection);

            var (color1, color2, color3) = windowAppearanceService.GetColorsFromInput(Color1, Color2, Color3);

            GradientBackground gradientBackground = new()
            {
                Colors = [color1, color2, color3],
                StartPoint = startPoint,
                EndPoint = endPoint
            };

            bool isGradientBright = imageService.IsGradientBright([color1, color2, color3]);

            windowAppearanceService.ApplyBackground(gradientBackground, isGradientBright ? AppTheme.Light : AppTheme.Dark);
        }

        [RelayCommand]
        private void SetSelectedGradientDirection(GradientDirection direction)
        {
            SelectedGradientDirection = direction;
        }

        partial void OnIsDesktopWallpaperChanged(bool value)
        {
            _ = SetDesktopWallpaper(value);
        }

        partial void OnSelectedGradientDirectionChanged(GradientDirection value)
        {
            SelectedGradientDirection = value;
        }

        private async Task SetImage(StorageFile? imageFile)
        {
            if (imageFile == null)
            {
                return;
            }

            WriteableBitmap writeableBitmap = await imageService.LoadWriteableBitmap(imageFile);
            bool isImageBright = imageService.IsImageBright(writeableBitmap);

            imageService.SetImageFilePath(imageFile.Path);

            ImageBackground imageBackground = new()
            {
                ImageFilePath = imageFile.Path
            };

            windowAppearanceService.ApplyBackground(imageBackground, isImageBright ? AppTheme.Light : AppTheme.Dark);
        }

        private async Task SetDesktopWallpaper(bool value)
        {
            if (value)
            {
                string desktopWallpaperPath = imageService.GetDesktopWallpaperPath();

                StorageFile imageFile = await StorageFile.GetFileFromPathAsync(desktopWallpaperPath);

                await SetImage(imageFile);
            }
            else
            {
                imageService.ClearSaveImageFilepath();
                windowAppearanceService.RemoveBackground();
                windowAppearanceService.ApplyBackdrop(SelectedBackdrop);
                windowAppearanceService.ApplyTheme(SelectedTheme);
            }
        }
    }
}
