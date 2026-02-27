using BlueShell.Helpers;
using BlueShell.Model.Properties;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace BlueShell.Services.Properties
{
    public static class DrivePropertiesBuilder
    {
        public static async Task<PropertyItem> BuildDrivePropertyItem(
            string displayName,
            string driveFilePath,
            Dictionary<string, object> properties,
            bool includeAdvanced)
        {
            BitmapImage? icon = await Utilities.GetItemIcon(driveFilePath);

            Dictionary<string, List<PropertyRow>> groups = new(StringComparer.OrdinalIgnoreCase);

            AddGeneral(groups, properties);
            AddCapacity(groups, properties);
            AddHardware(groups, properties);

            if (includeAdvanced)
            {
                AddAdvanced(groups, properties);
            }

            return CreatePropertyItem(displayName, icon, groups);
        }

        private static void AddGeneral(Dictionary<string, List<PropertyRow>> groups, Dictionary<string, object> properties)
        {
            AddIfNonEmpty(groups, "General", "Label",
                GetString(properties, "Logical.VolumeName") ?? GetString(properties, "Volume.Label"));

            AddIfNonEmpty(groups, "General", "Letter",
                GetString(properties, "Logical.DeviceID") ?? GetString(properties, "Volume.DriveLetter"));

            AddIfNonEmpty(groups, "General", "File system",
                GetString(properties, "Logical.FileSystem") ?? GetString(properties, "Volume.FileSystem"));

            AddIfNonEmpty(groups, "General", "Drive type", GetString(properties, "Logical.DriveType"));
            AddIfNonEmpty(groups, "General", "Status", GetString(properties, "Logical.Status"));
            AddIfNonEmpty(groups, "General", "Description", GetString(properties, "Logical.Description"));

            AddIfNonEmpty(groups, "General", "Volume serial", GetString(properties, "Logical.VolumeSerialNumber"));
            AddIfNonEmpty(groups, "General", "Boot volume", GetString(properties, "Volume.BootVolume"));
            AddIfNonEmpty(groups, "General", "System volume", GetString(properties, "Volume.SystemVolume"));

            AddIfNonEmpty(groups, "General", "Disk health", GetString(properties, "MSFT_Disk.HealthStatus"));
            AddIfNonEmpty(groups, "General", "Disk operational", GetString(properties, "MSFT_Disk.OperationalStatus"));
        }

        private static void AddCapacity(
            Dictionary<string, List<PropertyRow>> groups,
            Dictionary<string, object> properties)
        {
            ulong? size = GetBytes(properties, "Logical.Size") ?? GetBytes(properties, "Volume.Capacity");
            ulong? free = GetBytes(properties, "Logical.FreeSpace") ?? GetBytes(properties, "Volume.FreeSpace");

            if (size is not null)
            {
                Add(groups, "Capacity", "Total size", Utilities.ReturnSize((long)size.Value));
            }

            if (free is not null)
            {
                Add(groups, "Capacity", "Free space", Utilities.ReturnSize((long)free.Value));
            }

            if (size is null || free is null)
            {
                return;
            }

            ulong used = size.Value - free.Value;
            Add(groups, "Capacity", "Used space", Utilities.ReturnSize((long)used));

            double percent = size.Value > 0 ? (double)used / size.Value * 100.0 : 0.0;
            Add(groups, "Capacity", "Used (%)", percent.ToString("0.##", CultureInfo.InvariantCulture) + "%");
        }

        private static void AddHardware(
            Dictionary<string, List<PropertyRow>> groups,
            Dictionary<string, object> properties)
        {
            AddIfNonEmpty(groups, "Hardware", "Model", GetString(properties, "DiskDrive.Model") ?? GetString(properties, "MSFT_Disk.Model") ?? GetString(properties, "MSFT_Disk.FriendlyName"));
            AddIfNonEmpty(groups, "Hardware", "Manufacturer", GetString(properties, "DiskDrive.Manufacturer") ?? GetString(properties, "MSFT_Disk.Manufacturer"));
            AddIfNonEmpty(groups, "Hardware", "Interface", GetString(properties, "DiskDrive.InterfaceType"));
            AddIfNonEmpty(groups, "Hardware", "Media type", GetString(properties, "DiskDrive.MediaType"));
            AddIfNonEmpty(groups, "Hardware", "Serial", GetString(properties, "DiskDrive.SerialNumber") ?? GetString(properties, "PhysicalMedia.SerialNumber") ?? GetString(properties, "MSFT_Disk.SerialNumber"));
            AddIfNonEmpty(groups, "Hardware", "PNP Device ID", GetString(properties, "DiskDrive.PNPDeviceID"));
            AddIfNonEmpty(groups, "Hardware", "Firmware", GetString(properties, "DiskDrive.FirmwareRevision") ?? GetString(properties, "MSFT_Disk.FirmwareVersion"));
            AddIfNonEmpty(groups, "Hardware", "Bus type", GetString(properties, "MSFT_Disk.BusType"));
            AddIfNonEmpty(groups, "Hardware", "Partition style", GetString(properties, "MSFT_Disk.PartitionStyle"));
            AddIfNonEmpty(groups, "Hardware", "Health Status", GetString(properties, "MSFT_Disk.HealthStatus"));
            AddIfNonEmpty(groups, "Hardware", "Operational Status", GetString(properties, "MSFT_Disk.OperationalStatus"));
        }

        private static void AddAdvanced(
            Dictionary<string, List<PropertyRow>> groups,
            Dictionary<string, object> properties)
        {
            foreach (KeyValuePair<string, object> property in properties
                         .OrderBy(property => property.Key, StringComparer.OrdinalIgnoreCase))
            {
                (string groupName, string label) = SplitKey(property.Key);

                Add(groups, groupName, label, FormatValue(property.Value));
            }
        }

        private static (string Group, string Label) SplitKey(string key)
        {
            int dot = key.LastIndexOf('.');

            if (dot <= 0 || dot == key.Length - 1)
            {
                return ("Other", key);
            }

            string groupName = key[..dot];
            string label = key[(dot + 1)..];

            return (groupName, label);
        }

        private static PropertyItem CreatePropertyItem(
            string displayName,
            BitmapImage? icon,
            Dictionary<string, List<PropertyRow>> groups)
        {
            List<PropertyGroup> propertyGroups = [.. groups
                .Where(group => group.Value.Count > 0)
                .Select(group => new PropertyGroup
                {
                    GroupName = group.Key,
                    PropertyRows = group.Value
                })];

            return new PropertyItem
            {
                BitmapIcon = icon,
                ItemName = displayName,
                DriveItem = null,
                FileSystemItem = null,
                PropertyGroups = propertyGroups
            };
        }

        private static void Add(
            Dictionary<string, List<PropertyRow>> groups,
            string group,
            string label = "",
            string value = "")
        {
            if (!groups.TryGetValue(group, out var list))
            {
                list = [];
                groups[group] = list;
            }

            list.Add(new PropertyRow { Label = label, Text = value });
        }

        private static void AddIfNonEmpty(
            Dictionary<string, List<PropertyRow>> groups,
            string group,
            string label,
            string? value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                Add(groups, group, label, value);
            }
        }

        private static string? GetString(Dictionary<string, object> props, string key)
        {
            if (!props.TryGetValue(key, out var value))
            {
                return null;
            }

            if (value is ushort[] u16)
            {
                return "{" + string.Join(", ", u16) + "}";
            }

            return Convert.ToString(value, CultureInfo.InvariantCulture);
        }

        private static ulong? GetBytes(Dictionary<string, object> props, string key)
        {
            if (!props.TryGetValue(key, out var v) || v is null)
            {
                return null;
            }

            try
            {
                return v switch
                {
                    ulong u => u,
                    long l when l >= 0 => (ulong)l,
                    int i when i >= 0 => (ulong)i,
                    string s when ulong.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed) => parsed,
                    _ => Convert.ToUInt64(v, CultureInfo.InvariantCulture)
                };
            }
            catch
            {
                return null;
            }
        }

        private static string FormatValue(object value)
        {
            if (value is Array arr)
            {
                return string.Join(", ", arr.Cast<object?>().Select(x => x?.ToString() ?? ""));
            }

            return Convert.ToString(value, CultureInfo.InvariantCulture) ?? "";
        }
    }
}