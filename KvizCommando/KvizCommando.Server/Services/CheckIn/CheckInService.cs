#nullable enable
using KvizCommando.Server.Domain.Entities.Compliance; // TermsConsent
using KvizCommando.Server.Domain.Entities.Players;
using KvizCommando.Server.Domain.Entities.Statistics;
using KvizCommando.Server.Identity;                   // IdentityErrorCodes, CheckInValidationOptions, DisplayNameValidator, ApplicationUser (ha itt van)
using KvizCommando.Server.Infrastructure.Persistence; // ApplicationDbContext
using KvizCommando.Server.Resources;
using KvizCommando.Server.Services.Auth;             // IClaimsSyncService
using KvizCommando.Server.Services.Db;
using KvizCommando.Server.Services.PlayerCache;
using KvizCommando.Shared.Contracts.CheckIn;          // CheckInGetResponse, CheckInPostRequest
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace KvizCommando.Server.Services.CheckIn
{
    /// <summary>
    /// Check-in üzleti logika: DisplayName + Terms (ÁSZF).
    /// - Nincs extra DTO a POST-hoz; hiba esetén kulcsok listája tér vissza (ProblemDetails.Errors kulcsaiként megy tovább).
    /// - Audit: TermsConsent táblába append-only sor; GDPR-minimum (UA/IP HMAC), tokenbe/cookie-ba nem kerül PII.
    /// - Claim-szinkron: AspNetUserClaims upsert a UserManager-rel; cookie esetén RefreshSignIn, bearer/opaque esetén kliens /refresh.
    /// </summary>
    public sealed class CheckInService : ICheckInService
    {
        private readonly IPlayerDbService _playerDb;
        private readonly ITermsProvider _termsProvider;
        private readonly ILogger<CheckInService> _logger;
        private readonly IStringLocalizer<CheckInService> _localizer;
        private readonly IPlayerCacheService _cacheService;
        private readonly IClaimsSyncService _claimsSync;

        public CheckInService(
            IPlayerDbService playerdb,
            ITermsProvider termsProvider,
            ILogger<CheckInService> logger,
            IPlayerCacheService cacheService,
            IStringLocalizer<CheckInService> localizer,
            IClaimsSyncService claimsSync)

        {
            _playerDb = playerdb;
            _termsProvider = termsProvider;
            _logger = logger;
            _localizer = localizer;
            _claimsSync = claimsSync;
            _cacheService = cacheService;
        }

        public async Task<CheckInGetResponse> GetStatusAsync(string userId, CancellationToken ct)
        {
            // csak a szükséges mezők
            var sessionId = "Teszt";
            /*
            var user = await _userManager.Users
                .Where(u => u.Id == userId)
                .Select(u => new { u.DisplayName })
                .SingleAsync(ct);
            var playerId = await _db.Players
                .Where(p => p.UserId == userId)
                .Select(p => p.PlayerId)
                .FirstOrDefaultAsync(ct);
            

            var lastAcceptedVersion = await _db.Set<TermsConsent>()
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.AcceptedAtUtc)
                .Select(x => x.TermsVersion)
                .FirstOrDefaultAsync(ct);
            */
            var response = await _playerDb.LoadCheckinDataFromDbAsync(userId, ct);
            var _user = response.Item1;
            string? _lastAcceptedTerms = response.Item2;
            int? _playerId = response.Item3;

            var currentTerms = _termsProvider.GetCurrentTerms();
            var requiredDispName = string.IsNullOrWhiteSpace(_user.DisplayName);
            var requiredTerms = string.IsNullOrWhiteSpace(_lastAcceptedTerms)
                                       || !string.Equals(_lastAcceptedTerms, currentTerms.Version, StringComparison.Ordinal);
            var success = (requiredDispName == false && requiredTerms == false);
            if (success==true && _playerId != null) 
                {
                    await _cacheService.NewSessionCheckLockedAsync(_playerId ?? 0, sessionId);
                }


            return new CheckInGetResponse
            {
                Success = success,
                NeedsDisplayName = requiredDispName,
                NeedsTermsAcceptance = requiredTerms,
                CurrentTerms = currentTerms
            };
        }

        public async Task<(IReadOnlyList<string>,string Suggested)> CompleteAsync(string userId, CheckInPostRequest request, CancellationToken ct)
        {
            var sessionId = "Teszt";
            var errorKeys = new List<string>();
            var currentTerms = _termsProvider.GetCurrentTerms();

            var response = await _playerDb.LoadCheckinDataFromDbAsync(userId, ct);
            var _user = response.Item1;
            string? _lastAcceptedTerms = response.Item2;
            int? _playerId = response.Item3;

            var needsDisplayName = string.IsNullOrWhiteSpace(_user.DisplayName);
            var needsTermsAcceptance = string.IsNullOrWhiteSpace(_lastAcceptedTerms)
                                       || !string.Equals(_lastAcceptedTerms, currentTerms.Version, StringComparison.Ordinal);

            // 3) DISPLAY NAME validáció (ha kell, vagy ha küldött újat)
            var providedName = request.DisplayName?.Trim();
            
            var suggested = string.Empty;
            if (needsDisplayName || !string.IsNullOrEmpty(providedName))
            {
                // formátum/szabályok a központi validatorból
                foreach (var code in DisplayNameValidator.Validate(providedName))
                  errorKeys.Add(code);

                // egyediség (case-insensitive) csak akkor, ha formailag átment
                if (errorKeys.Count == 0 && !string.IsNullOrEmpty(providedName))
                {
                    // Identity-konform normalizálás (ugyanazt használjuk, mint UserName/Email esetén)
                    //var norm = _normalizer.NormalizeName(providedName);

                    // Egyediség ellenőrzés a NORMALIZÁLT mezőn
                    //var taken = await _userManager.Users
                      //  .AnyAsync(u => u.Id != userId
                        //           && u.NormalizedDisplayName == norm, ct);
                    suggested = await _playerDb.SuggestAsync(providedName, ct);
                    if (providedName != suggested)
                    {
                        errorKeys.Add(IdentityErrorCodes.DisplayNameAlreadyTaken);
                    }
                   
                        
                }
            }

            // 4) TERMS validáció (ha kell)
            if (needsTermsAcceptance)
            {
                if (string.IsNullOrWhiteSpace(request.AcceptedTermsVersion))
                {
                    errorKeys.Add(IdentityErrorCodes.TermsNotAccepted);
                }
                else if (!string.Equals(request.AcceptedTermsVersion, currentTerms.Version, StringComparison.Ordinal))
                {
                    // GET→POST közben frissült a Terms
                    errorKeys.Add(IdentityErrorCodes.TermsVersionOutdated);
                }
            }

            // 5) Ha van hiba, visszaadjuk a kulcsokat (endpoint 400/409 ProblemDetails-t fog csinálni belőle)
            if (errorKeys.Count > 0)
                return (errorKeys,suggested);

            // 6) MENTÉSEK

            // 6/a) DisplayName frissítése (ha küldött és változik)
            if (!string.IsNullOrEmpty(providedName) &&
                !string.Equals(_user.DisplayName, providedName, StringComparison.Ordinal))
            {
                var result = await _playerDb.SaveDisplayNameToDbAsync(_user, providedName, ct);
                if (!result.success)
                    return (result.Item1, suggested);
            }

            var acceptedAtUtc = DateTime.UtcNow;
            // 6/b) Terms elfogadás beszúrása (idempotens a (UserId, TermsVersion) pároson)
            if (needsTermsAcceptance &&
                string.Equals(request.AcceptedTermsVersion, currentTerms.Version, StringComparison.Ordinal))
            {
                await _playerDb.SaveTermsToDbAsync(_user, request.AcceptedTermsVersion, currentTerms.Version,acceptedAtUtc, ct);
            }

            // --- 6/b/2) User-claim upsert + (ha cookie) RefreshSignIn ---
            await _claimsSync.UpsertTermsClaimsAsync(_user, currentTerms.Version, acceptedAtUtc, ct);
            // 6/c) Player ENSURE (ha nincs, létrehozunk egyet; versenyhelyzet-biztos)
            if (providedName != null && needsDisplayName==true)
            {
                //var culture = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
                var DisplayName = providedName;
                var TeamName = providedName + _localizer["team.Append"];

                //wait EnsurePlayerExistsAsync(userId, DisplayName, TeamName, ct);
                await _playerDb.CreatePlayerToDbAsync(userId, DisplayName, TeamName, ct);
            }
          
            return (Array.Empty<string>(),"");
        }

        private static string SessionIdGen()
        { 
            var sb = new StringBuilder();
            return string.Empty;
        }

    }
}


