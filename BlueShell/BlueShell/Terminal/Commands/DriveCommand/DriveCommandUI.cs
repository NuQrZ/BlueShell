using BlueShell.Core;
using BlueShell.Model;
using BlueShell.Services.Wrappers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Threading.Tasks;

namespace BlueShell.Terminal.Commands.DriveCommand
{
    public static class DriveCommandUI
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
                SizeMargin = new Thickness(265, 10, 0, 10),
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
                SizeMargin = new Thickness(265, 10, 0, 10),
                TypeMargin = new Thickness(200, 10, 0, 10),
                SubFoldersMargin = new Thickness(200, 10, 0, 10)
            };
        }

        public static DataDisplayItem CreateDriveItem(DriveItem driveItem)
        {
            return new DataDisplayItem()
            {
                ItemName = driveItem.DisplayName,
                ItemType = "Drive",
                DriveInfo = driveItem.DriveInfo,
                TakenSpace = driveItem.TakenSpaceBytes,
                TotalSize = driveItem.TotalSize
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
                FileInfo = fileSystemItem.FileInfo
            };
        }

        public static async Task ConfigureDriveDataItem(DataDisplayItem dataDisplayItem)
        {
            dataDisplayItem.TextPadding = new Thickness(50, 0, 0, 0);
            dataDisplayItem.ImageMargin = new Thickness(0, 10, 0, 5);
            dataDisplayItem.ProgressMargin = new Thickness(400, 0, 0, 0);
            dataDisplayItem.ItemTypeMargin = new Thickness(710, 0, 0, 0);

            string? filePath = dataDisplayItem.DriveInfo?.RootDirectory.FullName;
            BitmapImage? itemIcon = await Utilities.GetItemIcon(filePath);
            double takenSpace = dataDisplayItem.TakenSpace;

            dataDisplayItem.ItemIcon = itemIcon;
            dataDisplayItem.ImageSize = 40;
            dataDisplayItem.IsTakenSpaceVisible = true;
            dataDisplayItem.IsSizeVisible = false;
            dataDisplayItem.Color = takenSpace < 34359738368 ? "Red" : "CornflowerBlue";
        }

        public static async Task ConfigureFileSystemDataItem(DataDisplayItem dataDisplayItem)
        {
            dataDisplayItem.TextPadding = new Thickness(50, 0, 0, 0);
            dataDisplayItem.ImageMargin = new Thickness(0, 10, 0, 5);
            dataDisplayItem.ProgressMargin = new Thickness(400, 0, 0, 0);
            dataDisplayItem.ItemTypeMargin = new Thickness(680, 0, 0, 0);

            string? filePath = dataDisplayItem.DirectoryInfo != null ? dataDisplayItem.DirectoryInfo?.FullName : dataDisplayItem.FileInfo?.FullName;
            BitmapImage? itemIcon = await Utilities.GetItemIcon(filePath);
            double takenSpace = dataDisplayItem.TakenSpace;

            dataDisplayItem.ItemIcon = itemIcon;
            dataDisplayItem.ImageSize = 40;
            dataDisplayItem.IsTakenSpaceVisible = false;
            dataDisplayItem.IsSizeVisible = true;
            dataDisplayItem.Color = "Transparent";
        }
    }
}
