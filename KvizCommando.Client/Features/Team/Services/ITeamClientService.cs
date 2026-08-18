using KvizCommando.Shared.Contracts.Team;

namespace KvizCommando.Client.Features.Team.Services;

public interface ITeamClientService
{
    /// <summary>
    /// Elmenti a karakter képességpontjain végzett módosítást.
    /// </summary>
    Task<bool> ModifySkillsAsync(
        ModifySkillRequest request,
        CancellationToken ct = default);

    /// <summary>
    /// Végrehajtja a csapaton kért kezelési műveletet.
    /// </summary>
    Task<bool> ManageTeamAsync(
        ManageTeamRequest request,
        CancellationToken ct = default);
}
