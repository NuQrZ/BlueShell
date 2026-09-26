using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace BlueShell.Services.Settings
{
    internal class FilePickerService : IFilePickerService
    {
        public async Task<StorageFile?> PickImageAsync()
        {
            nint hWnd = WindowNative.GetWindowHandle(App.MainWindow);

            FileOpenPicker fileOpenPicker = new FileOpenPicker();

            InitializeWithWindow.Initialize(fileOpenPicker, hWnd);

            fileOpenPicker.ViewMode = PickerViewMode.Thumbnail;
            fileOpenPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            fileOpenPicker.FileTypeFilter.Add(".jpg");
            fileOpenPicker.FileTypeFilter.Add(".jpeg");
            fileOpenPicker.FileTypeFilter.Add(".png");

            StorageFile? imageFile = await fileOpenPicker.PickSingleFileAsync();
            return imageFile == null ? null : imageFile;
        }
    }
}
