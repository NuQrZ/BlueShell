using BlueShell.Services.Wrappers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlueShell.Services.FileSystem
{
    public interface IDriveService
    {
        Task<string> GetDriveDisplayName(string driveFilePath);
        Task<List<DriveItem>> GetDrives();
        Dictionary<string, object> GetDriveProperties(string driveLetter);
        Dictionary<string, object> GetAdvancedDriveProperties(string driveLetter);
    }
}
