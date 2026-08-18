using KvizCommando.Shared.Contracts.VsGame.Match;

namespace KvizCommando.Server.Hubs;

public interface IVsMatchHubClient
{
    Task<long> LatencyProbe(long token);
    Task QueueChanged(VsRankedQueueSnapshot snapshot);
    Task MatchChanged(VsMatchSnapshot snapshot);
    Task MatchClosed(string messageKey);
}
