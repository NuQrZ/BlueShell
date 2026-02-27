using BlueShell.Helpers;
using BlueShell.Model.Properties;
using BlueShell.Services.Wrappers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace BlueShell.Services
{
    public sealed class PrintService : IPrintService
    {
        private string[] PrintTable(
            List<string> headers,
            List<List<string>> rows)
        {
            StringBuilder stringBuilder = new();

            const string vertical = "|";
            const string separator = "   ";

            int columnCount = headers.Count;

            int[] widths = new int[columnCount];

            for (int i = 0; i < columnCount; i++)
            {
                int headerWidth = headers[i].Length;
                int contentWidth = rows
                    .Select(r => r[i].Length)
                    .DefaultIfEmpty(0)
                    .Max();

                widths[i] = Math.Max(headerWidth, contentWidth);
            }

            int totalWidth =
                widths.Sum() +
                separator.Length * (columnCount - 1) +
                vertical.Length * 2;

            stringBuilder.AppendLine("+" + new string('-', totalWidth - 2) + "+");

            stringBuilder.Append(vertical);
            for (int i = 0; i < columnCount; i++)
            {
                stringBuilder.Append(headers[i].PadRight(widths[i]));
                if (i < columnCount - 1)
                {
                    stringBuilder.Append(separator);
                }
            }
            stringBuilder.AppendLine(vertical);

            stringBuilder.AppendLine("+" + new string('-', totalWidth - 2) + "+");

            foreach (var row in rows)
            {
                stringBuilder.Append(vertical);
                for (int i = 0; i < columnCount; i++)
                {
                    stringBuilder.Append(row[i].PadRight(widths[i]));
                    if (i < columnCount - 1)
                    {
                        stringBuilder.Append(separator);
                    }
                }
                stringBuilder.AppendLine(vertical);
            }

            stringBuilder.AppendLine("+" + new string('-', totalWidth - 2) + "+");

            return stringBuilder.ToString()
                .Replace("\r\n", "\n")
                .TrimEnd('\n')
                .Split('\n');
        }

        public string[] PrintDrives(List<DriveItem> drives)
        {
            List<string> headers =
            [
                "Display Name",
                "Drive Root",
                "Drive Type",
                "Drive Format",
                "Total Size",
                "Used Space",
                "Free Space",
                "Used Percent"
            ];

            List<List<string>> rows = drives.Select(d => new List<string>
            {
                d.VolumeLabel ?? "",
                d.RootPath ?? "",
                d.DriveType ?? "",
                d.DriveFormat ?? "",
                Utilities.ReturnSize(d.TotalBytes),
                Utilities.ReturnSize(d.UsedBytes),
                Utilities.ReturnSize(d.FreeBytes),
                Math.Round(d.UsedPrecent).ToString(CultureInfo.InvariantCulture) + " %"
            }).ToList();

            return PrintTable(headers, rows);
        }

        public string[] PrintFolderContents(List<FileSystemItem>? folders)
        {
            List<string> headers =
            [
                "Item Name",
                "Item Size",
                "Item Type"
            ];

            List<List<string>> rows = [.. (folders ?? [])
                .Select(folder => new List<string>
                {
                    folder.ItemName ?? "",
                    (folder.ItemSize + " " + (folder.ItemSizeType ?? "")).Trim(),
                    folder.ItemType ?? ""
                })
            ];

            return PrintTable(headers, rows);
        }

        private string[] PrintPropertyRows(List<PropertyRow>? propertyRows)
        {
            List<string> headers =
            [
                "Property",
                "Value"
            ];

            List<List<string>> rows =
            [
                .. (propertyRows ?? [])
                .Select(row => new List<string>
                {
                    row.Label ?? "",
                    row.Text ?? ""
                })
            ];

            return PrintTable(headers, rows);
        }

        public string[] PrintGeneralProperties(PropertyItem propertyItem)
        {
            StringBuilder stringBuilder = new();

            stringBuilder.AppendLine($"== {propertyItem.ItemName} ==");
            stringBuilder.AppendLine();

            foreach (PropertyGroup propertyGroup in propertyItem.PropertyGroups ?? [])
            {
                stringBuilder.AppendLine($"-- {propertyGroup.GroupName} --");

                string[] lines = PrintPropertyRows(propertyGroup.PropertyRows ?? []);
                foreach (string line in lines)
                {
                    stringBuilder.AppendLine(line);
                }

                stringBuilder.AppendLine();
            }

            return stringBuilder.ToString()
                .Replace("\r\n", "\n")
                .TrimEnd('\n')
                .Split('\n');
        }
    }
}
