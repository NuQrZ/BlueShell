using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace BlueShell.Helpers
{
    public static class IconUtilities
    {
        [ComImport]
        [Guid("bcc18b79-ba16-442f-80c4-8a59c30c463b")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IShellItemImageFactory
        {
            void GetImage(NativeSize size, Siigbf flags, out IntPtr phbm);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct NativeSize(int width, int height)
        {
            public int Width = width;
            public int Height = height;
        }

        [Flags]
        private enum Siigbf
        {
            ResizeToFit = 0x00000000,
            IconOnly = 0x00000004,
            ThumbnailOnly = 0x00000008,
            InCacheOnly = 0x00000010,
        }

        [DllImport("shell32.dll", CharSet = CharSet.Unicode, PreserveSig = true)]
        private static extern int SHCreateItemFromParsingName(
            [MarshalAs(UnmanagedType.LPWStr)] string pszPath,
            IntPtr pbc,
            ref Guid riid,
            [MarshalAs(UnmanagedType.Interface)] out IShellItemImageFactory ppv);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

        public static Task<BitmapImage?> GetCachedShellImageAsync(string? filePath, int width, int height)
        {
            return GetShellImageAsync(filePath, width, height, Siigbf.InCacheOnly | Siigbf.ResizeToFit);
        }

        public static Task<BitmapImage?> GetShellThumbnailAsync(string? filePath, int width, int height)
        {
            return GetShellImageAsync(filePath, width, height, Siigbf.ResizeToFit);
        }

        public static Task<BitmapImage?> GetShellThumbnailOnlyAsync(string? filePath, int width, int height)
        {
            return GetShellImageAsync(filePath, width, height, Siigbf.ThumbnailOnly | Siigbf.ResizeToFit);
        }

        public static Task<BitmapImage?> GetShellIconOnlyAsync(string? filePath, int width, int height)
        {
            return GetShellImageAsync(filePath, width, height, Siigbf.IconOnly | Siigbf.ResizeToFit);
        }

        private static async Task<BitmapImage?> GetShellImageAsync(string? filePath, int width, int height,
            Siigbf flags)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return null;
            }

            byte[]? imageBytes = await Task.Run(() => GetShellImageBytes(filePath, width, height, flags));

            if (imageBytes is null || imageBytes.Length == 0)
            {
                return null;
            }

            using MemoryStream memoryStream = new(imageBytes);
            using IRandomAccessStream randomAccessStream = memoryStream.AsRandomAccessStream();

            BitmapImage image = new();
            await image.SetSourceAsync(randomAccessStream);
            return image;
        }

        private static byte[]? GetShellImageBytes(string filePath, int width, int height, Siigbf flags)
        {
            IShellItemImageFactory? factory = null;
            IntPtr hBitmap = IntPtr.Zero;

            try
            {
                factory = GetShellItemImageFactory(filePath);
                if (factory is null)
                {
                    return null;
                }

                factory.GetImage(new NativeSize(width, height), flags, out hBitmap);

                if (hBitmap == IntPtr.Zero)
                {
                    return null;
                }

                using Bitmap bitmap = Image.FromHbitmap(hBitmap);
                using MemoryStream stream = new();
                bitmap.MakeTransparent();
                bitmap.Save(stream, ImageFormat.Png);
                return stream.ToArray();
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                if (hBitmap != IntPtr.Zero)
                {
                    DeleteObject(hBitmap);
                }

                if (factory != null)
                {
                    Marshal.ReleaseComObject(factory);
                }
            }
        }

        private static IShellItemImageFactory? GetShellItemImageFactory(string filePath)
        {
            Guid iid = new("bcc18b79-ba16-442f-80c4-8a59c30c463b");

            int hResult =
                SHCreateItemFromParsingName(filePath, IntPtr.Zero, ref iid, out IShellItemImageFactory factory);
            return hResult == 0 ? factory : null;
        }
    }
}