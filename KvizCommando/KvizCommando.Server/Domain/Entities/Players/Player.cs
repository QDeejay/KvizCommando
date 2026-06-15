using System;

namespace KvizCommando.Server.Domain.Entities.Players
{
    /// <summary>
    /// Játékos törzs rekord. Bridge az Identity felhasználó (UserId) és a játékbeli kulcs (PlayerId) között.
    /// </summary>
    public class Player
    {
        public int PlayerId { get; set; }                      // PK (int, IDENTITY)
        public string UserId { get; set; } = default!;         // FK → AspNetUsers.Id (UNIQUE)
        public string DisplayName { get; set; } = string.Empty;
        public string TeamName { get; set; } = string.Empty;

        /// <summary>Rang enum integer reprezentációja.</summary>
        public int RankEnum { get; set; }

        /// <summary>Összpont (leaderboardhoz indexelve).</summary>
        public int XP { get; set; }

        /// <summary>Játékon belüli coin egyenleg (leaderboardhoz is használható).</summary>
        public int Credit { get; set; }
        public int DevPoint { get; set; }

        /// <summary>Opcionális külön kupon egyenleg.</summary>
        public int Voucher { get; set; }

        public DateTime CreatedUtc { get; set; }
        public DateTime UpdatedUtc { get; set; }

        /// <summary>Optimista konkurenciakezeléshez.</summary>
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }
}
