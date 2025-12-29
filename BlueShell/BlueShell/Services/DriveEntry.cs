using System.IO;

namespace BlueShell.Services
{
    public sealed class DriveEntry
    {
        public required DriveInfo DriveInfo { get; init; }
        public required string DisplayName { get; init; }
        public long TakenSpaceBytes { get; init; }
        public double TotalSize { get; init; }
    }
}
