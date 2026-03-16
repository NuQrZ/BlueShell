namespace BlueShell.Terminal.Abstractions
{
    public interface IAddressBarNavigator
    {
        void SetPath(string path, bool isMultiple);
        void ClearPath();
    }
}