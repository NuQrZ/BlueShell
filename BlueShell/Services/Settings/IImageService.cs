using Microsoft.UI.Xaml.Media.Imaging;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI;

namespace BlueShell.Services.Settings
{
    public interface IImageService
    {
        bool IsImageBright(WriteableBitmap writeableBitmap);
        bool IsGradientBright(List<Color> colors);
        string GetDesktopWallpaperPath();
        Task<WriteableBitmap> LoadWriteableBitmap(StorageFile imageFile);
    }
}
