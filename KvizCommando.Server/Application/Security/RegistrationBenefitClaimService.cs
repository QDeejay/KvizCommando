using System.Security.Cryptography;
using System.Text;
using KvizCommando.Server.Application.Abstractions.Security;
using KvizCommando.Server.Domain.Entities.Compliance;
using KvizCommando.Server.Infrastructure.Options;
using KvizCommando.Server.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace KvizCommando.Server.Application.Security;

public sealed class RegistrationBenefitClaimService : IRegistrationBenefitClaimService
{
    private readonly ApplicationDbContext _db;
    private readonly ILookupNormalizer _normalizer;
    private readonly byte[] _secret;

    public RegistrationBenefitClaimService(
        ApplicationDbContext db,
        ILookupNormalizer normalizer,
        IOptions<RegistrationBenefitHashOptions> options)
    {
        _db = db;
        _normalizer = normalizer;
        _secret = Encoding.UTF8.GetBytes(options.Value.Secret);
    }

    /// <inheritdoc />
    public Task<DateTime?> GetEligibleAgainAtUtcAsync(
        string email,
        CancellationToken ct = default)
    {
        var fingerprint = GetFingerprint(email);
        return _db.RegistrationBenefitClaims
            .AsNoTracking()
            .Where(x => x.EmailFingerprint == fingerprint)
            .Select(x => (DateTime?)x.EligibleAgainAtUtc)
            .SingleOrDefaultAsync(ct);
    }

    /// <inheritdoc />
    public async Task RecordAsync(
        string email,
        DateTime eligibleAgainAtUtc,
        CancellationToken ct = default)
    {
        var fingerprint = GetFingerprint(email);
        var claim = await _db.RegistrationBenefitClaims
            .SingleOrDefaultAsync(x => x.EmailFingerprint == fingerprint, ct);

        if (claim is null)
        {
            _db.RegistrationBenefitClaims.Add(new RegistrationBenefitClaim
            {
                EmailFingerprint = fingerprint,
                EligibleAgainAtUtc = eligibleAgainAtUtc
            });
        }
        else if (claim.EligibleAgainAtUtc < eligibleAgainAtUtc)
        {
            claim.EligibleAgainAtUtc = eligibleAgainAtUtc;
        }

        await _db.SaveChangesAsync(ct);
    }

    private string GetFingerprint(string email)
    {
        var normalized = _normalizer.NormalizeEmail(email)
            ?? email.Trim().ToUpperInvariant();
        return Convert.ToHexString(
            HMACSHA256.HashData(
                _secret,
                Encoding.UTF8.GetBytes(normalized)));
    }
}
