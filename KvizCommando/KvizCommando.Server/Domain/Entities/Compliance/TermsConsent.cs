using System;

namespace KvizCommando.Server.Domain.Entities.Compliance
{
    /// <summary>ÁSZF elfogadások audit naplója. Append-only; (UserId, TermsVersion) egyedi.</summary>
    public class TermsConsent
    {
        public long Id { get; set; }                // PK (IDENTITY)
        public string UserId { get; set; } = null!; // FK → AspNetUsers.Id

        public string TermsVersion { get; set; } = string.Empty;
        public DateTime AcceptedAtUtc { get; set; }

        // Opcionális, PII-minimalista azonosítók (HMAC-SHA256 → 32 byte)
        public byte[]? UserAgentHash { get; set; }
        public byte[]? IpHash { get; set; }
    }
}
