using KvizCommando.Shared.Contracts.Team;

namespace KvizCommando.Server.Models
{
    public sealed class CharachterSlot
    {
        public string Name { get; set; } = string.Empty; // Generated name      // lÉTREHOZÁS UTÁN NEM VÁLTOZHAT // nincs rá szűrás, ahol van az egész van beolvasva
        public string PictureCode { get; set; } = string.Empty;                             // lÉTREHOZÁS UTÁN NEM VÁLTOZHAT // nincs rá szűrás, ahol van az egész van beolvasva
        public int Rank { get; set; } = 0;                  // TÜBB MECCS UTÁN VÁLTOZHAT // nincs rá szűrás, ahol van az egész van beolvasva
        public int Pension { get; set; } = 0;
        public int XP { get; set; } = 0;                    // MECCSEK UTÁN VÁLTOZHAT// nincs rá szűrás, ahol van az egész van beolvasva
        public int DevPoints { get; set; } = 0;             // TÖBB MECCS UTÁN VÁLTOZHAT ILLETVE FEJLESZTÉS UTÁN // nincs rá szűrás, ahol van az egész van beolvasva
        public int EnergyPoints { get; set; } = 0;          // MECCSEK UTÁN VÁLTOZHAT// nincs rá szűrás, ahol van az egész van beolvasva
        public Attitude Attitude { get; set; } = new();    // lÉTREHOZÁS UTÁN NEM VÁLTOZHAT // nincs rá szűrás, ahol van az egész van beolvasva
        public CharStatistic CharStatistic { get; set; } = new(); // CSAK MECCSEK UTÁN VÁLTOZHAT    // nincs rá szűrás, ahol van az egész van beolvasva

    }
    public sealed class CharStatistic
    {
        public int PlayDuels { get; set; } = 0;     // CSAK MECCSEK UTÁN VÁLTOZHAT // nincs rá szűrás, ahol van az egész van beolvasva
        public int WinDuels { get; set; } = 0;     // CSAK MECCSEK UTÁN VÁLTOZHAT // nincs rá szűrás, ahol van az egész van beolvasva
    }
    public sealed class Attitude
    {
        public AttitudeBranch Main { get; set; } = new();
        public AttitudeBranch Secondary { get; set; } = new();
        public AttitudeBranch Gender { get; set; } = new();

    }
    public sealed class AttitudeBranch
    {
        public int[] CatNo { get; set; } = new int[4];     // karakter létrehozásakor választott hozzáállás nem változik
        public int[] Level { get; set; } = new int[4]; // Index 1–4 használandó   // ha szerzttünk dev pointot akkor változhat
    }
  
    public sealed class RecruitSlot 
    {
        public DateTime ExpirationTime { get; set; } = DateTime.UtcNow; // Ha lejárt, akkor új jelölt generálható
        public string[] Names { get; set; } = new string[8]; // jelöltek nevei
        public string[] PictureCodes { get; set; } = new string[8]; // jelöltek képkódjai
    }

}
