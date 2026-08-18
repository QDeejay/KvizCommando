namespace KvizCommando.Client.Models.ViewModels
{
    public sealed class RecruitVm
    {
        public List<RecruitBlock> Cards { get; } = new();
        public string Info { get; set; } = string.Empty;
    }
    /// <summary>
    /// Egy toborzási kártya láthatóságát és kattintási azonosítóját kapcsolja a jelölt adataihoz.
    /// </summary>
    /// <param name="Show">Jelzi, hogy a kártya megjelenjen-e.</param>
    /// <param name="ClickId">A felületi művelet azonosítója.</param>
    /// <param name="Card">A megjelenítendő jelölt kártyaadatai.</param>
    public sealed record RecruitBlock(
        bool Show,
        int ClickId,
        RecruitCardVm Card
         );
    public sealed class RecruitCardVm
    {
        public string Name { get; set; } = "";
        public bool Sex { get; set; } = false;
        public string MainCat { get; set; } = "";
        public string SubCat { get; set; } = "";
        public string ExtCat { get; set; } = "";
        public string SecOrient { get; set; } = "";

    }
}
