namespace KvizCommando.Client.Models.ViewModels.Team
{
    public sealed class RecruitVm
    {
        public List<RecruitBlock> Cards { get; } = new();
        public string Info { get; set; } = string.Empty;
    }
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
