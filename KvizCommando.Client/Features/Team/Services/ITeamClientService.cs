using KvizCommando.Shared.Contracts.Team;

namespace KvizCommando.Client.Features.Team.Services;

public interface ITeamClientService
{
    /// <summary>
    /// Elmenti a karakter képességpontjain végzett módosítást.
    /// </summary>
    /// <param name="request">A karakter és a módosítandó képesség adatai.</param>
    /// <param name="ct">A művelet megszakítását jelző token.</param>
    /// <returns><see langword="true"/>, ha a művelet sikeresen befejeződött; egyébként <see langword="false"/>.</returns>
    Task<bool> ModifySkillsAsync(
        ModifySkillRequest request,
        CancellationToken ct = default);

    /// <summary>
    /// Végrehajtja a csapaton kért kezelési műveletet.
    /// </summary>
    /// <param name="request">A csapaton végrehajtandó művelet adatai.</param>
    /// <param name="ct">A művelet megszakítását jelző token.</param>
    /// <returns><see langword="true"/>, ha a művelet sikeresen befejeződött; egyébként <see langword="false"/>.</returns>
    Task<bool> ManageTeamAsync(
        ManageTeamRequest request,
        CancellationToken ct = default);
}
