using BlueShell.Model.Properties;
using BlueShell.Services.Wrappers;
using System.Collections.Generic;

namespace BlueShell.Services
{
    public interface IPrintService
    {
        string[] PrintDrives(List<DriveItem> drives);
        string[] PrintFolderContents(List<FileSystemItem> folders);
        string[] PrintGeneralProperties(PropertyItem propertyItem);
    }
}
