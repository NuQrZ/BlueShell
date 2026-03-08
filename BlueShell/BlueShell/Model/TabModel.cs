using BlueShell.Terminal.Abstractions;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BlueShell.Model
{
    public sealed partial class TabModel : INotifyPropertyChanged, IAddressBarNavigator
    {
        public ObservableCollection<AddressBarItem> AddressBarItems { get; set; } = [];
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

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void SetPath(string path, bool isMultiple)
        {
            if (isMultiple)
            {
                AddressBarItems.Add(new AddressBarItem()
                {
                    Text = path,
                    FontFamily = "Cascadia Code",
                    FontSize = 16
                });
                return;
            }

            string[] pathSegments = path.Replace("\"", "").Trim().Split('\\', StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < pathSegments.Length; i++)
            {
                if (i == 0)
                {
                    string rootSegment = pathSegments[i] + '\\';
                    AddressBarItems.Add(new AddressBarItem()
                    {
                        Text = rootSegment,
                        FontFamily = "Cascadia Code",
                        FontSize = 16
                    });
                }
                else
                {
                    AddressBarItems.Add(new AddressBarItem()
                    {
                        Text = pathSegments[i],
                        FontFamily = "Cascadia Code",
                        FontSize = 16
                    });
                }
            }

            OnPropertyChanged(nameof(AddressBarItems));
        }

        public void ClearPath()
        {
            AddressBarItems.Clear();
            OnPropertyChanged(nameof(AddressBarItems));
        }
    }
}
