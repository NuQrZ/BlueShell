using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BlueShell.Model
{
    public sealed partial class TabModel : INotifyPropertyChanged
    {
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

        private Dictionary<string, object> State { get; } = new(StringComparer.Ordinal);
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

        public T GetOrCreateState<T>(string key, Func<T> factory) where T : class
        {
            if (State.TryGetValue(key, out object? value) && value is T tValue)
            {
                return tValue;
            }

            T created = factory();
            State[key] = created;
            return created;
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
    }
}
