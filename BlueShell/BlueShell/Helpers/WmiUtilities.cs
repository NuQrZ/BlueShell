using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;

namespace BlueShell.Helpers
{
    public static class WmiUtilities
    {
        private static string EscapeWmi(string? value)
        {
            return (value ?? "")
                .Replace("\\", @"\\")
                .Replace("'", "''");
        }

        private static string NormalizeDriveLetter(string? driveLetter)
        {
            driveLetter = (driveLetter ?? "").Trim();
            if (driveLetter.EndsWith('\\'))
            {
                driveLetter = driveLetter.TrimEnd('\\');
            }

            return driveLetter;
        }

        private static ManagementBaseObject? FirstOrDefaultWmi(string scope, string query)
        {
            using ManagementObjectSearcher objectSearcher = new(scope, query);
            using ManagementObjectCollection resultsCollection = objectSearcher.Get();
            return resultsCollection.Cast<ManagementBaseObject>().FirstOrDefault();
        }

        private static ManagementBaseObject? FirstAssociator(string scope, string objectClass, string associatedClass)
        {
            using ManagementObjectSearcher objectSearcher = new(
                scope,
                $"ASSOCIATORS OF {{{objectClass}}} WHERE AssocClass={associatedClass}");
            using ManagementObjectCollection results = objectSearcher.Get();
            return results.Cast<ManagementBaseObject>().FirstOrDefault();
        }

        private static void AddAllProperties(Dictionary<string, object> target, ManagementBaseObject source,
            string prefix)
        {
            foreach (PropertyData propertyData in source.Properties)
            {
                if (propertyData?.Name is null)
                {
                    continue;
                }

                if (propertyData.Value is null)
                {
                    continue;
                }

                target[$"{prefix}.{propertyData.Name}"] = propertyData.Value;
            }
        }

        private static void TryAddStorageProperties(Dictionary<string, object> target, string driveLetter)
        {
            try
            {
                driveLetter = NormalizeDriveLetter(driveLetter); // "C:"
                string letterOnly = driveLetter.TrimEnd(':');    // "C"

                if (string.IsNullOrWhiteSpace(letterOnly))
                    return;

                string scope = @"\\.\root\Microsoft\Windows\Storage";

                // 1) Nađi MSFT_Partition koja predstavlja baš taj drive (C)
                ManagementBaseObject? cPartition = FirstOrDefaultWmi(
                    scope,
                    $"SELECT * FROM MSFT_Partition WHERE DriveLetter='{EscapeWmi(letterOnly)}'");

                if (cPartition == null)
                    return;

                AddAllProperties(target, cPartition, "MSFT_Partition[C]");

                // 2) Iz nje uzmi DiskNumber pa dovuci MSFT_Disk
                int diskNumber = Convert.ToInt32(cPartition["DiskNumber"]);
                ManagementBaseObject? msftDisk = FirstOrDefaultWmi(
                    scope,
                    $"SELECT * FROM MSFT_Disk WHERE Number={diskNumber}");

                if (msftDisk != null)
                    AddAllProperties(target, msftDisk, "MSFT_Disk");

                // 3) Sve particije tog diska (indeksiraj!)
                List<ManagementBaseObject> partitions = new();
                using (var s = new ManagementObjectSearcher(scope, $"SELECT * FROM MSFT_Partition WHERE DiskNumber={diskNumber}"))
                using (var r = s.Get())
                    partitions.AddRange(r.Cast<ManagementBaseObject>());

                for (int i = 0; i < partitions.Count; i++)
                    AddAllProperties(target, partitions[i], $"MSFT_Partition[{i + 1}]");

                // 4) Volume-i koji pripadaju tom disku:
                //    - uzmi GUID-eve iz AccessPaths (\\?\Volume{GUID}\)
                HashSet<string> volumeGuids = new(StringComparer.OrdinalIgnoreCase);

                foreach (var p in partitions)
                {
                    if (p["AccessPaths"] is string[] paths)
                    {
                        foreach (string path in paths)
                        {
                            // tipično: \\?\Volume{GUID}\
                            int a = path.IndexOf("Volume{", StringComparison.OrdinalIgnoreCase);
                            if (a < 0) continue;
                            int b = path.IndexOf("}", a, StringComparison.OrdinalIgnoreCase);
                            if (b < 0) continue;

                            string guid = path.Substring(a, b - a + 1); // "Volume{...}"
                            volumeGuids.Add(guid);
                        }
                    }
                }

                // 5) Enumeriši MSFT_Volume i zadrži samo one čiji UniqueId/Path sadrži neki od GUID-eva
                List<ManagementBaseObject> volumes = new();
                using (var s = new ManagementObjectSearcher(scope, "SELECT * FROM MSFT_Volume"))
                using (var r = s.Get())
                {
                    foreach (var v in r.Cast<ManagementBaseObject>())
                    {
                        string uniqueId = v["UniqueId"]?.ToString() ?? "";
                        string path = v["Path"]?.ToString() ?? "";

                        bool match = volumeGuids.Any(g => uniqueId.Contains(g, StringComparison.OrdinalIgnoreCase)
                                                       || path.Contains(g, StringComparison.OrdinalIgnoreCase));

                        if (match)
                            volumes.Add(v);
                    }
                }

                for (int i = 0; i < volumes.Count; i++)
                    AddAllProperties(target, volumes[i], $"MSFT_Volume[{i + 1}]");
            }
            catch
            {
                // swallow
            }
        }

