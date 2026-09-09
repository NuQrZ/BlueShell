using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;

namespace BlueShell.Services
{
    public sealed class ClipboardService : IClipboardService
    {
        public void Copy(string text)
        {
            DataPackage dataPackage = new()
            {
                RequestedOperation = DataPackageOperation.Copy
            };

            dataPackage.SetText(text);
            Clipboard.SetContent(dataPackage);
        }

        public async Task<string> GetTextAsync()
        {
            DataPackageView dataPackageView = Clipboard.GetContent();

            if (!dataPackageView.Contains(StandardDataFormats.Text))
            {
                return "";
            }

            return await dataPackageView.GetTextAsync();
        }
    }
}
