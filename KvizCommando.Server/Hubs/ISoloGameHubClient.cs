namespace KvizCommando.Server.Hubs;

public interface ISoloGameHubClient
{
    /// <summary>
    /// Visszaküldi a késleltetésméréshez kapott jelzőértéket.
    /// </summary>
    Task<long> LatencyProbe(long token);
}
