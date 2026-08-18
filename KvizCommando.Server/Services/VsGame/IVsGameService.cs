using KvizCommando.Shared.Contracts.VsGame;
using KvizCommando.Server.Services.PlayerCache;

namespace KvizCommando.Server.Services.VsGame
{
    public interface IVsGameService
    {
        /// <summary>
        /// Elmenti a rangsorolt játékhoz összeállított csapatot.
        /// </summary>
        /// <param name="playerId">A játékos adatbázis-azonosítója.</param>
        /// <param name="request">A rangsorolt játékhoz kiválasztott karakterhelyek.</param>
        /// <param name="ct">A művelet megszakítását jelző token.</param>
        Task<CacheUpdateResult> SaveBattleTeamAsync(
            int playerId,
            SaveBattleTeamRequest request,
            CancellationToken ct = default);
    }
}
