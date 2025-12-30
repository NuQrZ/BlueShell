using BlueShell.Services.Wrappers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace BlueShell.Services
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
                string driveFilePath = driveInfo.RootDirectory.FullName;
                string? driveName = await GetDriveDisplayName(driveFilePath);

                long totalFreeSpace = driveInfo.TotalFreeSpace;
                long totalSize = driveInfo.TotalSize;

                long takenSpace = totalSize - totalFreeSpace;

                if (driveName == null)
                {
                    continue;
                }

                DriveItem driveItem = new()
                {
                    DisplayName = driveName,
                    DriveInfo = driveInfo,
                    TakenSpaceBytes = takenSpace,
                    TotalSize = totalSize
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
