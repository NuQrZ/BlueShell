using BlueShell.View.Windows;
using BlueShell.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;

namespace BlueShell
{
    public partial class App : Application
    {
        public static MainWindow? MainWindow { get; private set; }
        public static ServiceProvider? ServiceProvider { get; private set; }

        public App()
        {
            InitializeComponent();
            ServiceProvider = ConfigureServices();
        }

        private static ServiceProvider ConfigureServices()
        {
            ServiceCollection serviceCollection = new();

            serviceCollection.AddSingleton<MainWindowViewModel>();

            return serviceCollection.BuildServiceProvider();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            MainWindow = new MainWindow();
            MainWindow.Activate();
        }
    }
}
