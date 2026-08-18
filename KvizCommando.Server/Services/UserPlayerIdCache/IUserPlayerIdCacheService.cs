namespace KvizCommando.Server.Services.UserPlayerIdCache
{
    public interface IUserPlayerIdCacheService
    {
        /// <summary>
        /// Visszaadja az Identity-felhasználóhoz tartozó játékosazonosítót,
        /// szükség esetén adatbázisból betöltve és 30 percre gyorsítótárazva.
        /// </summary>
        /// <param name="userId">Az Identity-felhasználó azonosítója.</param>
        /// <param name="ct">A művelet megszakítását jelző token.</param>
        Task<int?> GetPlayerIdAsync(string userId, CancellationToken ct = default);
        /// <summary>
        /// Eltávolítja a megadott felhasználó játékosazonosítóját a gyorsítótárból.
        /// </summary>
        /// <param name="userId">Az Identity-felhasználó azonosítója.</param>
        void Invalidate(string userId);
        /// <summary>
        /// Kiüríti a teljes felhasználó–játékos leképezési gyorsítótárat.
        /// </summary>
        void Clear();
    }
}
