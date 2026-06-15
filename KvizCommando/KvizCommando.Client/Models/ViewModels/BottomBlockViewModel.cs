namespace KvizCommando.Client.Models.ViewModels
{
    public class BottomBlockViewModel
    {
        public List<BottomBlockRow> Rows { get; } = new();
    }
    public record BottomBlockRow(
        string Label,
        string Column1,
        string Column2,
        string Remark,
        int action
    );
}
