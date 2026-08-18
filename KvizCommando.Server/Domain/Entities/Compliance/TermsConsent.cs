using System;

namespace KvizCommando.Server.Domain.Entities.Compliance
{
    /// <summary>Az ÁSZF-elfogadások csak bővíthető auditnapló-bejegyzése.</summary>
    public class TermsConsent
    {
        public long Id { get; set; }
        public string UserId { get; set; } = null!;

        public string TermsVersion { get; set; } = string.Empty;
        public DateTime AcceptedAtUtc { get; set; }

        // Az opcionális HMAC-kivonatok személyes adat tárolása nélkül támogatják az auditálást.
        public byte[]? UserAgentHash { get; set; }
        public byte[]? IpHash { get; set; }
    }
}
