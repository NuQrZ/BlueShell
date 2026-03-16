using BlueShell.Helpers;
using BlueShell.Services.Wrappers;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace BlueShell.Services.FileSystem
{
    public sealed class FileSystemService : IFileSystemService
    {
        public List<FileSystemItem> LoadFiles(string filePath, CancellationToken cancellationToken = default)
        {
            if (!Directory.Exists(filePath))
            {
                return [];
            }

            List<FileSystemItem> files = [];

            DirectoryInfo directory = new(filePath);

            foreach (FileInfo fileInfo in directory.EnumerateFiles("*"))
            {
                cancellationToken.ThrowIfCancellationRequested();

                string unit = Utilities.ReturnSizeUnit(fileInfo.Length, true);
                double size = Utilities.ReturnSize(fileInfo.Length, unit);

                FileSystemItem fileSystemItem = new()
                {
                    ItemName = fileInfo.Name,
                    ItemType = fileInfo.Extension.ToUpperInvariant() + " File",
                    ItemSizeType = unit,
                    ItemSize = size,
                    DirectoryInfo = null,
                    FileInfo = fileInfo,
                };

                files.Add(fileSystemItem);
            }

            return files;
        }

        public List<FileSystemItem> LoadFolders(string filePath, CancellationToken cancellationToken = default)
        {
            if (!Directory.Exists(filePath))
            {
                return [];
            }

            List<FileSystemItem> folders = [];

            DirectoryInfo directory = new(filePath);

            foreach (DirectoryInfo directoryInfo in directory.EnumerateDirectories("*"))
            {
                cancellationToken.ThrowIfCancellationRequested();

                FileSystemItem fileSystemItem = new()
                {
                    ItemName = directoryInfo.Name,
                    ItemType = "Folder",
                    ItemSizeType = string.Empty,
                    ItemSize = null,
                    DirectoryInfo = directoryInfo,
                    FileInfo = null
                };

                folders.Add(fileSystemItem);
            }

            return folders;
        }
    }
}