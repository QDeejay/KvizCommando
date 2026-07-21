using KvizCommando.Server.Domain.Entities.Players;
using KvizCommando.Server.Domain.Entities.Statistics;
using KvizCommando.Server.Models;
using KvizCommando.Shared.Contracts.Question;
using KvizCommando.Shared.Contracts.Team;

namespace KvizCommando.Server.Services.PlayerCache
{
    public interface IPlayerCacheService
    {

        IReadOnlyCollection<int> GetActivePlayerIds();
        Task<(CachedPlayer?, CachedQuestion?)> GetOrLoadLockedAsync(
            int playerId,
            string sessionId,
            CancellationToken ct = default);

        Task<bool> NewSessionCheckLockedAsync(
            int playerId,
            string sessionId,
            CancellationToken ct = default);
        Task<bool?> UpdatePartialCharachtersAsync(
            int playerId,
            string sessionId,
            ManageTeamRequest teamReq,
            RecruitSlot[]? recruitSlots = null,
            CancellationToken ct = default
            );
        Task<bool?> UpdatePartialModifySillsLockedAsync(
           int playerId,
           string sessionId,
           string newhelpdata,
           ModifySkillRequest newskilldata,
           CancellationToken ct = default);
        Task<bool?> UpdatePartialQuestionsLockedAsync(
            int playerId,
            string sessionId,
            ManageSlotRequest slotReq,
            CancellationToken ct = default);
        Task<bool?> UpdatePartialNewQuestionLockedAsync(
            int playerId,
            string sessionId,
            NewQuestionRequest slotReq,
            CancellationToken ct = default);
        Task<bool?> UpdatePartialLoadoutLockedAsync(
           int playerId,
           string sessionId,
           PlayerLoadout newLoadout,
           CancellationToken ct = default);

        Task<bool?> UpdatePartialAskStatsLockedAsync(
            int playerId,
            string sessionId,
            PlayerAskStats newStats,
            CancellationToken ct = default);
        Task<bool?> UpdatePartialCategoryStatsLockedAsync(
            int playerId,
            string sessionId,
            List<PlayerCategoryStat> newStats,
            CancellationToken ct = default);
        Task<bool?> UpdatePartialOrientStatsLockedAsync(
            int playerId,
            string sessionId,
            List<PlayerOrientStat> newStats,
            CancellationToken ct = default);

        Task<bool?> UpdatePartialPlayerAsync(
            int playerId,
            string sessionId,
            int xp, int devpoints,
            int selectedid,
            int memXp, int memDev,
            int credit,
            CancellationToken ct = default
            );



        Task<bool?> UpdateAllLockedAsync(
            int playerId,
            string sessionId,
            CachedPlayer updated,
            CachedQuestion updatedQ,
            CancellationToken ct = default);
        Task<bool?> LogoutLockedRequestAsync(
            int playerId,
            string sessionId,
            CancellationToken ct = default);
        Task<(SaveResult, bool)> SaveDirtyLockedAsync(int playerId, CancellationToken ct = default);

        Task<int> SaveDirtyQuestionLockedAsync(int playerId, CancellationToken ct = default);

    }
}
