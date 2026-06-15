namespace KvizCommando.Client.Models.DataModels
{
    public class OrientationNameRow
    {
        public int Id { get; set; }
        public string NameHu { get; set; }
        public string NameEn { get; set; }
        public string ShortHu { get; set; }
        public string ShortEn { get; set; }
        //public int AttitudeIndex { get; set; }

        public OrientationNameRow(
            int id,
            string namehu,
            string nameen,
            string shorthu,
            string shorten
            //int attitudeindex
            )
        {
            Id = id;
            NameHu = namehu;
            NameEn = nameen;
            ShortHu = shorthu;
            ShortEn = shorten;
            //AttitudeIndex = attitudeindex;
        }

    }
}