using KvizCommando.Server.Services.Db;
using KvizCommando.Server.Services.PlayerCache;
using KvizCommando.Shared.Contracts.Profile;
using KvizCommando.Shared.Models.Rules;
using Microsoft.AspNetCore.Identity;
using System.Globalization;

namespace KvizCommando.Server.Services.Profile;

public sealed class ProfileService : IProfileService
{
    private static readonly SemaphoreSlim _teamNameGate = new(1, 1);

    private readonly IPlayerCacheService _cache;
    private readonly IPlayerDbService _playerDb;
    private readonly ILookupNormalizer _normalizer;

    public ProfileService(
        IPlayerCacheService cache,
        IPlayerDbService playerDb,
        ILookupNormalizer normalizer)
    {
        _cache = cache;
        _playerDb = playerDb;
        _normalizer = normalizer;
    }

    /// <inheritdoc />
    public async Task<ProfileLoadResponse> GetAsync(
        int playerId,
        string sessionId,
        CancellationToken ct = default)
    {
        var cacheResult = await _cache.GetOrLoadLockedAsync(
            playerId,
            sessionId,
            ct);

        return cacheResult.Status switch
        {
            CacheReadStatus.Success => new ProfileLoadResponse
            {
                State = ProfileRequestState.Success,
                Profile = BuildProfile(cacheResult.Player!)
            },
            CacheReadStatus.SessionMismatch => new ProfileLoadResponse
            {
                State = ProfileRequestState.SessionMismatch
            },
            _ => new ProfileLoadResponse
            {
                State = ProfileRequestState.NotFound
            }
        };
    }

    /// <inheritdoc />
    public async Task<CheckTeamNameResponse> CheckTeamNameAsync(
        int playerId,
        CheckTeamNameRequest request,
        CancellationToken ct = default)
    {
        var cacheResult = await _cache.GetOrLoadLockedAsync(
            playerId,
            request.SessionId,
            ct);

        if (cacheResult.Status != CacheReadStatus.Success)
        {
            return new CheckTeamNameResponse
            {
                State = MapReadState(cacheResult.Status)
            };
        }

        var teamName = request.TeamName?.Trim() ?? string.Empty;
        var validation = MapValidation(PublicNameRules.Validate(teamName));

        if (validation != TeamNameCheckState.Available)
        {
            return new CheckTeamNameResponse
            {
                State = ProfileRequestState.TeamNameRejected,
                TeamNameState = validation,
                CheckedTeamName = teamName
            };
        }

        var normalized = Normalize(teamName);

        await _teamNameGate.WaitAsync(ct);
        try
        {
            var taken = await IsTeamNameTakenAsync(
                normalized,
                playerId,
                ct);

            return new CheckTeamNameResponse
            {
                State = taken
                    ? ProfileRequestState.TeamNameRejected
                    : ProfileRequestState.Success,
                TeamNameState = taken
                    ? TeamNameCheckState.Taken
                    : TeamNameCheckState.Available,
                CheckedTeamName = teamName
            };
        }
        finally
        {
            _teamNameGate.Release();
        }
    }

    /// <inheritdoc />
    public async Task<SaveProfileResponse> SaveTeamNameAsync(
        int playerId,
        SaveTeamNameRequest request,
        CancellationToken ct = default)
    {
        var teamName = request.TeamName?.Trim() ?? string.Empty;
        var validation = MapValidation(PublicNameRules.Validate(teamName));

        if (validation != TeamNameCheckState.Available)
            return TeamNameRejected(validation);

        await _teamNameGate.WaitAsync(ct);
        try
        {
            var cacheResult = await _cache.GetOrLoadLockedAsync(
                playerId,
                request.SessionId,
                ct);

            if (cacheResult.Status != CacheReadStatus.Success)
                return Failed(MapReadState(cacheResult.Status));

            var player = cacheResult.Player!;
            var utcNow = DateTime.UtcNow;

            if (player.Core.RankEnum < ProfileRules.GetTeamNameRequiredRank())
                return Failed(ProfileRequestState.RankLocked, player);

            var nextChangeUtc = ProfileRules.GetNextTeamNameChangeUtc(
                player.Core.TeamNameChangedUtc);

            if (nextChangeUtc.HasValue && nextChangeUtc.Value > utcNow)
                return Failed(ProfileRequestState.CooldownActive, player);

            var normalized = Normalize(teamName);

            if (string.Equals(
                    player.Core.NormalizedTeamName,
                    normalized,
                    StringComparison.Ordinal))
            {
                return Failed(ProfileRequestState.SameValue, player);
            }

            if (await IsTeamNameTakenAsync(normalized, playerId, ct))
                return TeamNameRejected(TeamNameCheckState.Taken, player);

            var updateResult = await _cache.UpdatePlayerLockedAsync(
                playerId,
                request.SessionId,
                cachedPlayer =>
                {
                    if (!ProfileRules.CanChangeTeamName(
                            cachedPlayer.Core.RankEnum,
                            cachedPlayer.Core.TeamNameChangedUtc,
                            utcNow))
                    {
                        return null;
                    }

                    cachedPlayer.Core.TeamName = teamName;
                    cachedPlayer.Core.NormalizedTeamName = normalized;
                    cachedPlayer.Core.TeamNameChangedUtc = utcNow;
                    cachedPlayer.Core.UpdatedUtc = utcNow;
                    return DirtyFlags.Core;
                },
                ct);

            return updateResult switch
            {
                CacheUpdateResult.Updated => new SaveProfileResponse
                {
                    State = ProfileRequestState.Success,
                    TeamNameState = TeamNameCheckState.Available,
                    Profile = BuildProfile(player)
                },
                CacheUpdateResult.SessionMismatch =>
                    Failed(ProfileRequestState.SessionMismatch),
                CacheUpdateResult.NotFound =>
                    Failed(ProfileRequestState.NotFound),
                _ => Failed(ProfileRequestState.ServerError)
            };
        }
        finally
        {
            _teamNameGate.Release();
        }
    }

