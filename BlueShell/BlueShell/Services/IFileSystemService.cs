using BlueShell.Services.Wrappers;
using System.Collections.Generic;

namespace BlueShell.Services
{
    public interface IFileSystemService
    {
        public List<FileSystemItem> LoadFiles(string filePath);
        public List<FileSystemItem> LoadFolders(string filePath);
    }
}
