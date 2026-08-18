using System;
using System.Security.Cryptography;
using System.Text;
using KvizCommando.Server.Application.Abstractions.Security;
using KvizCommando.Server.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace KvizCommando.Server.Application.Security
{
    /// <summary>
    /// A normalizált e-mail-címből alkalmazástitokkal kiegészített SHA-256 keresési kivonatot képez.
    /// </summary>
    public class EmailLookup : IEmailLookup
    {
        private readonly SecurityOptions _opts;

        public EmailLookup(IOptions<SecurityOptions> opts)
        {
            _opts = opts.Value;
            if (string.IsNullOrWhiteSpace(_opts.EmailHashPepper))
                throw new InvalidOperationException("SecurityOptions.EmailHashPepper is not configured.");
        }

        /// <inheritdoc />
        public string Normalize(string email)
            => email.Trim().ToUpperInvariant();

        /// <inheritdoc />
        public byte[] ComputeNormalizedHash(string normalizedEmail)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(normalizedEmail + _opts.EmailHashPepper);
            return sha.ComputeHash(bytes);
        }

        /// <inheritdoc />
        public byte[] ComputeHashFromRaw(string email)
            => ComputeNormalizedHash(Normalize(email));
    }
}
