using BlueShell.Helpers;
using BlueShell.Model;
using BlueShell.Model.Properties;
using BlueShell.Services.Wrappers;
using BlueShell.View.AppWindows;
using BlueShell.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI;

namespace BlueShell.Terminal.Commands
{
    public static class UiClass
    {
        public static DataDisplayHeader CreateDriveHeader()
        {
            return new DataDisplayHeader()
            {
                NameHeader = "Drive Name",
                SizeHeader = "Taken Space",
                TypeHeader = "Type",
                SubFoldersHeader = "",
                NameMargin = new Thickness(110, 10, 0, 10),
                SizeMargin = new Thickness(255, 10, 0, 10),
                TypeMargin = new Thickness(200, 10, 0, 10),
                SubFoldersMargin = new Thickness(0, 0, 0, 0)
            };
        }

        public static DataDisplayHeader CreateFileSystemHeader()
        {
            return new DataDisplayHeader()
            {
                NameHeader = "Item Name",
                TypeHeader = "Item type",
                SizeHeader = "Item Size",
                SubFoldersHeader = "Subfolders",
                NameMargin = new Thickness(110, 10, 0, 10),
                SizeMargin = new Thickness(355, 10, 0, 10),
                TypeMargin = new Thickness(85, 10, 0, 10),
                SubFoldersMargin = new Thickness(200, 10, 0, 10)
            };
        }

        public static DataDisplayItem CreateDriveDisplayItem(DriveItem driveItem)
        {
            return new DataDisplayItem()
            {
                ItemName = driveItem.VolumeLabel,
                ItemType = driveItem.DriveType + " Drive",
                TakenSpace = driveItem.UsedBytes,
                TotalSize = driveItem.TotalBytes,
                DriveFilePath = driveItem.RootPath
            };
        }

        public static DataDisplayItem CreateFileSystemItem(FileSystemItem fileSystemItem)
        {
            return new DataDisplayItem()
            {
                ItemName = fileSystemItem.ItemName,
                ItemType = fileSystemItem.ItemType,
                ItemSizeType = fileSystemItem.ItemSizeType,
                ItemSize = fileSystemItem.ItemSize,
                DirectoryInfo = fileSystemItem.DirectoryInfo,
                FileInfo = fileSystemItem.FileInfo,
            };
        }

        public static async Task ConfigureDriveDisplayItem(DataDisplayItem dataDisplayItem,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            dataDisplayItem.TextPadding = new Thickness(50, 0, 0, 0);
            dataDisplayItem.ImageMargin = new Thickness(0, 10, 0, 5);
            dataDisplayItem.ProgressMargin = new Thickness(400, 0, 0, 0);
            dataDisplayItem.ItemTypeMargin = new Thickness(710, 0, 0, 0);

            string? filePath = dataDisplayItem.DriveFilePath;
            BitmapImage? itemIcon = await Utilities.GetItemIcon(filePath);
            double takenSpace = dataDisplayItem.TakenSpace;

            dataDisplayItem.ItemIcon = itemIcon;
            dataDisplayItem.ImageSize = 40;
            dataDisplayItem.IsTakenSpaceVisible = true;
            dataDisplayItem.IsSizeVisible = false;

            Color color = takenSpace < 34359738368 ? Colors.Red : Colors.CornflowerBlue;
            dataDisplayItem.Color = new SolidColorBrush(color);
        }

        public static async Task ConfigureFileSystemDataItem(DataDisplayItem dataDisplayItem,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            dataDisplayItem.TextPadding = new Thickness(50, 0, 0, 0);
            dataDisplayItem.ImageMargin = new Thickness(0, 10, 0, 5);
            dataDisplayItem.ProgressMargin = new Thickness(400, 0, 0, 0);
            dataDisplayItem.ItemTypeMargin = new Thickness(680, 0, 0, 0);

            string? filePath = dataDisplayItem.DirectoryInfo != null
                ? dataDisplayItem.DirectoryInfo?.FullName
                : dataDisplayItem.FileInfo?.FullName;
            BitmapImage? itemIcon = await Utilities.GetItemIcon(filePath);

            dataDisplayItem.ItemIcon = itemIcon;
            dataDisplayItem.ImageSize = 40;
            dataDisplayItem.IsTakenSpaceVisible = false;
            dataDisplayItem.IsSizeVisible = true;
            dataDisplayItem.Color = new SolidColorBrush(Colors.Transparent);
        }

        public static void InitializeDriveProperties(List<PropertyItem> propertyItems)
        {
            DispatcherQueue dispatcherQueue = App.MainWindow?.DispatcherQueue ?? DispatcherQueue.GetForCurrentThread();

            dispatcherQueue.TryEnqueue(() =>
            {
                PropertiesWindowViewModel propertiesWindowViewModel =
                    App.ServiceProvider!.GetRequiredService<PropertiesWindowViewModel>();
                propertiesWindowViewModel.ClearItems();
                foreach (PropertyItem propertyItem in propertyItems)
                {
                    propertiesWindowViewModel.AddItem(propertyItem);
                }

                propertiesWindowViewModel.SelectedItem = propertyItems[0];

                PropertiesWindow propertiesWindow = new(propertiesWindowViewModel);
                propertiesWindow.Activate();
            });
        }
    }
}