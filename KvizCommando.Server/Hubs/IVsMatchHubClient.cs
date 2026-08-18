using KvizCommando.Shared.Contracts.VsGame.Match;

namespace KvizCommando.Server.Hubs;

public interface IVsMatchHubClient
{
    /// <summary>
    /// Visszaküldi a késleltetésméréshez kapott jelzőértéket.
    /// </summary>
    Task<long> LatencyProbe(long token);
    /// <summary>
    /// Értesíti a klienst a rangsorolt várólista megváltozásáról.
    /// </summary>
    Task QueueChanged(VsRankedQueueSnapshot snapshot);
    /// <summary>
    /// Értesíti a klienst a meccsállapot megváltozásáról.
    /// </summary>
    Task MatchChanged(VsMatchSnapshot snapshot);
    /// <summary>
    /// Értesíti a klienst a meccs lezárásáról.
    /// </summary>
    Task MatchClosed(string messageKey);
}
