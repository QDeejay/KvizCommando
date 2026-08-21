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
    public async Task RecordAsync(
        string email,
        DateTime eligibleAgainAtUtc,
        CancellationToken ct = default)
    {
        var normalized = _normalizer.NormalizeEmail(email)
            ?? email.Trim().ToUpperInvariant();
        var fingerprint = Convert.ToHexString(
            HMACSHA256.HashData(_secret, Encoding.UTF8.GetBytes(normalized)));
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
}
