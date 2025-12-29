using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace BlueShell.Services
{
    public interface IDriveService
    {
        Task<List<DriveEntry>> GetDrives();
        DriveInfo? GetDrive(string filePath);
    }
}
