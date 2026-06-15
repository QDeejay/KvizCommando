
namespace KvizCommando.Shared.Models.User
{

    public sealed class UserMainData
    {
        public int PlayerId { get; set; } = 0;   // lÉTREHOZÁS UTÁN NEM VÁLTOZHAT
        public string UserName { get; set; } = string.Empty;  // lÉTREHOZÁS UTÁN NEM VÁLTOZHAT EZ A DISPLAY NÉV DE AZ IDENTITY  INKÁBB   
        public string TeamName { get; set; } = string.Empty; // VÁLTOZHAT
        public int RankEnum { get; set; } = 0;  // TÖBB MECCS UTÁN VÁLTOZHAT     
        public int XP { get; set; } = 0;  // MECSEK UTÁN VÁLTOZHAT      // nincs rá szűrás, 
        public int Voucher { get; set; } = 0;   // RITKÁN VÁLTOZHAT     // nincs rá szűrás, 
        public int Credit { get; set; } = 0;  // GYAK VÁLTOZHAT 
        //public int TeamDevPoints { get; set; } = 0;  // GYAK VÁLTOZHAT 
    }
}


   