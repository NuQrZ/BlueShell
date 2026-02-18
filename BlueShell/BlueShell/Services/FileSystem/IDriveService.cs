using BlueShell.Services.Wrappers;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace BlueShell.Services.FileSystem
{
    public interface IDriveService
    {
        Task<List<DriveItem>> GetDrives();
        DriveInfo? GetDrive(string filePath);
    }
}
