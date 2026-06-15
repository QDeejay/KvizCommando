namespace KvizCommando.Client.Models.DataModels
{
    public class CategoryRow
    {
        public int Id { get; set; }
        public string CategoryHu { get; set; }
        public string CategoryEn { get; set; }
        public string ShortCatHu { get; set; }
        public string ShortCatEn { get; set; }
        public CategoryRow(
            int id,
            string categoryhu,
            string categoryen,
            string shortcathu,
            string shortcaten
            )
        {
            Id = id;
            CategoryHu = categoryhu;
            CategoryEn = categoryen;
            ShortCatHu = shortcathu;
            ShortCatEn = shortcaten;
        }
    }
}
