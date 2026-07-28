using System.Collections.Concurrent;

namespace KvizCommando.Server.Services.VsGame.Match;

public sealed class VsMatchStore
{
    private readonly ConcurrentDictionary<Guid, VsMatchSession> _matches = [];
    private readonly ConcurrentDictionary<string, Guid> _connectionMatches = [];
    private readonly ConcurrentDictionary<int, Guid> _playerMatches = [];

    public bool TryAdd(VsMatchSession match)
    {
        if (!_matches.TryAdd(match.MatchId, match))
            return false;

        var addedConnections = new List<string>();
        var addedPlayers = new List<int>();

        foreach (var player in match.Players)
        {
            var connectionAdded = _connectionMatches.TryAdd(
                player.ConnectionId,
                match.MatchId);
            var playerAdded = connectionAdded &&
                              _playerMatches.TryAdd(
                                  player.PlayerId,
                                  match.MatchId);

            if (!connectionAdded || !playerAdded)
            {
                foreach (var connectionId in addedConnections)
                    _connectionMatches.TryRemove(connectionId, out _);

                foreach (var playerId in addedPlayers)
                    _playerMatches.TryRemove(playerId, out _);

                if (connectionAdded)
                {
                    _connectionMatches.TryRemove(
                        player.ConnectionId,
                        out _);
                }

                _matches.TryRemove(match.MatchId, out _);
                return false;
            }

            addedConnections.Add(player.ConnectionId);
            addedPlayers.Add(player.PlayerId);
        }

        return true;
    }

    public bool ContainsPlayer(int playerId) =>
        _playerMatches.ContainsKey(playerId);

    public bool TryGet(Guid matchId, out VsMatchSession? match) =>
        _matches.TryGetValue(matchId, out match);

    public bool TryGetByConnection(
        string connectionId,
        out VsMatchSession? match)
    {
        match = null;

        return _connectionMatches.TryGetValue(
                   connectionId,
                   out var matchId) &&
               _matches.TryGetValue(matchId, out match);
    }

    public bool TryRemove(Guid matchId, out VsMatchSession? match)
    {
        if (!_matches.TryRemove(matchId, out match))
            return false;

        foreach (var player in match.Players)
        {
            _connectionMatches.TryRemove(player.ConnectionId, out _);
            _playerMatches.TryRemove(player.PlayerId, out _);
        }

        match.Dispose();
        return true;
    }
}

/**
 * A futó VS meccsek és a connection/player hozzárendelések
 * folyamaton belüli, konkurens tárolója.
 */
