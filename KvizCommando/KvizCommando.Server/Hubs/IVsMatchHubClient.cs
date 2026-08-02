using KvizCommando.Shared.Contracts.VsGame.Match;

namespace KvizCommando.Server.Hubs;

public interface IVsMatchHubClient
{
    Task<long> LatencyProbe(long token);
    Task QueueChanged(VsRankedQueueSnapshot snapshot);
    Task MatchChanged(VsMatchSnapshot snapshot);
    Task MatchClosed(string messageKey);
}

/**
 * MÓDOSÍTÁS: a közvetlen queue-belépési eredmény kiváltja a
 * CommandRejected eseményt. A MatchClosed kizárólag olyan későbbi,
 * több játékost érintő meccslezárást jelez, amelyet egyetlen Hub-
 * visszatérési értékkel már nem lehet minden résztvevőnek továbbítani.
 * MÓDOSÍTÁS: a LatencyProbe a szerver által időzített, egyszeri
 * kapcsolatellenőrzés kliens-visszhangja; játékállapotot nem hordoz.
 *
 * A VS SignalR Hub szerverről kliensre küldhető, erősen típusos
 * eseményeit határozza meg.
 */
