namespace KvizCommando.Client.Pages.Team.ViewModels
{
    public sealed class BottomBlockVm
    {
        public List<BottomRow> Rows { get; } = [];
    }
    public sealed record BottomRow(
     string Label,
     string Column1,
     string Column2,
     string Remark,
     int Action
     );
}