    /// <inheritdoc />
    public async Task<SaveProfileResponse> SaveAvatarAsync(
        int playerId,
        SaveAvatarRequest request,
        CancellationToken ct = default)
    {
        var avatarNumber = ProfileRules.GetAvatarNumber(
            request.CaptainAvatar);

        var cacheResult = await _cache.GetOrLoadLockedAsync(
            playerId,
            request.SessionId,
            ct);

        if (cacheResult.Status != CacheReadStatus.Success)
            return Failed(MapReadState(cacheResult.Status));

        var player = cacheResult.Player!;

        if (!ProfileRules.CanChangeAvatar(player.Core.RankEnum))
            return Failed(ProfileRequestState.RankLocked, player);

        var avatar = avatarNumber.ToString(CultureInfo.InvariantCulture);
        var utcNow = DateTime.UtcNow;

        var updateResult = await _cache.UpdatePlayerLockedAsync(
            playerId,
            request.SessionId,
            cachedPlayer =>
            {
                if (!ProfileRules.CanChangeAvatar(cachedPlayer.Core.RankEnum))
                    return null;

                cachedPlayer.Core.CaptainAvatar = avatar;
                cachedPlayer.Core.UpdatedUtc = utcNow;
                return DirtyFlags.Core;
            },
            ct);

        return updateResult switch
        {
            CacheUpdateResult.Updated => new SaveProfileResponse
            {
                State = ProfileRequestState.Success,
                Profile = BuildProfile(player)
            },
            CacheUpdateResult.SessionMismatch =>
                Failed(ProfileRequestState.SessionMismatch),
            CacheUpdateResult.NotFound =>
                Failed(ProfileRequestState.NotFound),
            _ => Failed(ProfileRequestState.ServerError)
        };
    }

    private async Task<bool> IsTeamNameTakenAsync(
        string normalizedTeamName,
        int playerId,
        CancellationToken ct) =>
        _cache.IsNormalizedTeamNameInUse(normalizedTeamName, playerId) ||
        await _playerDb.IsNormalizedTeamNameTakenAsync(
            normalizedTeamName,
            playerId,
            ct);

    private string Normalize(string teamName) =>
        _normalizer.NormalizeName(teamName) ?? teamName.ToUpperInvariant();

    private static TeamProfileDto BuildProfile(CachedPlayer player) => new()
    {
        TeamName = player.Core.TeamName,
        CaptainAvatar = ProfileRules.GetAvatarNumber(
            player.Core.CaptainAvatar).ToString(CultureInfo.InvariantCulture),
        RankEnum = player.Core.RankEnum,
        TeamNameRequiredRank = ProfileRules.GetTeamNameRequiredRank(),
        AvatarRequiredRank = ProfileRules.GetAvatarRequiredRank(),
        TeamNameChangedUtc = player.Core.TeamNameChangedUtc,
        NextTeamNameChangeUtc = ProfileRules.GetNextTeamNameChangeUtc(
            player.Core.TeamNameChangedUtc)
    };

    private static ProfileRequestState MapReadState(CacheReadStatus status) =>
        status == CacheReadStatus.SessionMismatch
            ? ProfileRequestState.SessionMismatch
            : ProfileRequestState.NotFound;

    private static TeamNameCheckState MapValidation(
        PublicNameValidationResult validation) =>
        validation switch
        {
            PublicNameValidationResult.Valid => TeamNameCheckState.Available,
            PublicNameValidationResult.Required => TeamNameCheckState.Required,
            PublicNameValidationResult.TooShort => TeamNameCheckState.TooShort,
            PublicNameValidationResult.TooLong => TeamNameCheckState.TooLong,
            PublicNameValidationResult.InvalidCharacters =>
                TeamNameCheckState.InvalidCharacters,
            _ => TeamNameCheckState.InvalidCharacters
        };

    private static SaveProfileResponse Failed(
        ProfileRequestState state,
        CachedPlayer? player = null) => new()
        {
            State = state,
            Profile = player is null ? null : BuildProfile(player)
        };

    private static SaveProfileResponse TeamNameRejected(
        TeamNameCheckState state,
        CachedPlayer? player = null) => new()
        {
            State = ProfileRequestState.TeamNameRejected,
            TeamNameState = state,
            Profile = player is null ? null : BuildProfile(player)
        };
}
