using BlueShell.Services.Wrappers;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace BlueShell.Services.FileSystem
{
    public interface IDriveService
    {
        Task<string> GetDriveDisplayName(string driveFilePath);
        Task<List<DriveItem>> GetDrives();
        DriveInfo? GetDrive(string filePath);
        Dictionary<string, object> GetDriveProperties(string driveLetter);
        Dictionary<string, object> GetAdvancedDriveProperties(string driveLetter);
    }
}
