using KvizCommando.Server.Identity;
using KvizCommando.Server.Services.PlayerCache;

namespace KvizCommando.Server.Services.Db
{
    public interface IPlayerDbService
    {
        Task<CachedPlayer?> LoadPlayerFromDbAsync(
            int playerId,
            string sessionId,
            CancellationToken ct);
        Task<bool> SavePlayerToDbAsync(
            CachedPlayer player,
            DirtyFlags flags,
            int playerId,
            CancellationToken ct);
        Task<(ApplicationUser?, string?, int?)> LoadCheckinDataFromDbAsync(
            string userId,
            CancellationToken ct);
        Task<(IReadOnlyList<string>, bool success)> SaveDisplayNameToDbAsync(
            ApplicationUser user,
            string displayName,
            CancellationToken ct);

       Task<bool> SaveTermsToDbAsync(
            ApplicationUser user,
            string acceptedTerms,
            string currentTerms,
            DateTime acceptedAt,
            CancellationToken ct);

        Task CreatePlayerToDbAsync(
            string userId,
            string displayname,
            string teamname,
            CancellationToken ct);
        Task<string> SuggestAsync(
            string? rawName,
            CancellationToken ct = default);
    }
}
