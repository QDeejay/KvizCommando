namespace KvizCommando.Client.Models.ViewModels
{
    public class UpperBlockViewModel
    {
        public List<UpperRow> Rows { get; } = new();
    }

    public record UpperRow(string Label, string Value);
}
