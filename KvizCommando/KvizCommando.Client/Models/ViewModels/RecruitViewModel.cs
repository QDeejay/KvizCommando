namespace KvizCommando.Client.Models.ViewModels
{
    public sealed class RecruitViewModel
    {
        public List<RecruitBlock> Cards { get; } = new();
        public string Info { get; set; } = string.Empty;
    }

    public record RecruitBlock(
        string Name,
        bool Sex,
        string ImageCode,
        string SubOrientSh,
        string MainCat,
        string SubCat,
        string ExtCat,
        int ClickId
    );
    public sealed class RecruitCardEntity
    {
        public string Name { get; set; } = "";
        public string Info1 { get; set; } = "";
        public string Info2 { get; set; } = "";
        public string Info3 { get; set; } = "";
        public string SecOrient { get; set; } = "";
        public bool Sex { get; set; } = false;
    }
}
