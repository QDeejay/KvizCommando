using KvizCommando.Shared.Contracts.VsGame;

namespace KvizCommando.Client.Features.VsGame.Services;

public interface IVsGameClientService
{
    Task<bool> SaveBattleTeamAsync(
        SaveBattleTeamRequest request,
        CancellationToken ct = default);
}
