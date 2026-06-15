namespace KvizCommando.Client.Services.Visual
{
    public class CategoryOption
    {
        public int Code { get; }
        public string LabelHu { get; }
        public string LabelEn { get; }


        public CategoryOption(int code, string labelhu, string labelen)
        {
            Code = code;
            LabelHu = labelhu ?? string.Empty;
            LabelEn = labelen ?? string.Empty;

        }
    }
}

