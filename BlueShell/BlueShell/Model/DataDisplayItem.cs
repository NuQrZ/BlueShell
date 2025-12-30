using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;
using System.IO;

namespace BlueShell.Model
{
    public sealed class DataDisplayItem
    {
        public string? ItemName { get; init; }
        public string? ItemType { get; init; }
        public string? ItemSizeType { get; init; }
        public string? Color { get; set; }
        public BitmapImage? ItemIcon { get; set; }
        public int ImageSize { get; set; }
        public DriveInfo? DriveInfo { get; init; }
        public DirectoryInfo? DirectoryInfo { get; set; }
        public FileInfo? FileInfo { get; set; }
        public double TakenSpace { get; init; }
        public double TotalSize { get; set; }
        public long? ItemSize { get; set; }
        public bool IsTakenSpaceVisible { get; set; }
        public bool IsSizeVisible { get; set; }
        public Thickness TextPadding { get; set; }
        public Thickness ImageMargin { get; set; }
        public Thickness ProgressMargin { get; set; }
        public Thickness ItemTypeMargin { get; set; }
        public Thickness ItemSizeMargin { get; set; }
    }
}
