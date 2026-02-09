using System.Collections.Generic;
using BlueShell.Services.Wrappers;

namespace BlueShell.Services
{
    public interface IPrintService
    {
        string[] PrintDrives(List<DriveItem> drives);
        string[] PrintDriveProperties(List<DriveItem> drives);
        string[] PrintDriveAdvancedProperties(List<DriveItem> drives);
        string[] PrintFolderContents(List<FileSystemItem> folders);
        string[] PrintFolderProperties(List<FileSystemItem> folders);
        string[] PrintFileProperties(List<FileSystemItem> files);
    }
}