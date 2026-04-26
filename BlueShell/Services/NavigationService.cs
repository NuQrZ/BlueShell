using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using System;

namespace BlueShell.Services
{
    public sealed class NavigationService : INavigationService
    {
        private Frame? _frame;
        public bool CanGoBack => _frame?.CanGoBack ?? false;

        public void GoBack()
        {
            if (CanGoBack)
            {
                _frame!.GoBack();
            }
        }

        public bool Navigate(Type pageType, object? parameter = null)
        {
            if (_frame is null)
            {
                return false;
            }

            return _frame.Navigate(pageType, parameter, new SlideNavigationTransitionInfo()
            {
                Effect = SlideNavigationTransitionEffect.FromRight
            });
        }

        public void SetFrame(Frame frame)
        {
            _frame = frame;
        }
    }
}
