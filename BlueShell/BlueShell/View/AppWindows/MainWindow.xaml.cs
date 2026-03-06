using BlueShell.Helpers;
using BlueShell.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace BlueShell.View.AppWindows
{
    public sealed partial class MainWindow : Window
    {
        private readonly MainWindowViewModel _mainWindowViewModel;
        public MainWindow()
        {
            _mainWindowViewModel = App.ServiceProvider!.GetRequiredService<MainWindowViewModel>();

            InitializeComponent();

            ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar);

            SystemBackdrop = new MicaBackdrop()
            {
                Kind = MicaKind.BaseAlt
            };

            AppTitleBar.Loaded += async (_, _) =>
            {
                Stopwatch stopwatch = Stopwatch.StartNew();

                await Task.Run(() =>
                {
                    foreach (DriveInfo driveInfo in DriveInfo.GetDrives())
                    {
                        WmiUtilities.GetAllDriveProperties(driveInfo.RootDirectory.FullName);
                    }
                });

                stopwatch.Stop();

                Debug.WriteLine($"WMI preload total time: {stopwatch.ElapsedMilliseconds} ms");
            };
        }
    }
}
