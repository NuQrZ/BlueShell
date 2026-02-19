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
            { "Exit", Color.FromArgb(255, 255, 70, 110) },
            { "Clear", Color.FromArgb(255,   0, 220, 225) },
            { "ClearDisplay", Color.FromArgb(255, 185, 185, 185) },
            { "--Version", Color.FromArgb(255, 255, 165, 60) },
            { "Drive", Color.FromArgb(255,  50, 160, 255) },
            { "-GetDrives", Color.FromArgb(255, 255, 215, 120) },
            { "-Properties", Color.FromArgb(255, 215, 120, 255) },
            { "-Advanced", Color.FromArgb(255, 100, 140, 255) },
            { "-Open", Color.FromArgb(255,  40, 235, 200) },
            { "-Print", Color.FromArgb(255,  60, 255, 145) }
        };
        public static Dictionary<string, Color> LightThemeKeywordColors { get; } = new()
        {
            { "Exit", Color.FromArgb(255, 210,  30,  80) },
            { "Clear", Color.FromArgb(255,   0, 155, 155) },
            { "ClearDisplay", Color.FromArgb(255,  90,  90,  90) },
            { "--Version", Color.FromArgb(255, 190, 115,  15) },
            { "Drive", Color.FromArgb(255,  20,  45, 225) },
            { "-GetDrives", Color.FromArgb(255, 175, 145,  25) },
            { "-Properties", Color.FromArgb(255, 155,  55, 215) },
            { "-Advanced", Color.FromArgb(255,  55,  95, 235) },
            { "-Open", Color.FromArgb(255,  10, 175, 180) },
            { "-Print", Color.FromArgb(255,  10, 155,  70) }
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
