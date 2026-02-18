using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;

namespace BlueShell.Helpers
{
    public static class Utilities
    {
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
