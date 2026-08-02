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

    public bool TryGetByPlayer(
        int playerId,
        out VsMatchSession? match)
    {
        match = null;

        return _playerMatches.TryGetValue(playerId, out var matchId) &&
               _matches.TryGetValue(matchId, out match);
    }

    public void ReleasePlayer(VsMatchSession match, VsMatchPlayerState player)
    {
        if (_connectionMatches.TryGetValue(
                player.ConnectionId,
                out var connectionMatchId) &&
            connectionMatchId == match.MatchId)
        {
            _connectionMatches.TryRemove(player.ConnectionId, out _);
        }

        if (_playerMatches.TryGetValue(
                player.PlayerId,
                out var playerMatchId) &&
            playerMatchId == match.MatchId)
        {
            _playerMatches.TryRemove(player.PlayerId, out _);
        }
    }

    public (int PlayerId, int ClassificationId)[]
        GetConnectedPlayers()
    {
        var matches = _matches.Values.ToArray();
        var players =
            new List<(int PlayerId, int ClassificationId)>();

        foreach (var match in matches)
        {
            lock (match.SyncRoot)
            {
                if (match.IsClosed)
                    continue;

                players.AddRange(
                    match.Players
                        .Where(player => player.IsConnected)
                        .Select(player => (
                            player.PlayerId,
                            match.Classification.ClassificationId)));
            }
        }

        return [.. players];
    }

    public bool TryRemove(Guid matchId, out VsMatchSession? match)
    {
        match = null;

        if (!_matches.TryGetValue(matchId, out var current))
            return false;

        lock (current.SyncRoot)
        {
            if (!_matches.TryRemove(matchId, out match) ||
                !ReferenceEquals(match, current))
            {
                return false;
            }

            match.IsClosed = true;
        }

        foreach (var player in current.Players)
        {
            _connectionMatches.TryRemove(player.ConnectionId, out _);
            _playerMatches.TryRemove(player.PlayerId, out _);
        }

        current.Dispose();
        return true;
    }
}

/**
 * MÓDOSÍTÁS: eltávolításkor a session saját lockja alatt kap Closed
 * állapotot, ezért egy korábban lekért referencia sem módosíthatja
 * tovább a törölt meccset. A szinkronizáló objektum nem eldobható
 * erőforrás, így nincs SemaphoreSlim-dispose versenyhelyzet.
 * A kapcsolódott játékosokról rövid, lockolt pillanatképet ad a
 * ranked létszám képernyő-DTO-ba töltéséhez.
 * MÓDOSÍTÁS: egy befejezett meccs játékoszárolása külön is
 * feloldható anélkül, hogy a többi, reward képernyőt még néző
 * játékos sessionjét törölné.
 * MÓDOSÍTÁS: logoutkor PlayerId alapján is megkereshető a játékos
 * aktuális VS meccse.
 *
 * A futó VS meccsek és a connection/player hozzárendelések
 * folyamaton belüli, konkurens tárolója.
 */
