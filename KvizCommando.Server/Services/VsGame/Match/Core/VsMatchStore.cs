using System.Collections.Concurrent;

namespace KvizCommando.Server.Services.VsGame.Match;

public sealed class VsMatchStore
{
    private readonly ConcurrentDictionary<Guid, VsMatchSession> _matches = [];
    private readonly ConcurrentDictionary<string, Guid> _connectionMatches = [];
    private readonly ConcurrentDictionary<int, Guid> _playerMatches = [];

    /// <summary>
    /// Megkísérli hozzáadni a meccset a meccstárhoz.
    /// </summary>
    /// <param name="match">Az inicializálandó, már zárolt meccsállapot.</param>
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

    /// <summary>
    /// Jelzi, hogy a meccstár tartalmazza-e a megadott játékost.
    /// </summary>
    /// <param name="playerId">A játékos adatbázis-azonosítója.</param>
    public bool ContainsPlayer(int playerId) =>
        _playerMatches.ContainsKey(playerId);

    /// <summary>
    /// Megkísérli visszaadni a megadott azonosítójú elemet.
    /// </summary>
    /// <param name="matchId">A meccs azonosítója.</param>
    /// <param name="match">Az inicializálandó, már zárolt meccsállapot.</param>
    public bool TryGet(Guid matchId, out VsMatchSession? match) =>
        _matches.TryGetValue(matchId, out match);

    /// <summary>
    /// Megkísérli visszaadni a kapcsolathoz tartozó meccset és játékost.
    /// </summary>
    /// <param name="connectionId">Az aktív SignalR-kapcsolat azonosítója.</param>
    /// <param name="match">Az inicializálandó, már zárolt meccsállapot.</param>
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

    /// <summary>
    /// Megkísérli visszaadni a játékoshoz tartozó meccset és játékosállapotot.
    /// </summary>
    /// <param name="playerId">A játékos adatbázis-azonosítója.</param>
    /// <param name="match">Az inicializálandó, már zárolt meccsállapot.</param>
    public bool TryGetByPlayer(
        int playerId,
        out VsMatchSession? match)
    {
        match = null;

        return _playerMatches.TryGetValue(playerId, out var matchId) &&
               _matches.TryGetValue(matchId, out match);
    }

    /// <summary>
    /// Felszabadítja a játékos meccshez tartozó foglalásait.
    /// </summary>
    /// <param name="match">Az inicializálandó, már zárolt meccsállapot.</param>
    /// <param name="player">A mentendő gyorsítótárazott játékosállapot.</param>
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

    /// <summary>
    /// Megkísérli eltávolítani a meccset a meccstárból.
    /// </summary>
    /// <param name="matchId">A meccs azonosítója.</param>
    /// <param name="match">Az inicializálandó, már zárolt meccsállapot.</param>
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
