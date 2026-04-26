using BlueShell.Services;
using BlueShell.View.Pages;
using BlueShell.ViewModel;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System.ComponentModel;

namespace BlueShell
{
    public sealed partial class MainWindow : Window
    {
        private readonly MainWindowViewModel? _mainWindowViewModel;
        private readonly INavigationService? _navigationService;
        public MainWindow(
            MainWindowViewModel mainWindowViewModel,
            INavigationService navigationService)
        {
            InitializeComponent();

            SetTitleBar(AppTitleBar);
            ExtendsContentIntoTitleBar = true;

            SystemBackdrop = new MicaBackdrop()
            {
                Kind = MicaKind.BaseAlt
            };

            _mainWindowViewModel = mainWindowViewModel;
            _navigationService = navigationService;

            navigationService.SetFrame(MainWindowFrame);
            navigationService.Navigate(typeof(MainPage), _mainWindowViewModel);

            _mainWindowViewModel.PropertyChanged += MainWindowViewModel_PropertyChanged;
        }

        private void MainWindowViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainWindowViewModel.IsSettingsOpen))
            {
                bool isSettingsOpen = _mainWindowViewModel?.IsSettingsOpen ?? false;

                if (isSettingsOpen)
                {
                    _navigationService?.Navigate(typeof(SettingsPage));
                }
                else
                {
                    _navigationService?.Navigate(typeof(MainPage), _mainWindowViewModel);
                }
            }
        }

        private void TabView_TabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
        {
            _mainWindowViewModel?.RemoveTab(args.Item as TabViewModel);
        }
    }
}
