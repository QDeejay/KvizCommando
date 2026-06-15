using KvizCommando.Client.Models.DataModels;

namespace KvizCommando.Client.Data
{
    public static class OrientationNameTable
    {
        public static readonly List<OrientationNameRow> Data = new()
        {
            new(0, "Szabad", "Vaccant", "N/A","N/A"),
            new(1, "Teologus", "Teologiest", "Teo","Teo"),
            new(2, "Történész", "Historian", "Tör","His"),
            new(3, "Művész", "Artist", "Műv", "Art"),
            new(4, "Játékos", "Gamer", "Ját", "Gam"),
            new(5, "Mérnök", "Engineer", "Mér", "Eng"),
            new(6, "Tudós", "Scientist", "Tud", "Sci"),
            new(7, "Trendi", "Trendy", "Tre", "Tre"),
            new(8, "Művelt", "Educated", "Mvt", "Edu")
        };
    }
}
