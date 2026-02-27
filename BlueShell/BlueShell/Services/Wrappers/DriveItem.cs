namespace BlueShell.Services.Wrappers
{
    public sealed class DriveItem
    {
        public string? RootPath { get; init; }
        public string? VolumeLabel { get; init; }

        public string? DriveType { get; init; }
        public string? DriveFormat { get; init; }

        public long TotalBytes { get; init; }
        public long FreeBytes { get; init; }
        public long UsedBytes { get; init; }

        public double UsedPrecent { get; init; }

        public bool IsReady { get; init; }
        public bool IsSystemDrive { get; init; }
    }
}
