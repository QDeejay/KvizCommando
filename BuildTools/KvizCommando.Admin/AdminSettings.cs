using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace KvizCommando.Admin;

internal enum AdminDatabaseProvider
{
    Sqlite,
    SqlServer
}

internal sealed record AdminSettings(
    AdminDatabaseProvider Provider,
    string ApplicationConnectionString,
    string GameConnectionString,
    string ServerLocalBaseUrl,
    string AuditOutputRoot,
    bool IsProduction)
{
    private const string USER_SECRETS_ID = "66c00aba-ecb3-4d1e-89f0-b323f37e8306";

    public static AdminSettings Resolve()
    {
        if (OperatingSystem.IsLinux())
            return ResolveProduction();

        Console.Clear();
        Console.WriteLine("KVIZ COMMANDO ADMIN");
        Console.WriteLine();
        Console.WriteLine("Adatbázis:");
        Console.WriteLine("[1] SQLite");
        Console.WriteLine("[2] SQL Server");
        Console.WriteLine("[0] Kilépés");
        Console.WriteLine();

        while (true)
        {
            Console.Write("Választás: ");
            var key = Console.ReadKey(intercept: true).KeyChar;
            Console.WriteLine(key);

            if (key == '0')
                Environment.Exit(0);

            if (key == '1')
                return ResolveDevelopmentSqlite();

            if (key == '2')
                return ResolveDevelopmentSqlServer();
        }
    }

    private static AdminSettings ResolveDevelopmentSqlite()
    {
        var serverDirectory = FindServerDirectory();
        return new AdminSettings(
            AdminDatabaseProvider.Sqlite,
            $"Data Source={Path.Combine(serverDirectory, "GameUser.db")}",
            $"Data Source={Path.Combine(serverDirectory, "Game.db")}",
            Environment.GetEnvironmentVariable("KVIZCOMMANDO_SERVER_LOCAL_URL") ?? "http://localhost:5055",
            Path.Combine(serverDirectory, "App", "Audit"),
            false);
    }

    private static AdminSettings ResolveDevelopmentSqlServer()
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets(USER_SECRETS_ID)
            .Build();

        var application = configuration["ConnectionStrings:SqlServerApplication"];
        var game = configuration["ConnectionStrings:SqlServerGame"];
        var serverDirectory = FindServerDirectory();

        if (string.IsNullOrWhiteSpace(application))
            throw new InvalidOperationException("Hiányzó User Secret: ConnectionStrings:SqlServerApplication");
        if (string.IsNullOrWhiteSpace(game))
            throw new InvalidOperationException("Hiányzó User Secret: ConnectionStrings:SqlServerGame");

        return new AdminSettings(
            AdminDatabaseProvider.SqlServer,
            application,
            game,
            (Environment.GetEnvironmentVariable("KVIZCOMMANDO_SERVER_LOCAL_URL") ?? "http://localhost:5055").TrimEnd('/'),
            Path.Combine(serverDirectory, "App", "Audit"),
            false);
    }

    private static AdminSettings ResolveProduction()
    {
        const string secretsPath = "/etc/kvizcommando/secrets.json";
        return FromSecrets(
            secretsPath,
            Environment.GetEnvironmentVariable("KVIZCOMMANDO_SERVER_LOCAL_URL") ?? "http://127.0.0.1:5000",
            true);
    }

    private static AdminSettings FromSecrets(string path, string serverLocalBaseUrl, bool production)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"Nem található a secrets fájl: {path}");

        using var document = JsonDocument.Parse(File.ReadAllText(path));
        if (!document.RootElement.TryGetProperty("ConnectionStrings", out var connectionStrings))
            throw new InvalidOperationException("A secrets.json nem tartalmaz ConnectionStrings szekciót.");

        var application = GetRequired(connectionStrings, "SqlServerApplication");
        var game = GetRequired(connectionStrings, "SqlServerGame");

        return new AdminSettings(
            AdminDatabaseProvider.SqlServer,
            application,
            game,
            serverLocalBaseUrl.TrimEnd('/'),
            "/var/lib/kvizcommando/Audit",
            production);
    }

    private static string GetRequired(JsonElement section, string name)
    {
        if (!section.TryGetProperty(name, out var value) || string.IsNullOrWhiteSpace(value.GetString()))
            throw new InvalidOperationException($"Hiányzó connection string: ConnectionStrings:{name}");

        return value.GetString()!;
    }

    private static string FindServerDirectory()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            var candidate = Path.Combine(current.FullName, "KvizCommando.Server");
            if (Directory.Exists(candidate))
                return candidate;

            current = current.Parent;
        }

        current = new DirectoryInfo(Environment.CurrentDirectory);
        while (current is not null)
        {
            var candidate = Path.Combine(current.FullName, "KvizCommando.Server");
            if (Directory.Exists(candidate))
                return candidate;

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Nem található a KvizCommando.Server könyvtár.");
    }
}
