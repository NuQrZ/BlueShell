using BlueShell.Core;
using BlueShell.Services.Wrappers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BlueShell.Services
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

            foreach (FileInfo fileInfo in directory.GetFiles("*", new EnumerationOptions()
            {
                IgnoreInaccessible = true
            }))
            {
                string fileName = fileInfo.Name;
                string fileType = fileInfo.Extension.ToUpper() + " File";
                string fileSizeType = Utilities.ReturnSizeType(fileInfo.Length);

                FileSystemItem fileSystemItem = new()
                {
                    ItemName = fileName,
                    ItemType = fileType,
                    ItemSizeType = fileSizeType,
                    ItemSize = fileInfo.Length,
                    DirectoryInfo = null,
                    FileInfo = fileInfo,
                };

                files.Add(fileSystemItem);
            }

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

            foreach (DirectoryInfo directoryInfo in directory.GetDirectories("*", new EnumerationOptions()
            {
                IgnoreInaccessible = true
            }))
            {
                string fileName = directoryInfo.Name;
                string fileSizeType = "";

                FileSystemItem fileSystemItem = new()
                {
                    ItemName = fileName,
                    ItemType = "Folder",
                    ItemSizeType = fileSizeType,
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
