using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System.IO;

namespace BlueShell.Model
{
    public sealed partial class DataDisplayItem : ObservableObject
    {
        [ObservableProperty] public partial string? ItemName { get; set; }
        [ObservableProperty] public partial string? ItemType { get; set; }
        [ObservableProperty] public partial string? ItemSizeType { get; set; }
        [ObservableProperty] public partial Brush? Color { get; set; }
        [ObservableProperty] public partial BitmapImage? ItemIcon { get; set; }
        [ObservableProperty] public partial int ImageSize { get; set; }
        public string? DriveFilePath { get; set; }
        public DirectoryInfo? DirectoryInfo { get; set; }
        public FileInfo? FileInfo { get; set; }
        public double TakenSpace { get; init; }
        public double TotalSize { get; set; }
        [ObservableProperty] public partial double? ItemSize { get; set; }
        [ObservableProperty] public partial bool IsTakenSpaceVisible { get; set; }
        [ObservableProperty] public partial bool IsSizeVisible { get; set; }
        [ObservableProperty] public partial Thickness TextPadding { get; set; }
        [ObservableProperty] public partial Thickness ImageMargin { get; set; }
        [ObservableProperty] public partial Thickness ProgressMargin { get; set; }
        [ObservableProperty] public partial Thickness ItemTypeMargin { get; set; }
    }
}