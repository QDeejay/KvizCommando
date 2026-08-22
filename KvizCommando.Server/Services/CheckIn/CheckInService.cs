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
    /// A játékosnév megadását és az ÁSZF elfogadását kezelő beléptetési szolgáltatás.
    /// Az elfogadás append-only auditbejegyzést és claimfrissítést hoz létre;
    /// személyes adat nem kerül a cookie-ba vagy a bearer tokenbe.
    /// </summary>
    public sealed class CheckInService : ICheckInService
    {
        private readonly IPlayerDbService _playerDb;
        private readonly ITermsProvider _termsProvider;
        private readonly ILogger<CheckInService> _logger;
        private readonly IPlayerCacheService _cacheService;
        private readonly IClaimsSyncService _claimsSync;
        private readonly IVsRankedQueueService _rankedQueue;
        private readonly IVsMatchService _vsMatch;
        private readonly IStringLocalizer<CheckInService> _localizer;

        public CheckInService(
            IPlayerDbService playerdb,
            ITermsProvider termsProvider,
            ILogger<CheckInService> logger,
            IPlayerCacheService cacheService,
            IClaimsSyncService claimsSync,
            IVsRankedQueueService rankedQueue,
            IVsMatchService vsMatch,
            IStringLocalizer<CheckInService> localizer)

        {
            _playerDb = playerdb;
            _termsProvider = termsProvider;
            _logger = logger;
            _claimsSync = claimsSync;
            _cacheService = cacheService;
            _rankedQueue = rankedQueue;
            _vsMatch = vsMatch;
            _localizer = localizer;
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
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

            var providedName = request.DisplayName?.Trim();

            var suggested = string.Empty;
            if (needsDisplayName || !string.IsNullOrEmpty(providedName))
            {
                foreach (var code in DisplayNameValidator.Validate(providedName))
                    errorKeys.Add(code);

                // Az egyediség csak formailag érvényes névnél ellenőrizhető.
                if (errorKeys.Count == 0 && !string.IsNullOrEmpty(providedName))
                {
                    suggested = await _playerDb.SuggestAsync(providedName, ct);
                    if (providedName != suggested)
                    {
                        errorKeys.Add(IdentityErrorCodes.DISPLAY_NAME_ALREADY_TAKEN);
                    }


                }
            }

            if (needsTermsAcceptance)
            {
                if (string.IsNullOrWhiteSpace(request.AcceptedTermsVersion))
                {
                    errorKeys.Add(IdentityErrorCodes.TERMS_NOT_ACCEPTED);
                }
                else if (!string.Equals(request.AcceptedTermsVersion, currentTerms.Version, StringComparison.Ordinal))
                {
                    // A GET és POST között megváltozott ÁSZF-et nem szabad elfogadottnak tekinteni.
                    errorKeys.Add(IdentityErrorCodes.TERMS_VERSION_OUTDATED);
                }
            }

            if (errorKeys.Count > 0)
                return (errorKeys, suggested, false);

            if (!string.IsNullOrEmpty(providedName) &&
                !string.Equals(_user.DisplayName, providedName, StringComparison.Ordinal))
            {
                var result = await _playerDb.SaveDisplayNameToDbAsync(_user, providedName, ct);
                if (!result.success)
                    return (result.Item1, suggested, false);
            }

            var acceptedAtUtc = DateTime.UtcNow;
            // Az egyedi adatbázis-kulcs az ÁSZF-elfogadás ismételt beszúrását idempotenssé teszi.
            if (needsTermsAcceptance &&
                string.Equals(request.AcceptedTermsVersion, currentTerms.Version, StringComparison.Ordinal))
            {
                await _playerDb.SaveTermsToDbAsync(_user, request.AcceptedTermsVersion, currentTerms.Version, acceptedAtUtc, ct);
            }

            await _claimsSync.UpsertTermsClaimsAsync(_user, currentTerms.Version, acceptedAtUtc, ct);

            // A játékosrekord első beléptetéskor, az elfogadott adatok mentése után jön létre.
            var playerId = _playerId ?? 0;
            if (providedName != null && needsDisplayName == true)
            {
                var displayName = providedName;
                var teamName = providedName + _localizer["team.Append"]; ;

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
