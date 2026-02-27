using BlueShell.Services.Wrappers;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Collections.Generic;

namespace BlueShell.Model.Properties
{
    public sealed class PropertyItem
    {
        public BitmapImage? BitmapIcon { get; set; }
        public string? ItemName { get; set; }
        public FileSystemItem? FileSystemItem { get; set; }
        public DriveItem? DriveItem { get; set; }
        public List<PropertyGroup>? PropertyGroups { get; set; }
    }
}
