using BlueShell.Services;
using BlueShell.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using System;

namespace BlueShell
{
    public partial class App : Application
    {
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
            serviceCollection.AddSingleton<MainWindow>();

            serviceCollection.AddSingleton<TabViewModel>();

            return serviceCollection.BuildServiceProvider();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            MainWindow mainWindow = ServiceProvider!.GetRequiredService<MainWindow>();
            mainWindow.Activate();
        }
    }
}
