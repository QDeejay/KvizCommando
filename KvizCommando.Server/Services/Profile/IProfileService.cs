using KvizCommando.Shared.Contracts.Profile;

namespace KvizCommando.Server.Services.Profile;

public interface IProfileService
{
    Task<ProfileLoadResponse> GetAsync(
        int playerId,
        string sessionId,
        CancellationToken ct = default);

    Task<CheckTeamNameResponse> CheckTeamNameAsync(
        int playerId,
        CheckTeamNameRequest request,
        CancellationToken ct = default);

    Task<SaveProfileResponse> SaveTeamNameAsync(
        int playerId,
        SaveTeamNameRequest request,
        CancellationToken ct = default);

    Task<SaveProfileResponse> SaveAvatarAsync(
        int playerId,
        SaveAvatarRequest request,
        CancellationToken ct = default);
}
