using BlueShell.Helpers;
using BlueShell.Model;
using BlueShell.Services.Wrappers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Threading.Tasks;

namespace BlueShell.Terminal.Commands.DriveCommand
{
    public static class DriveCommandUi
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

        public static async Task ConfigureDriveDisplayItem(DataDisplayItem dataDisplayItem)
        {
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
            dataDisplayItem.Color = takenSpace < 34359738368 ? "Red" : "CornflowerBlue";
        }
    }
}
