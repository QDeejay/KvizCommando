namespace KvizCommando.Server.Infrastructure.Persistence;

/// <summary>
/// Az alkalmazás által támogatott adatbázis-szolgáltatók.
/// </summary>
public enum DatabaseProvider
{
    /// <summary>Helyi fájlalapú SQLite adatbázis.</summary>
    Sqlite,

    /// <summary>Microsoft SQL Server adatbázis.</summary>
    SqlServer
}

/// <summary>
/// Az aktív adatbázis-szolgáltató beállításai.
/// </summary>
public sealed class DatabaseOptions
{
    /// <summary>A konfigurációs szakasz neve.</summary>
    public const string SECTION_NAME = "Database";

    /// <summary>Az alkalmazás által használt adatbázis-szolgáltató.</summary>
    public DatabaseProvider Provider { get; set; } = DatabaseProvider.Sqlite;

    /// <summary>Bekapcsolja az átmeneti SQL Server-hibák automatikus újrapróbálását.</summary>
    public bool EnableRetryOnFailure { get; set; }
}
