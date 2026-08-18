#nullable enable
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using KvizCommando.Server.Identity;
using KvizCommando.Server.Infrastructure.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace KvizCommando.Server.Services.Auth
{
    /// <summary>
    /// Claim-szinkron: UserManager API-val upsertel az AspNetUserClaims táblába
    /// (csak a terms.accepted.etag-et), majd – ha van HTTP-kontekstus – 
    /// RefreshSignInAsync-kel frissíti a cookie-t.
    /// </summary>
    internal sealed class ClaimsSyncService : IClaimsSyncService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<ClaimsSyncService> _logger;

        public ClaimsSyncService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IHttpContextAccessor httpContextAccessor,
            ILogger<ClaimsSyncService> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public Task UpsertTermsClaimsNowAsync(ApplicationUser user, string termsEtag, CancellationToken cancellationToken = default)
            => UpsertTermsClaimsAsync(user, termsEtag, DateTime.UtcNow, cancellationToken);

        public async Task UpsertTermsClaimsAsync(ApplicationUser user, string termsEtag, DateTime acceptedAtUtc, CancellationToken cancellationToken = default)
        {
            if (user is null) throw new ArgumentNullException(nameof(user));
            if (string.IsNullOrWhiteSpace(termsEtag)) throw new ArgumentException("ETag must not be null or empty.", nameof(termsEtag));

            cancellationToken.ThrowIfCancellationRequested();

            // Jelenlegi claim-ek kiolvasása
            var claims = await _userManager.GetClaimsAsync(user).ConfigureAwait(false);

            // Meglévő claim felkutatása LINQ nélkül (nem használunk FirstOrDefault-ot)
            Claim? existingEtag = null;
            foreach (var c in claims)
            {
                if (string.Equals(c.Type, CustomClaimTypes.TermsAcceptedEtag, StringComparison.Ordinal))
                {
                    existingEtag = c;
                    break;
                }
            }

            // Új cél-claim objektum
            var newEtag = new Claim(CustomClaimTypes.TermsAcceptedEtag, termsEtag);

            // Idempotens upsert: csak akkor írunk, ha változott
            bool changed = false;

            if (existingEtag is null)
            {
                var add = await _userManager.AddClaimAsync(user, newEtag).ConfigureAwait(false);
                EnsureSucceeded(add, "AddClaim (TermsAcceptedEtag)");
                changed = true;
            }
            else if (!string.Equals(existingEtag.Value, newEtag.Value, StringComparison.Ordinal))
            {
                var rep = await _userManager.ReplaceClaimAsync(user, existingEtag, newEtag).ConfigureAwait(false);
                EnsureSucceeded(rep, "ReplaceClaim (TermsAcceptedEtag)");
                changed = true;
            }

            // Ha történt változás és van HTTP-kontekstus (cookie-auth), frissítjük a bejelentkezést
            if (changed && _httpContextAccessor.HttpContext is not null)
            {
                try
                {
                    await _signInManager.RefreshSignInAsync(user).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    // Bearer/opaque tokenes hívásoknál itt jellemzően nincs HttpContext cookie-hoz → nem hiba.
                    _logger.LogDebug(ex, "ClaimsSync: RefreshSignIn kihagyva (nincs cookie-környezet vagy más ok).");
                }
            }
        }

        private static void EnsureSucceeded(IdentityResult result, string operation)
        {
            if (result.Succeeded) return;

            // Összefűzzük a hibákat egyetlen kivételbe – professzionális, nem nyeli el a hibát
            var msg = $"Identity {operation} failed: {string.Join("; ", result.Errors)}";
            throw new InvalidOperationException(msg);
        }
    }
}
