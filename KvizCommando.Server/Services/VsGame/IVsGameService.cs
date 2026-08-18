using KvizCommando.Shared.Contracts.VsGame;
using KvizCommando.Server.Services.PlayerCache;

namespace KvizCommando.Server.Services.VsGame
{
    public interface IVsGameService
    {
        Task<CacheUpdateResult> SaveBattleTeamAsync(
            int playerId,
            SaveBattleTeamRequest request,
            CancellationToken ct = default);
    }
}
