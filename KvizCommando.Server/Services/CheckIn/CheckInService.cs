#nullable enable
using KvizCommando.Server.Identity;                   // IdentityErrorCodes, CheckInValidationOptions, DisplayNameValidator, ApplicationUser (ha itt van)
using KvizCommando.Server.Services.Auth;             // IClaimsSyncService
using KvizCommando.Server.Services.Db;
using KvizCommando.Server.Services.PlayerCache;
using KvizCommando.Server.Services.VsGame.Match;
using KvizCommando.Server.Services.VsGame.Matchmaking;
using KvizCommando.Shared.Contracts.CheckIn;          // CheckInGetResponse, CheckInPostRequest
using Microsoft.Extensions.Localization;

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
        private readonly IVsRankedQueueService _rankedQueue;
        private readonly IVsMatchService _vsMatch;

        public CheckInService(
            IPlayerDbService playerdb,
            ITermsProvider termsProvider,
            ILogger<CheckInService> logger,
            IPlayerCacheService cacheService,
            IStringLocalizer<CheckInService> localizer,
            IClaimsSyncService claimsSync,
            IVsRankedQueueService rankedQueue,
            IVsMatchService vsMatch)

        {
            _playerDb = playerdb;
            _termsProvider = termsProvider;
            _logger = logger;
            _localizer = localizer;
            _claimsSync = claimsSync;
            _cacheService = cacheService;
            _rankedQueue = rankedQueue;
            _vsMatch = vsMatch;
        }

        public async Task<CheckInGetResponse> GetStatusAsync(string userId, string sessionid, CancellationToken ct)
        {
            var response = await _playerDb.LoadCheckinDataFromDbAsync(userId, ct);
            var _user = response.Item1;
            string? _lastAcceptedTerms = response.Item2;
            var playerId = response.Item3;

            var currentTerms = _termsProvider.GetCurrentTerms();
            var requiredDispName = string.IsNullOrWhiteSpace(_user.DisplayName);
            var requiredTerms = string.IsNullOrWhiteSpace(_lastAcceptedTerms)
                                       || !string.Equals(_lastAcceptedTerms, currentTerms.Version, StringComparison.Ordinal);
            var success = (requiredDispName == false && requiredTerms == false);
            var previousSessionReplaced = false;
            if (success && playerId is not null)
            {
                previousSessionReplaced = await CompleteSessionAsync(
                    playerId.Value,
                    sessionid,
                    ct);
            }


            return new CheckInGetResponse
            {
                Success = success,
                NeedsDisplayName = requiredDispName,
                NeedsTermsAcceptance = requiredTerms,
                CurrentTerms = currentTerms,
                PreviousSessionReplaced = previousSessionReplaced
            };
        }

        public async Task<(IReadOnlyList<string> Errors, string Suggested, bool PreviousSessionReplaced)> CompleteAsync(
            string userId,
            CheckInPostRequest request,
            CancellationToken ct)
        {
            var sessionId = request.SessionId ?? string.Empty;
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

                    // Egyediség ellenőrzés a NORMALIZÁLT mezőn
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
                return (errorKeys, suggested, false);

            // 6) MENTÉSEK

            // 6/a) DisplayName frissítése (ha küldött és változik)
            if (!string.IsNullOrEmpty(providedName) &&
                !string.Equals(_user.DisplayName, providedName, StringComparison.Ordinal))
            {
                var result = await _playerDb.SaveDisplayNameToDbAsync(_user, providedName, ct);
                if (!result.success)
                    return (result.Item1, suggested, false);
            }

            var acceptedAtUtc = DateTime.UtcNow;
            // 6/b) Terms elfogadás beszúrása (idempotens a (UserId, TermsVersion) pároson)
            if (needsTermsAcceptance &&
                string.Equals(request.AcceptedTermsVersion, currentTerms.Version, StringComparison.Ordinal))
            {
                await _playerDb.SaveTermsToDbAsync(_user, request.AcceptedTermsVersion, currentTerms.Version, acceptedAtUtc, ct);
            }

            // --- 6/b/2) User-claim upsert + (ha cookie) RefreshSignIn ---
            await _claimsSync.UpsertTermsClaimsAsync(_user, currentTerms.Version, acceptedAtUtc, ct);
            // 6/c) Player ENSURE (ha nincs, létrehozunk egyet; versenyhelyzet-biztos)
            var playerId = _playerId ?? 0;
            if (providedName != null && needsDisplayName == true)
            {
                var displayName = providedName;
                var teamName = providedName + _localizer["team.Append"];

                if (playerId == 0)
                {
                    playerId = await _playerDb.CreatePlayerToDbAsync(
                        userId,
                        displayName,
                        teamName,
                        ct);
                }
            }
            var previousSessionReplaced = await CompleteSessionAsync(
                playerId,
                sessionId,
                ct);

            return (Array.Empty<string>(), "", previousSessionReplaced);
        }

        private async Task<bool> CompleteSessionAsync(
            int playerId,
            string sessionId,
            CancellationToken ct)
        {
            var previousSessionReplaced =
                await _cacheService.NewSessionCheckLockedAsync(
                    playerId,
                    sessionId,
                    ct);

            await _rankedQueue.LeavePlayerAsync(playerId, ct);
            await _vsMatch.DisconnectPlayerAsync(playerId, ct);

            return previousSessionReplaced;
        }



    }
}
