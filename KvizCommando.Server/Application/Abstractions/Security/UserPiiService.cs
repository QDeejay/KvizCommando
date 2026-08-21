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
        private const string PHONE_FIELD = "Phone";
        private const string BILLING_NAME_FIELD = "BillingName";
        private const string BILLING_ADDRESS_FIELD = "BillingAddress";

        private readonly ApplicationDbContext _db;
        private readonly IEncryptionProvider _enc;

        /// <summary>
        /// Létrehozza a titkosított PII-adatok tárolási szolgáltatását.
        /// </summary>
        /// <param name="db">Az Identity- és PII-adatokat kezelő adatbáziskontextus.</param>
        /// <param name="enc">A mezőszintű hitelesített titkosítás szolgáltatása.</param>
        public UserPiiService(
            ApplicationDbContext db,
            IEncryptionProvider enc)
        {
            _db = db;
            _enc = enc;
        }

        /// <inheritdoc />
        public async Task SetProfileAsync(
            string userId,
            string? phone,
            string? billingName,
            string? billingAddress,
            CancellationToken ct = default)
        {
            var entity = await EnsureEntity(userId, ct);
            SetEncryptedField(entity, phone, PHONE_FIELD,
                (cipher, nonce, tag) =>
                {
                    entity.PhoneEncrypted = cipher;
                    entity.PhoneNonce = nonce;
                    entity.PhoneTag = tag;
                });
            SetEncryptedField(entity, billingName, BILLING_NAME_FIELD,
                (cipher, nonce, tag) =>
                {
                    entity.BillingNameEncrypted = cipher;
                    entity.BillingNameNonce = nonce;
                    entity.BillingNameTag = tag;
                });
            SetEncryptedField(entity, billingAddress, BILLING_ADDRESS_FIELD,
                (cipher, nonce, tag) =>
                {
                    entity.BillingAddressEncrypted = cipher;
                    entity.BillingAddressNonce = nonce;
                    entity.BillingAddressTag = tag;
                });
            entity.UpdatedUtc = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
        }

        /// <inheritdoc />
        public async Task<(string? Phone, string? BillingName, string? BillingAddress)> GetProfileAsync(
            string userId,
            CancellationToken ct = default)
        {
            var entity = await _db.UserPii.AsNoTracking()
                .SingleOrDefaultAsync(x => x.UserId == userId, ct);
            if (entity is null)
                return (null, null, null);

            return (
                DecryptField(entity.PhoneEncrypted, entity.PhoneNonce, entity.PhoneTag, userId, PHONE_FIELD),
                DecryptField(entity.BillingNameEncrypted, entity.BillingNameNonce, entity.BillingNameTag, userId, BILLING_NAME_FIELD),
                DecryptField(entity.BillingAddressEncrypted, entity.BillingAddressNonce, entity.BillingAddressTag, userId, BILLING_ADDRESS_FIELD));
        }

        /// <inheritdoc />
        public async Task SetPhoneAsync(string userId, string phoneE164, CancellationToken ct = default)
        {
            var entity = await EnsureEntity(userId, ct);

            var context = BuildContext(userId, PHONE_FIELD);
            var (cipher, nonce, tag) = _enc.Encrypt(phoneE164, context);
            entity.PhoneEncrypted = cipher;
            entity.PhoneNonce = nonce;
            entity.PhoneTag = tag;
            entity.UpdatedUtc = DateTime.UtcNow;

            await _db.SaveChangesAsync(ct);
        }

        /// <inheritdoc />
        public async Task<string?> GetPhoneAsync(string userId, CancellationToken ct = default)
        {
            var entity = await _db.UserPii.FirstOrDefaultAsync(x => x.UserId == userId, ct);
            if (entity?.PhoneEncrypted is null || entity.PhoneNonce is null || entity.PhoneTag is null)
                return null;
            return _enc.Decrypt(
                entity.PhoneEncrypted,
                entity.PhoneNonce,
                entity.PhoneTag,
                BuildContext(userId, PHONE_FIELD));
        }

        /// <inheritdoc />
        public async Task SetBillingAsync(string userId, string billingName, string billingAddress, CancellationToken ct = default)
        {
            var entity = await EnsureEntity(userId, ct);

            var e1 = _enc.Encrypt(
                billingName,
                BuildContext(userId, BILLING_NAME_FIELD));
            entity.BillingNameEncrypted = e1.Cipher;
            entity.BillingNameNonce = e1.Nonce;
            entity.BillingNameTag = e1.Tag;

            var e2 = _enc.Encrypt(
                billingAddress,
                BuildContext(userId, BILLING_ADDRESS_FIELD));
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
                name = _enc.Decrypt(
                    entity.BillingNameEncrypted,
                    entity.BillingNameNonce,
                    entity.BillingNameTag,
                    BuildContext(userId, BILLING_NAME_FIELD));
            if (entity.BillingAddressEncrypted != null && entity.BillingAddressNonce != null && entity.BillingAddressTag != null)
                addr = _enc.Decrypt(
                    entity.BillingAddressEncrypted,
                    entity.BillingAddressNonce,
                    entity.BillingAddressTag,
                    BuildContext(userId, BILLING_ADDRESS_FIELD));

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
            return entity;
        }

        private void SetEncryptedField(
            UserPii entity,
            string? value,
            string fieldName,
            Action<byte[]?, byte[]?, byte[]?> assign)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                assign(null, null, null);
                return;
            }

            var encrypted = _enc.Encrypt(value.Trim(), BuildContext(entity.UserId, fieldName));
            assign(encrypted.Cipher, encrypted.Nonce, encrypted.Tag);
        }

        private string? DecryptField(
            byte[]? cipher,
            byte[]? nonce,
            byte[]? tag,
            string userId,
            string fieldName) =>
            cipher is null || nonce is null || tag is null
                ? null
                : _enc.Decrypt(cipher, nonce, tag, BuildContext(userId, fieldName));

        private static string BuildContext(string userId, string fieldName) =>
            $"UserPii:{userId}:{fieldName}";
    }
}
