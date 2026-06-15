using System;

namespace KvizCommando.Server.Domain.Entities.Security
{
    /// <summary>
    /// Személyes adatok titkosítva. Kereséshez normált hash mezők.
    /// </summary>
    public class UserPii
    {
        public string UserId { get; set; } = null!; // PK, FK → AspNetUsers.Id

        // Email – AES-GCM: Encrypted + Nonce(12) + Tag(16). Keresés: NormHash (SHA-256 + pepper)
        public byte[]? EmailEncrypted { get; set; }
        public byte[]? EmailNonce { get; set; }
        public byte[]? EmailTag { get; set; }
        public byte[]? EmailNormHash { get; set; } // 32 byte

        // Phone – opcionális
        public byte[]? PhoneEncrypted { get; set; }
        public byte[]? PhoneNonce { get; set; }
        public byte[]? PhoneTag { get; set; }
        public byte[]? PhoneNormHash { get; set; } // ha kell equality keresés

        // Billing – egyszerű modell: név + cím (egyben). Bővíthető külön mezőkre.
        public byte[]? BillingNameEncrypted { get; set; }
        public byte[]? BillingNameNonce { get; set; }
        public byte[]? BillingNameTag { get; set; }

        public byte[]? BillingAddressEncrypted { get; set; }
        public byte[]? BillingAddressNonce { get; set; }
        public byte[]? BillingAddressTag { get; set; }

        public DateTime CreatedUtc { get; set; }
        public DateTime UpdatedUtc { get; set; }
    }
}
