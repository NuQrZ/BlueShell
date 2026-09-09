using System.Threading.Tasks;

namespace BlueShell.Services
{
    public interface IClipboardService
    {
        void Copy(string text);
        Task<string> GetTextAsync();
    }
}
