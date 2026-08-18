namespace KvizCommando.Server.Hubs;

public interface ISoloGameHubClient
{
    /// <summary>
    /// Visszaküldi a késleltetésméréshez kapott jelzőértéket.
    /// </summary>
    /// <param name="token">A késleltetésméréshez változatlanul visszaküldendő érték.</param>
    /// <returns>A kapott <paramref name="token"/> értéke változtatás nélkül.</returns>
    Task<long> LatencyProbe(long token);
}
