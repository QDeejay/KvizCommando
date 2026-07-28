using KvizCommando.Shared.Contracts.VsGame.Match;

namespace KvizCommando.Server.Hubs;

public interface IVsMatchHubClient
{
    Task QueueChanged(VsRankedQueueSnapshot snapshot);
    Task MatchChanged(VsMatchSnapshot snapshot);
    Task CommandRejected(string messageKey);
}

/**
 * A VS SignalR Hub szerverről kliensre küldhető, erősen típusos
 * eseményeit határozza meg.
 */
