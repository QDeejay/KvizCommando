using KvizCommando.Shared.Contracts.VsGame;

namespace KvizCommando.Client.Features.VsGame.Services;

public interface IVsGameClientService
{
    /// <summary>
    /// Elmenti a rangsorolt játékhoz összeállított csapatot.
    /// </summary>
    /// <param name="request">A rangsorolt játékhoz kiválasztott karakterhelyek.</param>
    /// <param name="ct">A művelet megszakítását jelző token.</param>
    /// <returns><see langword="true"/>, ha a művelet sikeresen befejeződött; egyébként <see langword="false"/>.</returns>
    Task<bool> SaveBattleTeamAsync(
        SaveBattleTeamRequest request,
        CancellationToken ct = default);
}
