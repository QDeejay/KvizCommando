using System;
using System.Threading;
using System.Threading.Tasks;
using KvizCommando.Server.Application.Abstractions.Security;
using KvizCommando.Server.Domain.Entities.Security;
using KvizCommando.Server.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace KvizCommando.Server.Application.Security
{
    /// <summary>
    /// A személyes adatok mezőszintű tárolását és visszaolvasását kezeli.
    /// A tényleges adatvédelem erősségét a regisztrált titkosítási adapter határozza meg.
    /// </summary>
    public class UserPiiService : IUserPiiService
    {
        private readonly ApplicationDbContext _db;
        private readonly IEmailLookup _emailLookup;
        private readonly IEncryptionProvider _enc;

        public UserPiiService(
            ApplicationDbContext db,
            IEmailLookup emailLookup,
            IEncryptionProvider enc)
        {
            _db = db;
            _emailLookup = emailLookup;
            _enc = enc;
        }

        /// <inheritdoc />
        public async Task SetEmailAsync(string userId, string email, CancellationToken ct = default)
        {
            var entity = await EnsureEntity(userId, ct);

            var (cipher, nonce, tag) = _enc.Encrypt(email);
            entity.EmailEncrypted = cipher;
            entity.EmailNonce = nonce;
            entity.EmailTag = tag;
            entity.EmailNormHash = _emailLookup.ComputeHashFromRaw(email);
            entity.UpdatedUtc = DateTime.UtcNow;

            await _db.SaveChangesAsync(ct);
        }

        /// <inheritdoc />
        public async Task<string?> GetEmailAsync(string userId, CancellationToken ct = default)
        {
            var entity = await _db.UserPii.FirstOrDefaultAsync(x => x.UserId == userId, ct);
            if (entity?.EmailEncrypted is null || entity.EmailNonce is null || entity.EmailTag is null)
                return null;
            return _enc.Decrypt(entity.EmailEncrypted, entity.EmailNonce, entity.EmailTag);
        }

        /// <inheritdoc />
        public async Task<string?> FindUserIdByEmailAsync(string email, CancellationToken ct = default)
        {
            var hash = _emailLookup.ComputeHashFromRaw(email);
            var entity = await _db.UserPii.FirstOrDefaultAsync(x => x.EmailNormHash == hash, ct);
            return entity?.UserId;
        }

        /// <inheritdoc />
        public async Task SetPhoneAsync(string userId, string phoneE164, CancellationToken ct = default)
        {
            var entity = await EnsureEntity(userId, ct);

            var (cipher, nonce, tag) = _enc.Encrypt(phoneE164);
            entity.PhoneEncrypted = cipher;
            entity.PhoneNonce = nonce;
            entity.PhoneTag = tag;
            // Az egységes hash-szolgáltatás ideiglenesen telefonszámhoz is használatos.
            // Élesítés előtt külön normalizálási szabály szükséges a telefonszámokhoz.
            entity.PhoneNormHash = _emailLookup.ComputeNormalizedHash(phoneE164);
            entity.UpdatedUtc = DateTime.UtcNow;

            await _db.SaveChangesAsync(ct);
        }

        /// <inheritdoc />
        public async Task<string?> GetPhoneAsync(string userId, CancellationToken ct = default)
        {
            var entity = await _db.UserPii.FirstOrDefaultAsync(x => x.UserId == userId, ct);
            if (entity?.PhoneEncrypted is null || entity.PhoneNonce is null || entity.PhoneTag is null)
                return null;
            return _enc.Decrypt(entity.PhoneEncrypted, entity.PhoneNonce, entity.PhoneTag);
        }

        /// <inheritdoc />
        public async Task SetBillingAsync(string userId, string billingName, string billingAddress, CancellationToken ct = default)
        {
            var entity = await EnsureEntity(userId, ct);

            var e1 = _enc.Encrypt(billingName);
            entity.BillingNameEncrypted = e1.Cipher;
            entity.BillingNameNonce = e1.Nonce;
            entity.BillingNameTag = e1.Tag;

            var e2 = _enc.Encrypt(billingAddress);
            entity.BillingAddressEncrypted = e2.Cipher;
            entity.BillingAddressNonce = e2.Nonce;
            entity.BillingAddressTag = e2.Tag;

            entity.UpdatedUtc = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
        }

        /// <inheritdoc />
        public async Task<(string? BillingName, string? BillingAddress)> GetBillingAsync(string userId, CancellationToken ct = default)
        {
            var entity = await _db.UserPii.FirstOrDefaultAsync(x => x.UserId == userId, ct);
            if (entity is null) return (null, null);

            string? name = null, addr = null;
            if (entity.BillingNameEncrypted != null && entity.BillingNameNonce != null && entity.BillingNameTag != null)
                name = _enc.Decrypt(entity.BillingNameEncrypted, entity.BillingNameNonce, entity.BillingNameTag);
            if (entity.BillingAddressEncrypted != null && entity.BillingAddressNonce != null && entity.BillingAddressTag != null)
                addr = _enc.Decrypt(entity.BillingAddressEncrypted, entity.BillingAddressNonce, entity.BillingAddressTag);

            return (name, addr);
        }

        private async Task<UserPii> EnsureEntity(string userId, CancellationToken ct)
        {
            var entity = await _db.UserPii.FirstOrDefaultAsync(x => x.UserId == userId, ct);
            if (entity != null) return entity;

            entity = new UserPii
            {
                UserId = userId,
                CreatedUtc = DateTime.UtcNow,
                UpdatedUtc = DateTime.UtcNow
            };
            _db.UserPii.Add(entity);
            await _db.SaveChangesAsync(ct);
            return entity;
        }
    }
}
