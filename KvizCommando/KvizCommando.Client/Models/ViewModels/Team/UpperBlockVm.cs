namespace KvizCommando.Client.Models.ViewModels.Team
{
    public sealed class UpperBlockVm
    {
        public List<UpperRow> Rows { get; } = [];
    }

    public sealed record UpperRow(
        string Label,
        string Value
        );
}
