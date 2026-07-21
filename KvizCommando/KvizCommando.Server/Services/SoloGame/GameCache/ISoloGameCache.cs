namespace KvizCommando.Server.Services.SoloGame.GameCache;

public interface ISoloGameCache
{
    bool TryCreate(SoloGameSession session);
    bool TryGet(Guid gameId, out SoloGameSession? session);
    bool HasActiveGame(int playerId, string sessionId);
    void Remove(Guid gameId);
}
