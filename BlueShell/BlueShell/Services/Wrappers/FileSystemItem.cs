using System.IO;

namespace BlueShell.Services.Wrappers
{
    public sealed class FileSystemItem
    {
        public string? ItemName { get; set; }
        public string? ItemType { get; set; }
        public string? ItemSizeType { get; set; }
        public long? ItemSize { get; set; }
        public DriveInfo? DriveInfo { get; set; }
        public DirectoryInfo? DirectoryInfo { get; set; } = null;
        public FileInfo? FileInfo { get; set; } = null;
    }
}
