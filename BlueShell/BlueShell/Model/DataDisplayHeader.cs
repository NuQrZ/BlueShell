using Microsoft.UI.Xaml;

namespace BlueShell.Model
{
    public sealed class DataDisplayHeader
    {
        public string? NameHeader { get; set; }
        public string? SizeHeader { get; set; }
        public string? SubFoldersHeader { get; set; }
        public string? TypeHeader { get; set; }
        public Thickness NameMargin { get; set; }
        public Thickness SizeMargin { get; set; }
        public Thickness SubFoldersMargin { get; set; }
        public Thickness TypeMargin { get; set; }
    }
}