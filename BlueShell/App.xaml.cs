using BlueShell.Services;
using BlueShell.Services.Settings;
using BlueShell.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using System;

namespace BlueShell
{
    public partial class App : Application
    {
        public static View.MainWindow? MainWindow { get; private set; }
        public static IServiceProvider? ServiceProvider { get; private set; }

        public App()
        {
            InitializeComponent();
            ServiceProvider = ConfigureServices();
        }

        private static ServiceProvider ConfigureServices()
        {
            ServiceCollection serviceCollection = new();

            serviceCollection.AddSingleton<MainWindowViewModel>();
            serviceCollection.AddSingleton<INavigationService, NavigationService>();
            serviceCollection.AddSingleton<IWindowAppearanceService, WindowAppearanceService>();
            serviceCollection.AddSingleton<IImageService, ImageService>();
            serviceCollection.AddSingleton<IFilePickerService, FilePickerService>();
            serviceCollection.AddSingleton<View.MainWindow>();

            serviceCollection.AddSingleton<SettingsViewModel>();
            serviceCollection.AddSingleton<TabViewModel>();

            return serviceCollection.BuildServiceProvider();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            MainWindow = ServiceProvider!.GetRequiredService<View.MainWindow>();
            MainWindow.Activate();
        }
    }
}
