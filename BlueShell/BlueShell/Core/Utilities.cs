using Microsoft.UI;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI;

namespace BlueShell.Core
{
    public static class Utilities
    {
        public static Dictionary<string, Color> DarkThemeKeywordColors { get; } = new()
        {
            { "Exit", Color.FromArgb(255, 255, 92, 125) }, // coral-pink (not pure error red)
            { "Clear", Color.FromArgb(255,   0, 206, 209) }, // cyan (keep; very distinct)
            { "ClearDisplay", Color.FromArgb(255, 155, 155, 155) }, // neutral gray
            { "--Version", Color.FromArgb(255, 255, 179,  71) }, // gold-orange (not warning amber)
            { "Drive", Color.FromArgb(255,  30, 144, 255) }, // bright blue (keep)
            { "-GetDrives", Color.FromArgb(255, 255, 245, 170) }, // pale warm cream (distinct from --Version)
            { "-Properties", Color.FromArgb(255, 205, 110, 255) }, // vivid lavender (more distinct from -Advanced)
            { "-Advanced", Color.FromArgb(255, 120, 120, 255) }, // periwinkle (shifted away from purple)
            { "-Open", Color.FromArgb(255,  64, 224, 208) },  // turquoise (not success green)
            { "-Print", Color.FromArgb(255,  80, 250, 150) }  // mint green
        };

        public static Dictionary<string, Color> LightThemeKeywordColors { get; } = new()
        {
            { "Exit", Color.FromArgb(255, 215,  45,  90) }, // deep coral (not pure red)
            { "Clear", Color.FromArgb(255,   0, 139, 139) }, // teal/cyan (keep)
            { "ClearDisplay", Color.FromArgb(255, 105, 105, 105) }, // gray (keep)
            { "--Version", Color.FromArgb(255, 176, 110,  20) }, // golden-brown (not warning amber)
            { "Drive", Color.FromArgb(255,  25,  55, 212) }, // deep blue (keep)
            { "-GetDrives", Color.FromArgb(255, 165, 140,  40) }, // olive-gold (distinct from --Version)
            { "-Properties", Color.FromArgb(255, 150,  60, 200) }, // purple (slightly adjusted for distinction)
            { "-Advanced", Color.FromArgb(255,  65, 105, 225) }, // royal blue (keep; distinct from Drive)
            { "-Open", Color.FromArgb(255,  20, 160, 170) },  // teal-green (not success green)
            { "-Print", Color.FromArgb(255,  20, 140,  70) }  // deep mint green
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

        public static string Date(DateTime dateTime)
        {
            return dateTime == DateTime.MinValue ? "N/A" : dateTime.ToString("yyyy-MM-dd HH:mm:ss");
        }


        public static async Task<BitmapImage?> GetItemIcon(string? filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return null;
            }

            try
            {
                StorageItemThumbnail? thumbnail = null;

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