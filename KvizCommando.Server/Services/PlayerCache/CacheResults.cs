namespace KvizCommando.Server.Services.PlayerCache;

public enum CacheReadStatus
{
    Success,
    NotFound,
    SessionMismatch
}

public enum CacheUpdateResult
{
    Updated,
    Rejected,
    NotFound,
    SessionMismatch
}

public sealed class CacheReadResult
{
    public CacheReadStatus Status { get; init; }
    public CachedPlayer? Player { get; init; }
    public CachedQuestion? Questions { get; init; }
}
