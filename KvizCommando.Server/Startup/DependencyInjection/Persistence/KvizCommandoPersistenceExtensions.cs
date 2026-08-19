using KvizCommando.Server.Infrastructure.Persistence;
using KvizCommando.Server.Services.Db;
using Microsoft.EntityFrameworkCore;

namespace KvizCommando.Server.Startup;

public static class KvizCommandoPersistenceExtensions
{
    private const string SQLITE_APPLICATION_CONNECTION = "SqliteApplication";
    private const string SQLITE_GAME_CONNECTION = "SqliteGame";
    private const string SQL_SERVER_APPLICATION_CONNECTION = "SqlServerApplication";
    private const string SQL_SERVER_GAME_CONNECTION = "SqlServerGame";

    /// <summary>
    /// Regisztrálja az Identity- és játékadatbázist, valamint az ezeket közvetlenül elérő szolgáltatásokat.
    /// </summary>
    /// <param name="services">A bővítendő szolgáltatásgyűjtemény.</param>
    /// <param name="configuration">Az adatbázis-kapcsolati karakterláncokat tartalmazó konfiguráció.</param>
    /// <returns>A további regisztrációkhoz használható szolgáltatásgyűjtemény.</returns>
    public static IServiceCollection AddKvizCommandoPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var databaseOptions = configuration
            .GetSection(DatabaseOptions.SECTION_NAME)
            .Get<DatabaseOptions>() ?? new DatabaseOptions();

        switch (databaseOptions.Provider)
        {
            case DatabaseProvider.Sqlite:
                AddSqliteContexts(services, configuration);
                break;

            case DatabaseProvider.SqlServer:
                AddSqlServerContexts(
                    services,
                    configuration,
                    databaseOptions.EnableRetryOnFailure);
                break;

            default:
                throw new InvalidOperationException(
                    $"Nem támogatott adatbázis-provider: {databaseOptions.Provider}");
        }

        services.AddScoped<IQuestionDbService, QuestionDbService>();
        services.AddScoped<IPlayerDbService, PlayerDbService>();

        return services;
    }

    private static void AddSqliteContexts(
        IServiceCollection services,
        IConfiguration configuration)
    {
        var applicationConnection = GetRequiredConnectionString(
            configuration,
            SQLITE_APPLICATION_CONNECTION);
        var gameConnection = GetRequiredConnectionString(
            configuration,
            SQLITE_GAME_CONNECTION);

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(applicationConnection));
        services.AddDbContext<GameDbContext>(options =>
            options.UseSqlite(gameConnection));

        // Ezeket kizárólag az EF migrációs eszközei használják.
        services.AddDbContext<SqliteApplicationDbContext>(options =>
            options.UseSqlite(applicationConnection));
        services.AddDbContext<SqliteGameDbContext>(options =>
            options.UseSqlite(gameConnection));
    }

    private static void AddSqlServerContexts(
        IServiceCollection services,
        IConfiguration configuration,
        bool enableRetryOnFailure)
    {
        var applicationConnection = GetRequiredConnectionString(
            configuration,
            SQL_SERVER_APPLICATION_CONNECTION);
        var gameConnection = GetRequiredConnectionString(
            configuration,
            SQL_SERVER_GAME_CONNECTION);

        services.AddDbContext<ApplicationDbContext>(options =>
            ConfigureSqlServer(options, applicationConnection, enableRetryOnFailure));
        services.AddDbContext<GameDbContext>(options =>
            ConfigureSqlServer(options, gameConnection, enableRetryOnFailure));

        // Ezeket kizárólag az EF migrációs eszközei használják.
        services.AddDbContext<SqlServerApplicationDbContext>(options =>
            ConfigureSqlServer(options, applicationConnection, enableRetryOnFailure));
        services.AddDbContext<SqlServerGameDbContext>(options =>
            ConfigureSqlServer(options, gameConnection, enableRetryOnFailure));
    }

    private static void ConfigureSqlServer(
        DbContextOptionsBuilder options,
        string connectionString,
        bool enableRetryOnFailure)
    {
        options.UseSqlServer(connectionString, sqlOptions =>
        {
            if (enableRetryOnFailure)
                sqlOptions.EnableRetryOnFailure();
        });
    }

    private static string GetRequiredConnectionString(
        IConfiguration configuration,
        string name)
    {
        var value = configuration.GetConnectionString(name);
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(
                $"Hiányzó adatbázis-kapcsolati karakterlánc: ConnectionStrings:{name}");
        }

        return value;
    }
}
