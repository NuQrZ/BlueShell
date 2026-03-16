using BlueShell.Terminal.Abstractions;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace BlueShell.Model
{
    public sealed partial class TabModel : INotifyPropertyChanged, IAddressBarNavigator
    {
        public ObservableCollection<AddressBarItem> AddressBarItems { get; set; } = [];
        public ObservableCollection<string> FilePathCollection { get; set; } = [];

        public IconSource? TabIcon
        {
            get;
            set
            {
                field = value;
                OnPropertyChanged();
            }
        }

        public string? TabHeader
        {
            get;
            set
            {
                field = value;
                OnPropertyChanged();
            }
        }

        public string? SelectedNavTag
        {
            get;
            set
            {
                field = value;
                OnPropertyChanged();
            }
        }

        public Frame? TabFrame { get; set; } = new();

        public int TabNumber { get; init; } = 1;

        public void ApplyNavSelection(string? itemTag)
        {
            if (string.IsNullOrWhiteSpace(itemTag))
            {
                return;
            }

            TabHeader = $"{itemTag} {TabNumber}";

            var iconName = ConvertTagIntoImageName(itemTag);
            TabIcon = new BitmapIconSource
            {
                UriSource = new Uri($"ms-appx:///Assets/{iconName}"),
                ShowAsMonochrome = false
            };
        }

        private static string ConvertTagIntoImageName(string itemTag) => itemTag switch
        {
            "Terminal" => "Terminal.ico",
            "Help" => "Help.ico",
            "SystemProcesses" => "Processes.ico",
            "WebPage" => "Web.ico",
            "SystemInfo" => "System Info.ico",
            "GraphicsCard" => "Graphics Card.ico",
            "Motherboard" => "Motherboard.ico",
            "NetworkInterface" => "Wifi.ico",
            "OperatingSystem" => "Windows 11.ico",
            "Processor" => "CPU.ico",
            _ => "Terminal.ico",
        };

        public bool IsMultipleTarget
        {
            get;
            set
            {
                field = value;
                OnPropertyChanged();
            }
        } = false;

        public string SearchLocation
        {
            get;
            set
            {
                field = value;
                OnPropertyChanged();
            }
        } = "Search System...";

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private static List<AddressBarItem> ReturnPath(string filePath)
        {
            List<AddressBarItem> result = [];

            bool pathExists = File.Exists(filePath) && Directory.Exists(filePath);

            if (filePath == "All" || !pathExists)
            {
                result.Add(new AddressBarItem()
                {
                    Text = filePath,
                    FontFamily = "Cascadia Code",
                    FontSize = 16
                });

                return result;
            }

            string[] pathSegments =
                filePath.Replace("\"", "").Trim().Split('\\', StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < pathSegments.Length; i++)
            {
                if (i == 0)
                {
                    string rootSegment = pathSegments[i] + '\\';
                    result.Add(new AddressBarItem()
                    {
                        Text = rootSegment,
                        FontFamily = "Cascadia Code",
                        FontSize = 16
                    });
                }
                else
                {
                    result.Add(new AddressBarItem()
                    {
                        Text = pathSegments[i],
                        FontFamily = "Cascadia Code",
                        FontSize = 16
                    });
                }
            }

            return result;
        }

        public void SetPath(string filePath, bool isMultiple)
        {
            IsMultipleTarget = isMultiple;
            List<AddressBarItem> items = ReturnPath(filePath);

            foreach (AddressBarItem addressBarItem in items)
            {
                AddressBarItems.Add(addressBarItem);
            }

            OnPropertyChanged(nameof(AddressBarItems));
        }

        public void AddFilePaths(List<string> filePaths)
        {
            FilePathCollection.Clear();

            FilePathCollection.Add("All");

            foreach (string filePath in filePaths)
            {
                FilePathCollection.Add(filePath);
            }
        }

        public void ClearPath()
        {
            AddressBarItems.Clear();
            OnPropertyChanged(nameof(AddressBarItems));
        }
    }
}