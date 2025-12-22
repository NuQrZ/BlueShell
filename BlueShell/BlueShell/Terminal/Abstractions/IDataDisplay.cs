namespace BlueShell.Terminal.Abstractions
{
    public interface IDataDisplay
    {
        void Clear();
        void Add(object item);
        void SetHeader(object header);
    }
}
