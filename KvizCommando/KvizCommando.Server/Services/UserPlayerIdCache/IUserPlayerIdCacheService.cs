namespace KvizCommando.Server.Services.UserPlayerIdCache
{
    public interface IUserPlayerIdCacheService
    {
        Task<int?> GetPlayerIdAsync(string userId, CancellationToken ct = default);
        void Invalidate(string userId);
        void Clear();
    }
}
