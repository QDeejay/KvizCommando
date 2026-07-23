namespace KvizCommando.Server.Services.SoloGame.GameCache;

public interface ISoloGameCache
{
    bool TryCreate(SoloGameSession session);
    bool TryGet(Guid gameId, out SoloGameSession? session);
    bool TryGetActiveGame(
    int playerId,
    string sessionId,
    out SoloGameSession? session);
    void Remove(Guid gameId);
}
