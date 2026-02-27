using System.Collections.Generic;

namespace BlueShell.Model.Properties
{
    public sealed class PropertyGroup
    {
        public string? GroupName { get; set; }
        public List<PropertyRow>? PropertyRows { get; set; }
    }
}
