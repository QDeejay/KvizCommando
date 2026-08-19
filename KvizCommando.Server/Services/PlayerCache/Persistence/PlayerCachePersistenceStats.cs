using System;

namespace KvizCommando.Server.Services.PlayerCache
{
    /// <summary>
    /// Egy cache mentési ciklus statisztikája.
    /// </summary>
    public sealed class PlayerCachePersistenceStats
    {
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;
        public int TotalUsers { get; init; }
        public int DirtyUsers { get; init; }
        public int ObscolatedUsers { get; init; }
        public int DirtyQuestions { get; init; }
        public int LogoutUsers { get; init; }
        public TimeSpan Duration { get; init; }
    }
}
