#nullable enable
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using KvizCommando.Server.Domain.Entities.Compliance;
using KvizCommando.Server.Infrastructure.Auth;           // CustomClaimTypes
using KvizCommando.Server.Infrastructure.Persistence;   // ApplicationDbContext
using KvizCommando.Server.Services.CheckIn;             // ITermsProvider
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace KvizCommando.Server.Authorization
{
    /// <summary>
    /// Authorization handler, amely összeveti a principal ÁSZF-claimjét (terms.accepted.etag)
    /// az aktuális központi ETag-gel. Opcionálisan – csak ha a claim HIÁNYZIK – egy gyors DB-fallbacket is végez.
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
            // 0) Authenticated user kell – ha nincs, nem itt a helye eldönteni
            if (context.User?.Identity?.IsAuthenticated != true)
                return;

            // 1) Aktuális központi ETag
            var currentEtag = _termsProvider.CurrentTermsEtag;
            if (string.IsNullOrWhiteSpace(currentEtag))
                return; // nincs mire ellenőrizni → ne succeed, ne is fail (más handler még érvényesülhet)

            // 2) Claimből olvasás (nincs FirstOrDefault – beépített API-t használunk)
            var claimedEtag = context.User.FindFirst(CustomClaimTypes.TermsAcceptedEtag)?.Value;

            if (!string.IsNullOrEmpty(claimedEtag))
            {
                // 2/a) Claim megvan → pontos egyezést várunk
                if (string.Equals(claimedEtag, currentEtag, StringComparison.Ordinal))
                {
                    context.Succeed(requirement);
                }
                else
                {
                    // Egyértelmű, régi ÁSZF – direkt fail (ne legyen további „mentő” próbálkozás)
                    context.Fail();
                }
                return;
            }

            // 3) Opcionális defense-in-depth: claim HIÁNYZIK → DB fallback (kikapcsolható appsettingsből)
            var enableDbFallback = _config.GetValue<bool>("Auth:TermsPolicy:EnableDbFallback", false);
            if (!enableDbFallback)
                return; // nincs claim → nem succeed, nem is fail (marad 403, ha ez az egyetlen handler)

            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userId))
                return;

            try
            {
                // Utolsó elfogadott Terms verzió lekérdezése – LINQ nélkül indexeléshez igazodva
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
                    // A DB szerint naprakész – succeed (a következő refresh/login frissíti majd a claimet)
                    context.Succeed(requirement);
                }
                else
                {
                    // Nincs egyezés → fail
                    context.Fail();
                }
            }
            catch (Exception ex)
            {
                // Ha bármi gond, nem engedünk át (biztonságos default)
                _logger.LogWarning(ex, "Terms policy DB-fallback hiba. UserId={UserId}", userId);
                context.Fail();
            }
        }
    }
}
