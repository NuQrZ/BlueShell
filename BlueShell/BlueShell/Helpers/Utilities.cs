using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI;

namespace BlueShell.Helpers
{
    public static class Utilities
    {
        public static Dictionary<string, Color> DarkThemeKeywordColors { get; } = new()
        {
            { "Exit", Color.FromArgb(255, 255, 95, 115) },
            { "Clear", Color.FromArgb(255, 0, 210, 200) },
            { "ClearDisplay", Color.FromArgb(255, 170, 170, 170)},
            { "--Version", Color.FromArgb(255, 255, 170, 70) },
            { "Drive", Color.FromArgb(255, 80, 170, 255) },
            { "-GetDrives", Color.FromArgb(255, 240, 205, 120) },
            { "-Properties", Color.FromArgb(255, 200, 140, 255) },
            { "-Advanced", Color.FromArgb(255, 120, 155, 255) },
            { "-Open", Color.FromArgb(255, 60, 235, 185) },
            { "-Print", Color.FromArgb(255, 90, 220, 120) }
        };

        public static Dictionary<string, Color> LightThemeKeywordColors { get; } = new()
        {
            { "Exit", Color.FromArgb(255, 200, 40, 75) },
            { "Clear", Color.FromArgb(255, 0, 150, 145) },
            { "ClearDisplay", Color.FromArgb(255, 95, 95, 95) },
            { "--Version", Color.FromArgb(255, 180, 110, 25) },
            { "Drive", Color.FromArgb(255, 35, 70, 220) },
            { "-GetDrives", Color.FromArgb(255, 165, 135, 30) },
            { "-Properties", Color.FromArgb(255, 145, 60, 205) },
            { "-Advanced", Color.FromArgb(255, 65, 105, 225) },
            { "-Open", Color.FromArgb(255, 20, 165, 170) },
            { "-Print", Color.FromArgb(255, 25, 150, 80) },
        };

        public static string ReturnSize(long sizeInBytes, bool returnOnlySizeType = false)
        {
            if (sizeInBytes == 0)
            {
                return "B";
            }

            string[] units = ["B", "KB", "MB", "GB", "TB", "PB"];

            if (returnOnlySizeType)
            {
                int order = (int)Math.Log(sizeInBytes, 1024);
                return units[Math.Min(order, units.Length - 1)];
            }

            long value = sizeInBytes;
            int i = 0;

            while (value >= 1024 && i < units.Length - 1)
            {
                value /= 1024;
                i++;
            }

            return value.ToString(i == 0 ? "0" : "0.##", CultureInfo.InvariantCulture) + " " + units[i];
        }

        public static async Task<BitmapImage?> GetItemIcon(string? filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return null;
            }

            try
            {
                StorageItemThumbnail? thumbnail;

                if (Directory.Exists(filePath))
                {
                    StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(filePath);
                    thumbnail = await folder.GetThumbnailAsync(
                        ThumbnailMode.SingleItem,
                        40,
                        ThumbnailOptions.UseCurrentScale);
                }
                else if (File.Exists(filePath))
                {
                    StorageFile file = await StorageFile.GetFileFromPathAsync(filePath);
                    thumbnail = await file.GetThumbnailAsync(
                        ThumbnailMode.SingleItem,
                        40,
                        ThumbnailOptions.UseCurrentScale);
                }
                else
                {
                    return null;
                }

                if (thumbnail is null || thumbnail.Size == 0)
                {
                    thumbnail?.Dispose();
                    return null;
                }

                using (thumbnail)
                {
                    BitmapImage bitmapImage = new();
                    await bitmapImage.SetSourceAsync(thumbnail);
                    return bitmapImage;
                }
            }
            catch (UnauthorizedAccessException)
            {
                return null;
            }
            catch (FileNotFoundException)
            {
                return null;
            }
            catch (DirectoryNotFoundException)
            {
                return null;
            }
        }
    }
}
