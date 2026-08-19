using System;

namespace KvizCommando.Server.Domain.Entities.Security
{
    /// <summary>
    /// Az Identity-fiókhoz tartozó, külön titkosított kapcsolattartási és számlázási adatokat tárolja.
    /// </summary>
    public class UserPii
    {
        /// <summary>Az <c>AspNetUsers</c> rekord olvasható kapcsolóazonosítója.</summary>
        public string UserId { get; set; } = null!;

        /// <summary>A telefonszám AES-GCM-mel titkosított tartalma.</summary>
        public byte[]? PhoneEncrypted { get; set; }

        /// <summary>A telefonszám titkosításakor használt egyszeri nonce.</summary>
        public byte[]? PhoneNonce { get; set; }

        /// <summary>A telefonszám titkosításának hitelesítési címkéje.</summary>
        public byte[]? PhoneTag { get; set; }

        /// <summary>A számlázási név AES-GCM-mel titkosított tartalma.</summary>
        public byte[]? BillingNameEncrypted { get; set; }

        /// <summary>A számlázási név titkosításakor használt egyszeri nonce.</summary>
        public byte[]? BillingNameNonce { get; set; }

        /// <summary>A számlázási név titkosításának hitelesítési címkéje.</summary>
        public byte[]? BillingNameTag { get; set; }

        /// <summary>A számlázási cím AES-GCM-mel titkosított tartalma.</summary>
        public byte[]? BillingAddressEncrypted { get; set; }

        /// <summary>A számlázási cím titkosításakor használt egyszeri nonce.</summary>
        public byte[]? BillingAddressNonce { get; set; }

        /// <summary>A számlázási cím titkosításának hitelesítési címkéje.</summary>
        public byte[]? BillingAddressTag { get; set; }

        /// <summary>A rekord létrehozásának UTC időpontja.</summary>
        public DateTime CreatedUtc { get; set; }

        /// <summary>A rekord utolsó módosításának UTC időpontja.</summary>
        public DateTime UpdatedUtc { get; set; }
    }
}
