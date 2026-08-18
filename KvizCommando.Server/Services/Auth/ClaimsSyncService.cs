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
    /// Az ÁSZF-verzióhoz tartozó claimet szinkronizálja az Identity-adattárral és a hitelesítési cookie-val.
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

        /// <summary>
        /// Az aktuális időponttal létrehozza vagy frissíti az ÁSZF-elfogadási claimet.
        /// </summary>
        public Task UpsertTermsClaimsNowAsync(ApplicationUser user, string termsEtag, CancellationToken cancellationToken = default)
            => UpsertTermsClaimsAsync(user, termsEtag, DateTime.UtcNow, cancellationToken);

        /// <summary>
        /// Létrehozza vagy frissíti a felhasználó ÁSZF-elfogadási claimjeit.
        /// </summary>
        public async Task UpsertTermsClaimsAsync(ApplicationUser user, string termsEtag, DateTime acceptedAtUtc, CancellationToken cancellationToken = default)
        {
            if (user is null) throw new ArgumentNullException(nameof(user));
            if (string.IsNullOrWhiteSpace(termsEtag)) throw new ArgumentException("ETag must not be null or empty.", nameof(termsEtag));

            cancellationToken.ThrowIfCancellationRequested();

            var claims = await _userManager.GetClaimsAsync(user).ConfigureAwait(false);

            Claim? existingEtag = null;
            foreach (var c in claims)
            {
                if (string.Equals(c.Type, CustomClaimTypes.TermsAcceptedEtag, StringComparison.Ordinal))
                {
                    existingEtag = c;
                    break;
                }
            }

            var newEtag = new Claim(CustomClaimTypes.TermsAcceptedEtag, termsEtag);

            // A változatlan claimet nem írjuk újra, így a biztonsági bélyeg sem módosul feleslegesen.
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

            if (changed && _httpContextAccessor.HttpContext is not null)
            {
                try
                {
                    await _signInManager.RefreshSignInAsync(user).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    // Bearer-tokenes kérésnél nincs frissíthető hitelesítési cookie; ez nem teszi sikertelenné a claim mentését.
                    _logger.LogDebug(ex, "ClaimsSync: RefreshSignIn kihagyva (nincs cookie-környezet vagy más ok).");
                }
            }
        }

        private static void EnsureSucceeded(IdentityResult result, string operation)
        {
            if (result.Succeeded) return;

            var msg = $"Identity {operation} failed: {string.Join("; ", result.Errors)}";
            throw new InvalidOperationException(msg);
        }
    }
}
