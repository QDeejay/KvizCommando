using System;

namespace KvizCommando.Server.Domain.Entities.Players
{
    /// <summary>
    /// A játékos kérdés-slot beállításai (21/21/5) JSON tömbökben.
    /// </summary>
    public class PlayerLoadout
    {
        public int PlayerId { get; set; }  // PK, FK → Players.PlayerId

        /// <summary>Gyári kérdés slotok (int[21]).</summary>
        public string FactorySlotsJson { get; set; } = "[]";

        /// <summary>Felhasználói kérdés slotok (int[21]).</summary>
        public string UserSlotsJson { get; set; } = "[]";

        /// <summary>Függő kérdés slotok (int?[5]).</summary>
        public string PendingSlotsJson { get; set; } = "[]";

        public string HelpLevelsJson { get; set; } = "[]";

        public DateTime UpdatedUtc { get; set; }
    }
}
