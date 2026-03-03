using System.Collections.ObjectModel;

namespace BlueShell.Model
{
    public sealed class DataDisplayGroup
    {
        public string Header { get; init; } = string.Empty;
        public ObservableCollection<DataDisplayItem> Items { get; } = [];
    }
}
