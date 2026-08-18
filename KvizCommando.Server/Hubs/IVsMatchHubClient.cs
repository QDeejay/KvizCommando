using KvizCommando.Shared.Contracts.VsGame.Match;

namespace KvizCommando.Server.Hubs;

public interface IVsMatchHubClient
{
    /// <summary>
    /// Visszaküldi a késleltetésméréshez kapott jelzőértéket.
    /// </summary>
    /// <param name="token">A késleltetésméréshez változatlanul visszaküldendő érték.</param>
    /// <returns>A kapott <paramref name="token"/> értéke változtatás nélkül.</returns>
    Task<long> LatencyProbe(long token);
    /// <summary>
    /// Értesíti a klienst a rangsorolt várólista megváltozásáról.
    /// </summary>
    /// <param name="snapshot">A kliensnek továbbítandó aktuális állapotpillanatkép.</param>
    Task QueueChanged(VsRankedQueueSnapshot snapshot);
    /// <summary>
    /// Értesíti a klienst a meccsállapot megváltozásáról.
    /// </summary>
    /// <param name="snapshot">A kliensnek továbbítandó aktuális állapotpillanatkép.</param>
    Task MatchChanged(VsMatchSnapshot snapshot);
    /// <summary>
    /// Értesíti a klienst a meccs lezárásáról.
    /// </summary>
    /// <param name="messageKey">A bezárás okát leíró lokalizációs kulcs.</param>
    Task MatchClosed(string messageKey);
}
