using BlueShell.Helpers;
using BlueShell.Services.Wrappers;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BlueShell.Services.FileSystem
{
    public sealed class FileSystemService : IFileSystemService
    {
        public List<FileSystemItem> LoadFiles(string filePath)
        {
            if (!Directory.Exists(filePath))
            {
                return [];
            }

            List<FileSystemItem> files = [];

            DirectoryInfo directory = new(filePath);

            files.AddRange(from fileInfo in directory.EnumerateFiles("*")
                           let fileType = fileInfo.Extension.ToUpper()
                           let sizeUnit = Utilities.ReturnSizeUnit(fileInfo.Length, true)
                           let itemSize = Utilities.ReturnSize(fileInfo.Length, sizeUnit)
                           select new FileSystemItem()
                           {
                               ItemName = fileInfo.Name,
                               DirectoryInfo = null,
                               DriveInfo = null,
                               FileInfo = fileInfo,
                               ItemSize = itemSize,
                               ItemSizeType = sizeUnit,
                               ItemType = fileType
                           });

            return files;
        }

        public List<FileSystemItem> LoadFolders(string filePath)
        {
            if (!Directory.Exists(filePath))
            {
                return [];
            }

            List<FileSystemItem> folders = [];

            DirectoryInfo directory = new(filePath);

            folders.AddRange(from directoryInfo in directory.EnumerateDirectories("*")
                             select new FileSystemItem()
                             {
                                 ItemName = directoryInfo.FullName,
                                 DirectoryInfo = directoryInfo,
                                 DriveInfo = null,
                                 FileInfo = null,
                                 ItemSize = null,
                                 ItemSizeType = null,
                                 ItemType = "Folder"
                             });

            return folders;
        }
    }
}