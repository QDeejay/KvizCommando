using KvizCommando.Shared.Contracts.VsGame;

namespace KvizCommando.Server.Services.VsGame
{
    public interface IVsGameService
    {
        Task<bool?> SaveBattleTeamAsync(
            int playerId,
            SaveBattleTeamRequest request,
            CancellationToken ct = default);
    }
}
