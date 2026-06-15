using System;

namespace KvizCommando.Server.Domain.Entities.Compliance
{
    /// <summary>Marketing hozzájárulások audit naplója.</summary>
    public class MarketingConsent
    {
        public long Id { get; set; }                // PK (IDENTITY)
        public string UserId { get; set; } = null!; // FK → AspNetUsers.Id

        public bool Granted { get; set; }
        public DateTime ChangedAtUtc { get; set; }
    }
}
