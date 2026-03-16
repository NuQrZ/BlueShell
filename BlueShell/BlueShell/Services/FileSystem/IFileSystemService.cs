using BlueShell.Services.Wrappers;
using System.Collections.Generic;
using System.Threading;

namespace BlueShell.Services.FileSystem
{
    public interface IFileSystemService
    {
        List<FileSystemItem> LoadFolders(string filePath, CancellationToken cancellationToken);
        List<FileSystemItem> LoadFiles(string filePath, CancellationToken cancellationToken);
    }
}