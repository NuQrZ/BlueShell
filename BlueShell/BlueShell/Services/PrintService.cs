using BlueShell.Core;
using BlueShell.Services.Wrappers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace BlueShell.Services
{
    public sealed class PrintService : IPrintService
    {
        public string PrintDrives(List<DriveItem> drives)
        {
            StringBuilder stringBuilder = new();

            const string driveLabel = "Display Name";
            const string driveRoot = "Drive Root";
            const string driveType = "Drive Type";
            const string driveFormat = "Drive Format";
            const string totalSize = "Total Size";
            const string usedSpace = "Used Space";
            const string freeSpace = "Free Space";
            const string usedPercent = "Used Percent";

            const string vertical = "|";
            const string separator = " | ";

            var formattedDrives = drives.Select(drive => new
            {
                DisplayName = drive.VolumeLabel,
                Root = drive.RootPath,
                Type = drive.DriveType,
                Format = drive.DriveFormat,
                Total = Utilities.ReturnSize(drive.TotalBytes),
                Used = Utilities.ReturnSize(drive.UsedBytes),
                Free = Utilities.ReturnSize(drive.FreeBytes),
                Percent = Math.Round(drive.UsedPrecent).ToString(CultureInfo.InvariantCulture) + " %"
            }).ToList();

            int displayWidth = Math.Max(driveLabel.Length,
                formattedDrives.Select(d => d.DisplayName.Length).DefaultIfEmpty(0).Max());
            int rootWidth = Math.Max(driveRoot.Length,
                formattedDrives.Select(d => d.Root.Length).DefaultIfEmpty(0).Max());
            int typeWidth = Math.Max(driveType.Length,
                formattedDrives.Select(d => d.Type.Length).DefaultIfEmpty(0).Max());
            int formatWidth = Math.Max(driveFormat.Length,
                formattedDrives.Select(d => d.Format.Length).DefaultIfEmpty(0).Max());
            int totalSizeWidth = Math.Max(totalSize.Length,
                formattedDrives.Select(d => d.Total.Length).DefaultIfEmpty(0).Max());
            int usedSpaceWidth = Math.Max(usedSpace.Length,
                formattedDrives.Select(d => d.Used.Length).DefaultIfEmpty(0).Max());
            int freeSpaceWidth = Math.Max(freeSpace.Length,
                formattedDrives.Select(d => d.Free.Length).DefaultIfEmpty(0).Max());
            int usedPercentWidth = Math.Max(usedPercent.Length,
                formattedDrives.Select(d => d.Percent.Length).DefaultIfEmpty(0).Max());

            int totalWidth =
                displayWidth + rootWidth + typeWidth + formatWidth +
                totalSizeWidth + usedSpaceWidth + freeSpaceWidth + usedPercentWidth +
                separator.Length * 7 + vertical.Length * 2;

            stringBuilder.AppendLine("+" + new string('-', totalWidth - 2) + "+");

            stringBuilder.AppendLine(
                vertical +
                driveLabel.PadRight(displayWidth) + separator +
                driveRoot.PadRight(rootWidth) + separator +
                driveType.PadRight(typeWidth) + separator +
                driveFormat.PadRight(formatWidth) + separator +
                totalSize.PadRight(totalSizeWidth) + separator +
                usedSpace.PadRight(usedSpaceWidth) + separator +
                freeSpace.PadRight(freeSpaceWidth) + separator +
                usedPercent.PadRight(usedPercentWidth) +
                vertical
            );

            stringBuilder.AppendLine("+" + new string('-', totalWidth - 2) + "+");

            foreach (var drive in formattedDrives)
            {
                stringBuilder.AppendLine(
                    vertical +
                    drive.DisplayName.PadRight(displayWidth) + separator +
                    drive.Root.PadRight(rootWidth) + separator +
                    drive.Type.PadRight(typeWidth) + separator +
                    drive.Format.PadRight(formatWidth) + separator +
                    drive.Total.PadRight(totalSizeWidth) + separator +
                    drive.Used.PadRight(usedSpaceWidth) + separator +
                    drive.Free.PadRight(freeSpaceWidth) + separator +
                    drive.Percent.PadRight(usedPercentWidth) +
                    vertical
                );
            }

            stringBuilder.AppendLine("+" + new string('-', totalWidth - 2) + "+");

            return stringBuilder.ToString();
        }

        public string PrintDriveProperties(List<DriveItem> drives)
        {
            StringBuilder stringBuilder = new();

            return stringBuilder.ToString();
        }

        public string PrintDriveAdvancedProperties(List<DriveItem> drives)
        {
            StringBuilder stringBuilder = new();

            return stringBuilder.ToString();
        }

        public string PrintFolderContents(List<FileSystemItem> folders)
        {
            StringBuilder stringBuilder = new();

            return stringBuilder.ToString();
        }

        public string PrintFolderProperties(List<FileSystemItem> folders)
        {
            StringBuilder stringBuilder = new();

            return stringBuilder.ToString();
        }

        public string PrintFileProperties(List<FileSystemItem> files)
        {
            StringBuilder stringBuilder = new();

            return stringBuilder.ToString();
        }
    }
}
