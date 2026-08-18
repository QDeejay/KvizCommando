namespace KvizCommando.Server.Services.SoloGame.GameCache;

public interface ISoloGameCache
{
    /// <summary>
    /// Megkísérli létrehozni az aktív egyéni játék cache-bejegyzését.
    /// </summary>
    /// <param name="session">A gyorsítótárba felveendő egyéni játékmenet.</param>
    /// <returns><see langword="true"/>, ha a játékmenet új bejegyzésként bekerült a gyorsítótárba.</returns>
    bool TryCreate(SoloGameSession session);
    /// <summary>
    /// Visszakeresi az egyéni játékmenetet a játékazonosító alapján.
    /// </summary>
    /// <param name="gameId">Az aktív egyéni játék azonosítója.</param>
    /// <param name="session">A gyorsítótárba felveendő egyéni játékmenet.</param>
    /// <returns><see langword="true"/>, ha a játékmenet megtalálható.</returns>
    bool TryGet(Guid gameId, out SoloGameSession? session);
    /// <summary>
    /// Megkísérli visszaadni a játékos aktív egyéni játékát.
    /// </summary>
    /// <param name="playerId">A játékos adatbázis-azonosítója.</param>
    /// <param name="session">A gyorsítótárba felveendő egyéni játékmenet.</param>
    /// <returns><see langword="true"/>, ha a játékoshoz tartozik aktív játékmenet.</returns>
    bool TryGetActiveGame(
        int playerId,
        out SoloGameSession? session);
    /// <summary>
    /// Eltávolítja a megadott játékot a gyorsítótárból.
    /// </summary>
    /// <param name="gameId">Az aktív egyéni játék azonosítója.</param>
    void Remove(Guid gameId);
}
