using BlueShell.Services.Wrappers;
using System.Collections.Generic;

namespace BlueShell.Services.FileSystem
{
    public interface IFileSystemService
    {
        List<FileSystemItem> LoadFolders(string filePath);
        List<FileSystemItem> LoadFiles(string filePath);
    }
}
