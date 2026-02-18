namespace BlueShell.Terminal.Abstractions
{
    public interface IDataDisplay
    {
        void Add(object item);
        void SetHeader(object header);
        void Clear();
    }
}
