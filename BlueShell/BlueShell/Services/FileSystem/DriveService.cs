using BlueShell.Services.Wrappers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace BlueShell.Services.FileSystem
{
    public sealed class DriveService : IDriveService
    {
        private async Task<string?> GetDriveDisplayName(string driveFilePath)
        {
            string? driveName;
            try
            {
                StorageFolder driveFolder = await StorageFolder.GetFolderFromPathAsync(driveFilePath);
                driveName = driveFolder.DisplayName;
            }
            catch (Exception)
            {
                driveName = GetDrive(driveFilePath)?.VolumeLabel;
            }

            return driveName;
        }

        public async Task<List<DriveItem>> GetDrives()
        {
            List<DriveItem> driveEntries = [];
            DriveInfo[] drives = DriveInfo.GetDrives();
            foreach (DriveInfo driveInfo in drives)
            {
                if (!driveInfo.IsReady)
                {
                    continue;
                }

                string rootPath = driveInfo.RootDirectory.FullName;
                string? volumeLabel = await GetDriveDisplayName(rootPath);
                string driveType = driveInfo.DriveType.ToString();
                string driveFormat = driveInfo.DriveFormat;

                long totalFreeSpace = driveInfo.TotalFreeSpace;
                long totalSize = driveInfo.TotalSize;

                long takenSpace = totalSize - totalFreeSpace;

                if (volumeLabel == null)
                {
                    continue;
                }

                double usedPrecent = totalSize > 0 ? (double)takenSpace / totalSize * 100 : 0;

                bool isReady = driveInfo.IsReady;
                bool isSystemDrive = string.Equals(driveInfo.Name,
                    Environment.GetFolderPath(Environment.SpecialFolder.Windows)[..3],
                    StringComparison.OrdinalIgnoreCase);

                DriveItem driveItem = new()
                {
                    RootPath = rootPath,
                    VolumeLabel = volumeLabel,
                    DriveType = driveType,
                    DriveFormat = driveFormat,
                    TotalBytes = totalSize,
                    UsedBytes = takenSpace,
                    FreeBytes = totalFreeSpace,
                    IsReady = driveInfo.IsReady,
                    IsSystemDrive = isSystemDrive,
                    UsedPrecent = usedPrecent,
                };

                driveEntries.Add(driveItem);
            }

            return driveEntries;
        }

        public DriveInfo? GetDrive(string filePath)
        {
            return (from DriveInfo driveInfo in DriveInfo.GetDrives()
                where driveInfo.RootDirectory.FullName == filePath
                select driveInfo).FirstOrDefault();
        }
    }
}
