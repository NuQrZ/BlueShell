using BlueShell.Helpers;
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
        private static DriveInfo? GetDrive(string filePath)
        {
            // filePath može biti "C:\\" ili "C:"
            string normalized = NormalizeDriveKey(filePath);

            return DriveInfo.GetDrives()
                .FirstOrDefault(d => string.Equals(
                    NormalizeDriveKey(d.RootDirectory.FullName),
                    normalized,
                    StringComparison.OrdinalIgnoreCase));
        }

        private static string NormalizeDriveKey(string? drivePath)
        {
            // "C:\\" -> "C:" ; "C:" -> "C:"
            drivePath = (drivePath ?? "").Trim();
            if (drivePath.EndsWith("\\", StringComparison.Ordinal))
                drivePath = drivePath.TrimEnd('\\');
            return drivePath;
        }

        private async Task<DriveItem?> BuildDriveItem(string filePath)
        {
            DriveInfo? driveInfo = GetDrive(filePath);
            if (driveInfo == null)
                return null;

            string rootPath = driveInfo.RootDirectory.FullName; // "C:\\"
            string volumeLabel = await GetDriveDisplayName(rootPath);

            long totalSize = driveInfo.TotalSize;
            long totalFreeSpace = driveInfo.TotalFreeSpace;
            long usedBytes = totalSize - totalFreeSpace;

            double usedPrecent = totalSize > 0
                ? (double)usedBytes / totalSize * 100.0
                : 0.0;

            bool isSystemDrive =
                string.Equals(
                    driveInfo.Name,
                    Environment.GetFolderPath(Environment.SpecialFolder.Windows)[..3],
                    StringComparison.OrdinalIgnoreCase);

            return new DriveItem
            {
                RootPath = rootPath,
                VolumeLabel = volumeLabel,
                DriveType = driveInfo.DriveType.ToString(),
                DriveFormat = driveInfo.DriveFormat,
                TotalBytes = totalSize,
                UsedBytes = usedBytes,
                FreeBytes = totalFreeSpace,
                IsReady = true,
                IsSystemDrive = isSystemDrive,
                UsedPrecent = usedPrecent,
            };
        }

        public async Task<string> GetDriveDisplayName(string driveFilePath)
        {
            try
            {
                StorageFolder driveFolder = await StorageFolder.GetFolderFromPathAsync(driveFilePath);
                return driveFolder.DisplayName;
            }
            catch (Exception)
            {
                return GetDrive(driveFilePath)?.VolumeLabel ?? "";
            }
        }

        public async Task<List<DriveItem>> GetDrives()
        {
            List<DriveItem> driveEntries = [];
            DriveInfo[] drives = DriveInfo.GetDrives();

            foreach (DriveInfo driveInfo in drives)
            {
                if (!driveInfo.IsReady)
                    continue;

                DriveItem? driveItem = await BuildDriveItem(driveInfo.RootDirectory.FullName);
                if (driveItem == null)
                    continue;

                driveEntries.Add(driveItem);
            }

            return driveEntries;
        }

        public Dictionary<string, object> GetDriveProperties(string driveLetter)
        {
            string key = NormalizeDriveKey(driveLetter);

            if (!WmiUtilities.DriveProperties.TryGetValue(key, out var allProperties))
                return [];

            return allProperties
                .Where(kv => WmiUtilities.GeneralKeys.Contains(kv.Key))
                .ToDictionary(kv => kv.Key, kv => kv.Value, StringComparer.OrdinalIgnoreCase);
        }

        public Dictionary<string, object> GetAdvancedDriveProperties(string driveLetter)
        {
            string key = NormalizeDriveKey(driveLetter);

            if (!WmiUtilities.DriveProperties.TryGetValue(key, out var allProperties))
                return [];

            return allProperties
                .Where(kv => !WmiUtilities.GeneralKeys.Contains(kv.Key))
                .ToDictionary(kv => kv.Key, kv => kv.Value, StringComparer.OrdinalIgnoreCase);
        }
    }
}