        public static readonly HashSet<string> GeneralKeys = new(StringComparer.OrdinalIgnoreCase)
        {
            "Logical.DeviceID",
            "Logical.VolumeName",
            "Logical.FileSystem",
            "Logical.DriveType",
            "Logical.Status",
            "Logical.Size",
            "Logical.FreeSpace",
            "Logical.VolumeSerialNumber",
            "Logical.Description",

            "Volume.Label",
            "Volume.FileSystem",
            "Volume.Capacity",
            "Volume.FreeSpace",
            "Volume.DriveLetter",
            "Volume.SerialNumber",
            "Volume.BootVolume",
            "Volume.SystemVolume",
            "Volume.DirtyBitSet",
            "Volume.Automount",

            "DiskDrive.Model",
            "DiskDrive.Manufacturer",
            "DiskDrive.InterfaceType",
            "DiskDrive.MediaType",
            "DiskDrive.SerialNumber",
            "DiskDrive.PNPDeviceID",
            "DiskDrive.FirmwareRevision",
            "DiskDrive.BytesPerSector",
            "DiskDrive.Size",

            "Partition.DeviceID",
            "Partition.Type",
            "Partition.Size",
            "Partition.StartingOffset",
            "Partition.BootPartition",
            "Partition.PrimaryPartition",

            "PhysicalMedia.SerialNumber",
            "PhysicalMedia.Tag",

            "MSFT_Disk.Number",
            "MSFT_Disk.FriendlyName",
            "MSFT_Disk.Manufacturer",
            "MSFT_Disk.Model",
            "MSFT_Disk.SerialNumber",
            "MSFT_Disk.FirmwareVersion",
            "MSFT_Disk.BusType",
            "MSFT_Disk.PartitionStyle",
            "MSFT_Disk.HealthStatus",
            "MSFT_Disk.OperationalStatus",
            "MSFT_Disk.Size",
        };

        public static Dictionary<string, Dictionary<string, object>> DriveProperties { get; } = [];

        public static void GetAllDriveProperties(string driveLetter)
        {
            Dictionary<string, object> dictionary = [];

            driveLetter = NormalizeDriveLetter(driveLetter);
            if (string.IsNullOrWhiteSpace(driveLetter))
            {
                return;
            }

            ManagementBaseObject? logicalDrive = FirstOrDefaultWmi(
                @"\\.\root\cimv2",
                $"SELECT * FROM Win32_LogicalDisk WHERE DeviceID='{EscapeWmi(driveLetter)}'");

            if (logicalDrive != null)
            {
                AddAllProperties(dictionary, logicalDrive, "Logical");
            }

            ManagementBaseObject? volumeDrive = FirstOrDefaultWmi(
                @"\\.\root\cimv2",
                $"SELECT * FROM Win32_Volume WHERE DriveLetter='{EscapeWmi(driveLetter)}'");

            if (volumeDrive != null)
            {
                AddAllProperties(dictionary, volumeDrive, "Volume");
            }

            ManagementBaseObject? partition = FirstAssociator(
                @"\\.\root\cimv2",
                $"Win32_LogicalDisk.DeviceID='{EscapeWmi(driveLetter)}'",
                "Win32_LogicalDiskToPartition");

            if (partition != null)
            {
                AddAllProperties(dictionary, partition, "Partition");
            }

            ManagementBaseObject? diskDrive = null;
            if (partition != null)
            {
                string partitionId = partition["DeviceID"]?.ToString() ?? "";
                if (!string.IsNullOrWhiteSpace(partitionId))
                {
                    diskDrive = FirstAssociator(
                        @"\\.\root\cimv2",
                        $"Win32_DiskPartition.DeviceID='{EscapeWmi(partitionId)}'",
                        "Win32_DiskDriveToDiskPartition");

                    if (diskDrive != null)
                    {
                        AddAllProperties(dictionary, diskDrive, "DiskDrive");
                    }
                }
            }

            if (diskDrive != null)
            {
                string diskDeviceId = diskDrive["DeviceID"]?.ToString() ?? "";
                if (!string.IsNullOrWhiteSpace(diskDeviceId))
                {
                    ManagementBaseObject? physicalMedia = FirstOrDefaultWmi(
                        scope: @"\\.\root\cimv2",
                        query: $"SELECT * FROM Win32_PhysicalMedia WHERE Tag='{EscapeWmi(diskDeviceId)}'");

                    if (physicalMedia != null)
                    {
                        AddAllProperties(dictionary, physicalMedia, "PhysicalMedia");
                    }
                }
            }

            TryAddStorageProperties(dictionary, driveLetter);

            DriveProperties[driveLetter] = dictionary;
        }
    }
}
