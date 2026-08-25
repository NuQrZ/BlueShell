using BlueShell.ViewModel;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace BlueShell.View.Pages
{
    public sealed partial class TerminalPage : Page
    {
        private TabViewModel? _tabViewModel;

        public TerminalPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is TabViewModel tabViewModel)
            {
                _tabViewModel = tabViewModel;
            }

            TerminalControl.BuildTabModel(_tabViewModel?.Tab);
        }
    }
}
