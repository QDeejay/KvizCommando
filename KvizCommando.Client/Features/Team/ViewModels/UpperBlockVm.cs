namespace KvizCommando.Client.Features.Team.ViewModels
{
    public sealed class UpperBlockVm
    {
        public List<UpperRow> Rows { get; } = [];
    }

    /// <summary>
    /// A csapatösszesítő egy felirat–érték sorát tartalmazza.
    /// </summary>
    /// <param name="Label">A megjelenített mező felirata.</param>
    /// <param name="Value">A mező megjelenített értéke.</param>
    public sealed record UpperRow(
        string Label,
        string Value);
}
