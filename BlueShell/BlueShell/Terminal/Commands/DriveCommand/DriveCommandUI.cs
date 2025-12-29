using BlueShell.Core;
using BlueShell.Model;
using BlueShell.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Threading.Tasks;

namespace BlueShell.Terminal.Commands.DriveCommand
{
    public static class DriveCommandUI
    {
        public static DataDisplayHeader CreateHeaderType1()
        {
            return new DataDisplayHeader()
            {
                NameHeader = "Drive Name",
                SizeHeader = "Type",
                TypeHeader = "Taken Space",
                SubFoldersHeader = "",
                NameMargin = new Thickness(110, 10, 0, 0),
                SizeMargin = new Thickness(225, 10, 0, 10),
                TypeMargin = new Thickness(445, 10, 0, 10),
                SubFoldersMargin = new Thickness(0, 0, 0, 0)
            };
        }

        public static DataDisplayItem CreateDriveItem(DriveEntry driveEntry)
        {
            return new DataDisplayItem()
            {
                ItemName = driveEntry.DisplayName,
                ItemType = "Drive",
                DriveInfo = driveEntry.DriveInfo,
                TakenSpace = driveEntry.TakenSpaceBytes,
                TotalSize = driveEntry.TotalSize
            };
        }

        public static async Task ConfigureDataItem(DataDisplayItem dataDisplayItem)
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
    }
}
