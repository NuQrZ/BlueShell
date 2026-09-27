using Microsoft.UI.Xaml.Media.Imaging;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI;

namespace BlueShell.Services.Settings
{
    public interface IImageService
    {
        void SetImageFilePath(string filePath);
        void ClearSaveImageFilepath();
        bool IsImageBright(WriteableBitmap writeableBitmap);
        bool IsGradientBright(List<Color> colors);
        string GetDesktopWallpaperPath();
        string GetSavedImageFilePath();
        Task<WriteableBitmap> LoadWriteableBitmap(StorageFile imageFile);
    }
}
