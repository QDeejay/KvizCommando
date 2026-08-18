namespace KvizCommando.Server.Services.UserPlayerIdCache
{
    public interface IUserPlayerIdCacheService
    {
        /// <summary>
        /// Visszaadja az Identity-felhasználóhoz tartozó játékosazonosítót,
        /// szükség esetén adatbázisból betöltve és 30 percre gyorsítótárazva.
        /// </summary>
        Task<int?> GetPlayerIdAsync(string userId, CancellationToken ct = default);
        /// <summary>
        /// Eltávolítja a megadott felhasználó játékosazonosítóját a gyorsítótárból.
        /// </summary>
        void Invalidate(string userId);
        /// <summary>
        /// Kiüríti a teljes felhasználó–játékos leképezési gyorsítótárat.
        /// </summary>
        void Clear();
    }
}
