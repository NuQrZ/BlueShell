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
            { "Exit", Color.FromArgb(255, 255, 80, 80) },
            { "ClearAll", Color.FromArgb(255, 0, 200, 255) },
            { "Clear", Color.FromArgb(255, 0, 255, 200) },
            { "ClearDisplay", Color.FromArgb(255, 180, 180, 180) },
            { "--Version", Color.FromArgb(255, 255, 185, 0) },

            { "Drive", Color.FromArgb(255, 70, 150, 255) },
            { "Folder", Color.FromArgb(255, 0, 235, 255) },

            { "-GetDrives", Color.FromArgb(255, 255, 215, 80) },
            { "-Open", Color.FromArgb(255, 0, 255, 140) },
            { "-Delete", Color.FromArgb(255, 255, 70, 120) },
            { "-Move", Color.FromArgb(255, 255, 140, 0) },
            { "-Copy", Color.FromArgb(255, 80, 255, 80) },
            { "-Rename", Color.FromArgb(255, 200, 120, 255) },
            { "-Properties", Color.FromArgb(255, 255, 120, 220) },
            { "-Advanced", Color.FromArgb(255, 120, 140, 255) },
            { "-Print", Color.FromArgb(255, 170, 255, 0) },

            { "-True", Color.FromArgb(255, 255, 230, 90) },
            { "-False", Color.FromArgb(255, 255, 170, 170) }
        };

        public static Dictionary<string, Color> LightThemeKeywordColors { get; } = new()
        {
            { "Exit", Color.FromArgb(255, 210, 30, 30) },
            { "ClearAll", Color.FromArgb(255, 0, 140, 220) },
            { "Clear", Color.FromArgb(255, 0, 170, 140) },
            { "ClearDisplay", Color.FromArgb(255, 90, 90, 90) },
            { "--Version", Color.FromArgb(255, 190, 125, 0) },

            { "Drive", Color.FromArgb(255, 0, 90, 220) },
            { "Folder", Color.FromArgb(255, 0, 170, 220) },

            { "-GetDrives", Color.FromArgb(255, 175, 130, 0) },
            { "-Open", Color.FromArgb(255, 0, 170, 90) },
            { "-Delete", Color.FromArgb(255, 210, 40, 90) },
            { "-Move", Color.FromArgb(255, 210, 110, 0) },
            { "-Copy", Color.FromArgb(255, 30, 170, 30) },
            { "-Rename", Color.FromArgb(255, 145, 70, 210) },
            { "-Properties", Color.FromArgb(255, 190, 60, 160) },
            { "-Advanced", Color.FromArgb(255, 70, 90, 220) },
            { "-Print", Color.FromArgb(255, 120, 170, 0) },

            { "-True", Color.FromArgb(255, 170, 135, 0) },
            { "-False", Color.FromArgb(255, 170, 90, 90) }
        };

        private static readonly Dictionary<string, long> SizeUnits = new()
        {
            { "B", 1 },
            { "KB", 1024 },
            { "MB", 1_048_576 },
            { "GB", 1_073_741_824 },
            { "TB", 1_099_511_627_776 },
            { "PB", 1_125_899_906_842_624 },
        };

        public static double ReturnSize(long sizeInBytes, string unit)
        {
            SizeUnits.TryGetValue(unit, out long size);

            return Math.Round((double)sizeInBytes / size, 3);
        }

        public static string ReturnSizeUnit(long sizeInBytes, bool returnOnlySizeType = false)
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

        private static readonly Dictionary<string, BitmapImage> CachedImages = [];

        public static async Task<BitmapImage?> GetItemIcon(string? filePath)
        {
            bool isFile = false;

            if (string.IsNullOrWhiteSpace(filePath))
            {
                return null;
            }

            if (CachedImages.TryGetValue(filePath, out BitmapImage? icon))
            {
                return icon;
            }

            try
            {
                StorageItemThumbnail? thumbnail;

                if (Directory.Exists(filePath))
                {
                    isFile = false;
                    StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(filePath);
                    thumbnail = await folder.GetThumbnailAsync(
                        ThumbnailMode.SingleItem,
                        40,
                        ThumbnailOptions.UseCurrentScale);
                }
                else if (File.Exists(filePath))
                {
                    isFile = true;
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
                    CachedImages[filePath] = bitmapImage;
                    return bitmapImage;
                }
            }
            catch (UnauthorizedAccessException)
            {
                BitmapImage bitmapImage = new()
                {
                    UriSource = !isFile
                        ? new Uri("ms-appx:///Assets/Folder.png")
                        : new Uri("ms-appx:///Assets/File.png")
                };
                return bitmapImage;
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