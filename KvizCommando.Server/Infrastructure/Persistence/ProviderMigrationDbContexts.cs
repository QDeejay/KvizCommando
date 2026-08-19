using Microsoft.EntityFrameworkCore;

namespace KvizCommando.Server.Infrastructure.Persistence;

/// <summary>Az SQLite Application migrációk saját kontextusa.</summary>
public sealed class SqliteApplicationDbContext : ApplicationDbContext
{
    /// <summary>Létrehozza az SQLite Application migrációs kontextust.</summary>
    public SqliteApplicationDbContext(
        DbContextOptions<SqliteApplicationDbContext> options)
        : base(options)
    {
    }
}

/// <summary>Az SQLite Game migrációk saját kontextusa.</summary>
public sealed class SqliteGameDbContext : GameDbContext
{
    /// <summary>Létrehozza az SQLite Game migrációs kontextust.</summary>
    public SqliteGameDbContext(
        DbContextOptions<SqliteGameDbContext> options)
        : base(options)
    {
    }
}

/// <summary>Az SQL Server Application migrációk saját kontextusa.</summary>
public sealed class SqlServerApplicationDbContext : ApplicationDbContext
{
    /// <summary>Létrehozza az SQL Server Application migrációs kontextust.</summary>
    public SqlServerApplicationDbContext(
        DbContextOptions<SqlServerApplicationDbContext> options)
        : base(options)
    {
    }
}

/// <summary>Az SQL Server Game migrációk saját kontextusa.</summary>
public sealed class SqlServerGameDbContext : GameDbContext
{
    /// <summary>Létrehozza az SQL Server Game migrációs kontextust.</summary>
    public SqlServerGameDbContext(
        DbContextOptions<SqlServerGameDbContext> options)
        : base(options)
    {
    }
}
