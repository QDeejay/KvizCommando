namespace KvizCommando.Client.Features.Team.ViewModels
{
    public sealed class BottomBlockVm
    {
        public List<BottomRow> Rows { get; } = [];
    }
    /// <summary>
    /// A csapattaglista egy megjelenítési sorát és az opcionális műveletét tartalmazza.
    /// </summary>
    /// <param name="Label">A sor felirata.</param>
    /// <param name="Column1">Az első értékoszlop tartalma.</param>
    /// <param name="Column2">A második értékoszlop tartalma.</param>
    /// <param name="Remark">A sorhoz tartozó kiegészítő megjegyzés.</param>
    /// <param name="Action">A sorból indítható csapattagművelet, ha van.</param>
    public sealed record BottomRow(
     string Label,
     string Column1,
     string Column2,
     string Remark,
     TeamMemberAction? Action);
}
