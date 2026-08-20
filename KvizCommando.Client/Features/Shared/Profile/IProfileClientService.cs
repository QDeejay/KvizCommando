using KvizCommando.Shared.Contracts.Profile;

namespace KvizCommando.Client.Features.Shared.Profile;

public interface IProfileClientService
{
    Task<ProfileLoadResponse> GetAsync(CancellationToken ct = default);

    Task<CheckTeamNameResponse> CheckTeamNameAsync(
        string teamName,
        CancellationToken ct = default);

    Task<SaveProfileResponse> SaveTeamNameAsync(
        string teamName,
        CancellationToken ct = default);

    Task<SaveProfileResponse> SaveAvatarAsync(
        string captainAvatar,
        CancellationToken ct = default);
}
