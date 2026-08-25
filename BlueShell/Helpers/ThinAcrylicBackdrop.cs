using Microsoft.UI.Composition;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

namespace BlueShell.Helpers
{
    public sealed partial class ThinAcrylicBackdrop : SystemBackdrop
    {
        private DesktopAcrylicController? _controller;
        private ICompositionSupportsSystemBackdrop? _target;
        private SystemBackdropConfiguration? _configuration;

        protected override void OnTargetConnected(ICompositionSupportsSystemBackdrop connectedTarget, XamlRoot xamlRoot)
        {
            base.OnTargetConnected(connectedTarget, xamlRoot);

            _target = connectedTarget;
            _controller = new DesktopAcrylicController
            {
                Kind = DesktopAcrylicKind.Thin
            };

            _configuration = GetDefaultSystemBackdropConfiguration(connectedTarget, xamlRoot);

            _controller.AddSystemBackdropTarget(connectedTarget);
            _controller.SetSystemBackdropConfiguration(_configuration);
        }

        protected override void OnTargetDisconnected(ICompositionSupportsSystemBackdrop disconnectedTarget)
        {
            base.OnTargetDisconnected(disconnectedTarget);

            _controller?.Dispose();
            _controller = null;
            _target = null;
        }
    }
}
