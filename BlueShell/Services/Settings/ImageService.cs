using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;

namespace BlueShell.Services.Settings
{
    public sealed class ImageService : IImageService
    {
        public bool IsImageBright(WriteableBitmap writeableBitmap)
        {
            using Stream buffer = writeableBitmap.PixelBuffer.AsStream();
            int pixelBufferSize = checked((int)buffer.Length);
            byte[] pixels = new byte[pixelBufferSize];
            buffer.ReadExactly(pixels, 0, pixels.Length);

            double brightnessSum = 0;
            int pixelCount = pixels.Length / 4;

            for (int i = 0; i < pixels.Length; i += 4)
            {
                byte blue = pixels[i];
                byte green = pixels[i + 1];
                byte red = pixels[i + 2];

                double brightness = (0.299 * red + 0.587 * green + 0.114 * blue) / 255;
                brightnessSum += brightness;
            }

            double averageBrightness = brightnessSum / pixelCount;

            return averageBrightness > 0.5;
        }

        public bool IsGradientBright(List<Color> colors)
        {
            if (!colors.Any())
            {
                return false;
            }

            double averageBrightness = colors.Average(color =>
                0.299 * color.R +
                0.587 * color.G +
                0.114 * color.B);

            return averageBrightness >= 128;
        }

        public string GetDesktopWallpaperPath()
        {
            using RegistryKey? registryKey = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", false);
            if (registryKey == null)
            {
                return "";
            }

            object? wallpaperValue = registryKey.GetValue("Wallpaper");
            return wallpaperValue?.ToString() ?? "";
        }

        public async Task<WriteableBitmap> LoadWriteableBitmap(StorageFile imageFile)
        {
            using IRandomAccessStreamWithContentType randomAccessStreamWithContentType =
                await RandomAccessStreamReference.CreateFromFile(imageFile).OpenReadAsync();

            BitmapDecoder bitmapDecoder = await BitmapDecoder.CreateAsync(randomAccessStreamWithContentType);
            int width = (int)bitmapDecoder.PixelWidth;
            int height = (int)bitmapDecoder.PixelHeight;

            WriteableBitmap writeableBitmap = new WriteableBitmap(width, height);

            randomAccessStreamWithContentType.Seek(0);
            await writeableBitmap.SetSourceAsync(randomAccessStreamWithContentType);

            return writeableBitmap;
        }
    }
}
