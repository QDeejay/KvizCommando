using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;

namespace KvizCommando.Server.Services.SoloGame.GameCache;

public sealed class SoloGameCache : ISoloGameCache
{
    private readonly IMemoryCache _cache;
    private readonly ConcurrentDictionary<int, Guid> _playerGames = new();
    public SoloGameCache(IMemoryCache cache) => _cache = cache;

    public bool TryCreate(SoloGameSession session)
    {
        if (!_playerGames.TryAdd(session.PlayerId, session.GameId)) return false;
        var options = new MemoryCacheEntryOptions { AbsoluteExpiration = session.ExpiresAtUtc };
        options.RegisterPostEvictionCallback((_, value, _, _) =>
        {
            if (value is SoloGameSession game)
                RemovePlayerGame(game.PlayerId, game.GameId);
        });
        _cache.Set(CacheKey(session.GameId), session, options);
        return true;
    }

    public bool TryGet(Guid gameId, out SoloGameSession? session)
        => _cache.TryGetValue(CacheKey(gameId), out session);

    public bool TryGetActiveGame(
     int playerId,
     string sessionId,
     out SoloGameSession? session)
    {
        session = null;

        if (!_playerGames.TryGetValue(playerId, out var gameId))
            return false;

        if (TryGet(gameId, out var game) &&
            game is not null &&
            game.SessionId == sessionId &&
            game.Status == SoloGameStatus.Active)
        {
            session = game;
            return true;
        }

        RemovePlayerGame(playerId, gameId);
        return false;
    }

    public void Remove(Guid gameId)
    {
        if (TryGet(gameId, out var game) && game is not null)
            RemovePlayerGame(game.PlayerId, gameId);
        _cache.Remove(CacheKey(gameId));
    }
    private static string CacheKey(Guid id) => $"solo-game:{id:N}";
    private void RemovePlayerGame(int playerId, Guid gameId)
        => ((ICollection<KeyValuePair<int, Guid>>)_playerGames)
            .Remove(new KeyValuePair<int, Guid>(playerId, gameId));
}
