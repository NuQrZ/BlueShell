using Microsoft.UI.Xaml.Controls;
using System;

namespace BlueShell.Services
{
    public interface INavigationService
    {
        void SetFrame(Frame frame);
        bool Navigate(Type pageType, object? parameter = null);
        bool CanGoBack { get; }
        void GoBack();
    }
}
