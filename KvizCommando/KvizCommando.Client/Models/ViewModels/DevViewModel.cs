namespace KvizCommando.Client.Models.ViewModels
{
    public sealed class DevViewModel
    {
        public List<DevRow> Rows { get; set; } = new();
        public int[] UsedPoints { get; set; } = [0, 0, 0, 0];
        public int AvailableDevPoints { get; set; }
        public string HeaderText { get; set; } = string.Empty;
        public string ListType { get; set; } = string.Empty;

        public string SaveButtonText { get; set; } = string.Empty;


    }
    public sealed record DevRow(
        string CategoryName,
        string LevelDisplay,
        string ValueDisplay,
        bool HasChanged,
        bool CanDevelop,
        string StatusText
    );
}
    
