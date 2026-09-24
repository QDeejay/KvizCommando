namespace KvizCommando.Server.Services.PlayerCache;

/// <summary>A játékosadatok időszakos tartósításának ütemezése.</summary>
public sealed class PlayerCachePersistenceOptions
{
    /// <summary>A műveleti beállítások JSON-szekciójának neve.</summary>
    public const string SECTION_NAME = "PlayerCachePersistence";
    /// <summary>A játékosadatok mentési ciklusának hossza másodpercben.</summary>
    public int FlushIntervalSeconds { get; set; } = 15;
}
