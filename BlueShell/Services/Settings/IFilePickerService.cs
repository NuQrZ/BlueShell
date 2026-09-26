using System.Threading.Tasks;
using Windows.Storage;

namespace BlueShell.Services.Settings
{
    public interface IFilePickerService
    {
        Task<StorageFile?> PickImageAsync();
    }
}
