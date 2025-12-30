using System.IO;

namespace BlueShell.Services.Wrappers
{
    public sealed class DriveItem
    {
        public required DriveInfo DriveInfo { get; init; }
        public required string DisplayName { get; init; }
        public long TakenSpaceBytes { get; init; }
        public double TotalSize { get; init; }
    }
}
