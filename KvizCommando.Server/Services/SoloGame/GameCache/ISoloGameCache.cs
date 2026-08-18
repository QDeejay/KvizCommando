namespace KvizCommando.Server.Services.SoloGame.GameCache;

public interface ISoloGameCache
{
    /// <summary>
    /// Megkísérli létrehozni az aktív egyéni játék cache-bejegyzését.
    /// </summary>
    bool TryCreate(SoloGameSession session);
    /// <summary>
    /// Megkísérli visszaadni a megadott azonosítójú elemet.
    /// </summary>
    bool TryGet(Guid gameId, out SoloGameSession? session);
    /// <summary>
    /// Megkísérli visszaadni a játékos aktív egyéni játékát.
    /// </summary>
    bool TryGetActiveGame(
        int playerId,
        out SoloGameSession? session);
    /// <summary>
    /// Eltávolítja a megadott játékot a gyorsítótárból.
    /// </summary>
    void Remove(Guid gameId);
}
