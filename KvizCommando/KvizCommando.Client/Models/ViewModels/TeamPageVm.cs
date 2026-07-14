namespace KvizCommando.Client.Models.ViewModels
{
    public sealed class UpperBlockVm
    {
        public List<UpperRow> Rows { get; } = [];
    }
    public sealed class BottomBlockVm
    {
        public List<BottomRow> Rows { get; } = [];
    }
    public sealed class BottomDevVm
    {
        public List<BottomDevRow> Rows { get; set; } = [];
        public int[] UsedPoints { get; set; } = [0, 0, 0, 0];
        public int AvailableDevPoints { get; set; }
        public string HeaderText { get; set; } = string.Empty;
        public string ResetButtonText { get; set; } = string.Empty;
    }
    public sealed class RecruitVm
    {
        public List<RecruitBlock> Cards { get; } = new();
        public string Info { get; set; } = string.Empty;
    }

    public sealed class RecruitCardEntity
    {
        public string Name { get; set; } = "";
        public string Info1 { get; set; } = "";
        public string Info2 { get; set; } = "";
        public string Info3 { get; set; } = "";
        public string SecOrient { get; set; } = "";
        public bool Sex { get; set; } = false;
    }

    public sealed record UpperRow(
        string Label,
        string Value
        );
    public sealed record BottomRow(
        string Label,
        string Column1,
        string Column2,
        string Remark,
        int Action
        );
    public sealed record BottomDevRow(
        string CategoryName,
        string LevelDisplay,
        string ValueDisplay,
        bool HasChanged,
        bool CanDevelop,
        string StatusText
        );
    public sealed record RecruitBlock(
        string Name,
        bool Sex,
        bool Show,
        string ImageCode,
        string SubOrientSh,
        string MainCat,
        string SubCat,
        string ExtCat,
        int ClickId
    );

}
