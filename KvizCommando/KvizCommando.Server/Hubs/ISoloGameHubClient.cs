namespace KvizCommando.Server.Hubs;

public interface ISoloGameHubClient
{
    Task<long> LatencyProbe(long token);
}

/**
 * ÚJ FÁJL: az egyetlen Solo hub kizárólag a pingméréshez kér
 * visszhangot a klienstől. Játékállapotot és snapshotot nem küld.
 */
