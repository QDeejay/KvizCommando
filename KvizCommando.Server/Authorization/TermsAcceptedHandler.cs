#nullable enable
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using KvizCommando.Server.Domain.Entities.Compliance;
using KvizCommando.Server.Infrastructure.Auth;
using KvizCommando.Server.Infrastructure.Persistence;
using KvizCommando.Server.Services.CheckIn;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace KvizCommando.Server.Authorization
{
    /// <summary>
    /// Az ÁSZF-claimet az aktuális központi verzióval összevető jogosultságkezelő.
    /// Hiányzó claim esetén konfigurálható adatbázis-ellenőrzést alkalmazhat.
    /// </summary>
    public sealed class TermsAcceptedHandler : AuthorizationHandler<TermsAcceptedRequirement>
    {
        private readonly ITermsProvider _termsProvider;
        private readonly ApplicationDbContext _db;
        private readonly IConfiguration _config;
        private readonly ILogger<TermsAcceptedHandler> _logger;

        public TermsAcceptedHandler(
            ITermsProvider termsProvider,
            ApplicationDbContext db,
            IConfiguration config,
            ILogger<TermsAcceptedHandler> logger)
        {
            _termsProvider = termsProvider;
            _db = db;
            _config = config;
            _logger = logger;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, TermsAcceptedRequirement requirement)
        {
            if (context.User?.Identity?.IsAuthenticated != true)
                return;

            var currentEtag = _termsProvider.CurrentTermsEtag;
            if (string.IsNullOrWhiteSpace(currentEtag))
                return;

            var claimedEtag = context.User.FindFirst(CustomClaimTypes.TermsAcceptedEtag)?.Value;

            if (!string.IsNullOrEmpty(claimedEtag))
            {
                if (string.Equals(claimedEtag, currentEtag, StringComparison.Ordinal))
                {
                    context.Succeed(requirement);
                }
                else
                {
                    // Az elavult claim kifejezett elutasítása megakadályozza a DB-fallback használatát.
                    context.Fail();
                }
                return;
            }

            // Az adatbázis-ellenőrzés csak hiányzó claimnél használható; elavult claimet nem írhat felül.
            var enableDbFallback = _config.GetValue<bool>("Auth:TermsPolicy:EnableDbFallback", false);
            if (!enableDbFallback)
                return;

            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userId))
                return;

            try
            {
                var lastAccepted = await _db.Set<TermsConsent>()
                    .AsNoTracking()
                    .Where(x => x.UserId == userId)
                    .OrderByDescending(x => x.AcceptedAtUtc)
                    .Select(x => x.TermsVersion)
                    .Take(1)
                    .ToListAsync()
                    .ConfigureAwait(false);

                var lastVersion = lastAccepted.Count > 0 ? lastAccepted[0] : null;

                if (!string.IsNullOrEmpty(lastVersion) &&
                    string.Equals(lastVersion, currentEtag, StringComparison.Ordinal))
                {
                    // A következő tokenfrissítés vagy cookie-belépés pótolja a hiányzó claimet.
                    context.Succeed(requirement);
                }
                else
                {
                    context.Fail();
                }
            }
            catch (Exception ex)
            {
                // Ellenőrzési hiba esetén a jogosultság nem adható meg.
                _logger.LogWarning(ex, "Terms policy DB-fallback hiba. UserId={UserId}", userId);
                context.Fail();
            }
        }
    }
}
