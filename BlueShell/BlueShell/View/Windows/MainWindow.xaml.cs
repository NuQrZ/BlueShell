using BlueShell.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

namespace BlueShell.View.Windows
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
        }
    }
}
