using KvizCommando.Shared.Contracts.VsGame;

namespace KvizCommando.Client.Features.VsGame.Services;

public interface IVsGameClientService
{
    /// <summary>
    /// Elmenti a rangsorolt játékhoz összeállított csapatot.
    /// </summary>
    Task<bool> SaveBattleTeamAsync(
        SaveBattleTeamRequest request,
        CancellationToken ct = default);
}
