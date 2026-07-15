namespace KvizCommando.Client.Pages.Team.ViewModels
{
    public sealed class BottomDevVm
    {
        public List<BottomDevRow> Rows { get; set; } = [];
        public int[] UsedPoints { get; set; } = [0, 0, 0, 0];
        public int AvailableDevPoints { get; set; }
        public string HeaderText { get; set; } = string.Empty;
        public string ResetButtonText { get; set; } = string.Empty;
    }
    public sealed record BottomDevRow(
      string CategoryName,
      string LevelDisplay,
      string ValueDisplay,
      bool HasChanged,
      bool CanDevelop,
      string StatusText
      );



}
