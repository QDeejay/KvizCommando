using KvizCommando.Client.Models.DataModels;

namespace KvizCommando.Client.Data
{
    public static class OrientationNameTable
    {
        public static readonly List<OrientationNameRow> Data =
        [
            new(0, "Szabad", "Vacant", "N/A","N/A"),
            new(1, "Teologus", "Theologian", "Teo","Teo"),
            new(2, "Kronológus", "Chronologist", "Kro","Chr"),
            new(3, "Médiaguru", "Media Guru", "Méd", "Med"),
            new(4, "Játékmester", "Game Master", "Ját", "Gam"),
            new(5, "Technológus", "Technologist", "Tec", "Tec"),
            new(6, "Tudós", "Scientist", "Tud", "Sci"),
            new(7, "Trendológus", "Trendologist", "Tre", "Tre"),
            new(8, "Esztéta", "Aesthete", "Esz", "Aes"),
            new(9, "Csapat Kapitány", "Team Captain", "Kap", "Cap"),
        ];
    }
}
