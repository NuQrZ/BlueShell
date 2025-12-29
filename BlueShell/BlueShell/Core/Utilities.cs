using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
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
            { "Exit",         Color.FromArgb(255, 255, 92, 125) }, // coral-pink (not pure error red)
            { "Clear",        Color.FromArgb(255,   0, 206, 209) }, // cyan (keep; very distinct)
            { "ClearDisplay", Color.FromArgb(255, 155, 155, 155) }, // neutral gray
            { "--Version",    Color.FromArgb(255, 255, 179,  71) }, // gold-orange (not warning amber)
            { "Drive",        Color.FromArgb(255,  30, 144, 255) }, // bright blue (keep)
            { "-GetDrives",   Color.FromArgb(255, 255, 245, 170) }, // pale warm cream (distinct from --Version)
            { "-Properties",  Color.FromArgb(255, 205, 110, 255) }, // vivid lavender (more distinct from -Advanced)
            { "-Advanced",    Color.FromArgb(255, 120, 120, 255) }, // periwinkle (shifted away from purple)
            { "-Open",        Color.FromArgb(255,  64, 224, 208) }  // turquoise (not success green)
        };

        public static Dictionary<string, Color> LightThemeKeywordColors { get; } = new()
        {
            { "Exit",         Color.FromArgb(255, 215,  45,  90) }, // deep coral (not pure red)
            { "Clear",        Color.FromArgb(255,   0, 139, 139) }, // teal/cyan (keep)
            { "ClearDisplay", Color.FromArgb(255, 105, 105, 105) }, // gray (keep)
            { "--Version",    Color.FromArgb(255, 176, 110,  20) }, // golden-brown (not warning amber)
            { "Drive",        Color.FromArgb(255,  25,  55, 212) }, // deep blue (keep)
            { "-GetDrives",   Color.FromArgb(255, 165, 140,  40) }, // olive-gold (distinct from --Version)
            { "-Properties",  Color.FromArgb(255, 150,  60, 200) }, // purple (slightly adjusted for distinction)
            { "-Advanced",    Color.FromArgb(255,  65, 105, 225) }, // royal blue (keep; distinct from Drive)
            { "-Open",        Color.FromArgb(255,  20, 160, 170) }  // teal-green (not success green)
        };

        public static async Task<BitmapImage?> GetItemIcon(string? filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))

                return null;

            try
            {
                StorageItemThumbnail? thumb = null;

                if (Directory.Exists(filePath))
                {
                    var folder = await StorageFolder.GetFolderFromPathAsync(filePath);
                    thumb = await folder.GetThumbnailAsync(
                        ThumbnailMode.SingleItem,
                        40,
                        ThumbnailOptions.UseCurrentScale);
                }
                else if (File.Exists(filePath))
                {
                    var file = await StorageFile.GetFileFromPathAsync(filePath);
                    thumb = await file.GetThumbnailAsync(
                        ThumbnailMode.SingleItem,
                        40,
                        ThumbnailOptions.UseCurrentScale);
                }
                else
                {
                    return null;
                }

                if (thumb is null || thumb.Size == 0)
                {
                    thumb?.Dispose();
                    return null;
                }

                using (thumb)
                {
                    var bmp = new BitmapImage();
                    await bmp.SetSourceAsync(thumb);
                    return bmp;
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