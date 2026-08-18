namespace KvizCommando.Server.Hubs;

public interface ISoloGameHubClient
{
    Task<long> LatencyProbe(long token);
}
