using System.Data.Common;
using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;

namespace KvizCommando.Admin;

internal sealed class AdminDatabase : IDisposable
{
    private readonly AdminSettings _settings;

    public AdminDatabase(AdminSettings settings)
    {
        _settings = settings;
    }

    public string EnvironmentLabel => _settings.IsProduction ? "PRODUCTION" : "DEVELOPMENT";
    public string ProviderLabel => _settings.Provider == AdminDatabaseProvider.SqlServer ? "SQL Server" : "SQLite";

    public void TestConnections()
    {
        using var app = OpenApplicationConnection();
        using var game = OpenGameConnection();
        ExecuteScalar(app, "SELECT 1");
        ExecuteScalar(game, "SELECT 1");
    }

    public IReadOnlyList<UserRow> GetUsers(string? search = null)
    {
        using var connection = OpenApplicationConnection();
        var top = _settings.Provider == AdminDatabaseProvider.SqlServer ? "TOP (200) " : string.Empty;
        var limit = _settings.Provider == AdminDatabaseProvider.Sqlite ? " LIMIT 200" : string.Empty;
        var sql = $"""
            SELECT {top}
                u.Id,
                COALESCE(u.Email, ''),
                u.DisplayName,
                u.EmailConfirmed,
                u.IsDeleted,
                u.CreatedAtUtc,
                p.PlayerId,
                p.RankEnum,
                p.XP,
                p.Credit,
                p.Voucher
            FROM AspNetUsers u
            LEFT JOIN Players p ON p.UserId = u.Id
            WHERE @search = ''
               OR u.Email LIKE @pattern
               OR COALESCE(u.DisplayName, '') LIKE @pattern
            ORDER BY u.CreatedAtUtc DESC{limit};
            """;

        using var command = CreateCommand(connection, sql);
        AddParameter(command, "@search", search?.Trim() ?? string.Empty);
        AddParameter(command, "@pattern", $"%{search?.Trim() ?? string.Empty}%");

        using var reader = command.ExecuteReader();
        var result = new List<UserRow>();
        while (reader.Read())
        {
            result.Add(new UserRow(
                reader.GetString(0),
                reader.GetString(1),
                reader.IsDBNull(2) ? null : reader.GetString(2),
                ReadBool(reader, 3),
                ReadBool(reader, 4),
                reader.GetDateTime(5),
                reader.IsDBNull(6) ? null : reader.GetInt32(6),
                reader.IsDBNull(7) ? null : reader.GetInt32(7),
                reader.IsDBNull(8) ? null : reader.GetInt32(8),
                reader.IsDBNull(9) ? null : reader.GetInt32(9),
                reader.IsDBNull(10) ? null : reader.GetInt32(10)));
        }

        return result;
    }

    public string CreateUser(string email, bool confirmed)
    {
        email = email.Trim();
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            throw new InvalidOperationException("Érvénytelen e-mail cím.");

        using var connection = OpenApplicationConnection();
        using var duplicate = CreateCommand(connection,
            "SELECT COUNT(*) FROM AspNetUsers WHERE NormalizedEmail = @email AND IsDeleted = 0;");
        AddParameter(duplicate, "@email", email.ToUpperInvariant());
        if (Convert.ToInt32(duplicate.ExecuteScalar()) > 0)
            throw new InvalidOperationException("Ezzel az e-mail címmel már létezik aktív felhasználó.");

        var id = Guid.NewGuid().ToString();
        using var command = CreateCommand(connection, """
            INSERT INTO AspNetUsers
            (
                Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed,
                PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumber, PhoneNumberConfirmed,
                TwoFactorEnabled, LockoutEnd, LockoutEnabled, AccessFailedCount,
                DisplayName, NormalizedDisplayName, PreferredLocale, CreatedAtUtc,
                AcceptTerms, MarketingConsent, IsDeleted
            )
            VALUES
            (
                @id, @email, @normalizedEmail, @email, @normalizedEmail, @confirmed,
                NULL, @securityStamp, @concurrencyStamp, NULL, 0,
                0, NULL, 1, 0,
                NULL, NULL, 'hu-HU', @createdAtUtc,
                0, 0, 0
            );
            """);

        AddParameter(command, "@id", id);
        AddParameter(command, "@email", email);
        AddParameter(command, "@normalizedEmail", email.ToUpperInvariant());
        AddParameter(command, "@confirmed", confirmed ? 1 : 0);
        AddParameter(command, "@securityStamp", Guid.NewGuid().ToString("N"));
        AddParameter(command, "@concurrencyStamp", Guid.NewGuid().ToString());
        AddParameter(command, "@createdAtUtc", DateTime.UtcNow);
        command.ExecuteNonQuery();
        return id;
    }

    public void UpdateUser(UserRow original, string? displayName, bool emailConfirmed, int? rank, int? xp, int? credit, int? voucher)
    {
        displayName = string.IsNullOrWhiteSpace(displayName) ? null : displayName.Trim();
        if (rank is < 0 || xp is < 0 || credit is < 0 || voucher is < 0)
            throw new InvalidOperationException("A rang, XP, kredit és voucher nem lehet negatív.");

        using var connection = OpenApplicationConnection();
        using var transaction = connection.BeginTransaction();

        using (var command = CreateCommand(connection, """
            UPDATE AspNetUsers
            SET DisplayName = @displayName,
                NormalizedDisplayName = @normalizedDisplayName,
                EmailConfirmed = @emailConfirmed,
                ConcurrencyStamp = @concurrencyStamp
            WHERE Id = @id;
            """, transaction))
        {
            AddParameter(command, "@displayName", displayName);
            AddParameter(command, "@normalizedDisplayName", displayName?.ToUpperInvariant());
            AddParameter(command, "@emailConfirmed", emailConfirmed ? 1 : 0);
            AddParameter(command, "@concurrencyStamp", Guid.NewGuid().ToString());
            AddParameter(command, "@id", original.Id);
            command.ExecuteNonQuery();
        }

        if (original.PlayerId.HasValue)
        {
            using var command = CreateCommand(connection, """
                UPDATE Players
                SET DisplayName = COALESCE(@displayName, DisplayName),
                    RankEnum = @rank,
                    XP = @xp,
                    Credit = @credit,
                    Voucher = @voucher,
                    UpdatedUtc = @updatedUtc
                WHERE PlayerId = @playerId;
                """, transaction);
            AddParameter(command, "@displayName", displayName);
            AddParameter(command, "@rank", rank ?? original.Rank ?? 0);
            AddParameter(command, "@xp", xp ?? original.XP ?? 0);
            AddParameter(command, "@credit", credit ?? original.Credit ?? 0);
            AddParameter(command, "@voucher", voucher ?? original.Voucher ?? 0);
            AddParameter(command, "@updatedUtc", DateTime.UtcNow);
            AddParameter(command, "@playerId", original.PlayerId.Value);
            command.ExecuteNonQuery();
        }

        transaction.Commit();
    }

    public void DeleteUser(UserRow user)
    {
        if (user.PlayerId.HasValue)
        {
            using var game = OpenGameConnection();
            using var gameTransaction = game.BeginTransaction();
            ExecuteNonQuery(game, "DELETE FROM PendingQuestions WHERE PlayerId = @playerId;", gameTransaction, ("@playerId", user.PlayerId.Value));
            ExecuteNonQuery(game, "DELETE FROM UserQuestions WHERE PlayerId = @playerId;", gameTransaction, ("@playerId", user.PlayerId.Value));
            gameTransaction.Commit();
        }

        using var app = OpenApplicationConnection();
        using var transaction = app.BeginTransaction();

        if (user.PlayerId.HasValue)
        {
            var playerId = user.PlayerId.Value;
            foreach (var table in new[]
                     {
                         "PlayerCharacters", "PlayerLoadouts", "PlayerCategoryStats", "PlayerOrientStat",
                         "PlayerAskStats", "TeamStatistics"
                     })
            {
                ExecuteNonQuery(app, $"DELETE FROM {table} WHERE PlayerId = @playerId;", transaction, ("@playerId", playerId));
            }
            ExecuteNonQuery(app, "DELETE FROM Players WHERE PlayerId = @playerId;", transaction, ("@playerId", playerId));
        }

        foreach (var table in new[] { "UserPaymentMethods", "MarketingConsents", "TermsConsents", "UserPii" })
            ExecuteNonQuery(app, $"DELETE FROM {table} WHERE UserId = @userId;", transaction, ("@userId", user.Id));

        foreach (var table in new[] { "AspNetUserTokens", "AspNetUserLogins", "AspNetUserClaims", "AspNetUserRoles" })
            ExecuteNonQuery(app, $"DELETE FROM {table} WHERE UserId = @userId;", transaction, ("@userId", user.Id));

        ExecuteNonQuery(app, "DELETE FROM AspNetUsers WHERE Id = @userId;", transaction, ("@userId", user.Id));
        transaction.Commit();
    }

    public IReadOnlyList<PendingQuestionRow> GetPendingQuestions(string? questionSearch, int playerId)
    {
        using var connection = OpenGameConnection();
        var top = _settings.Provider == AdminDatabaseProvider.SqlServer ? "TOP (300) " : string.Empty;
        var limit = _settings.Provider == AdminDatabaseProvider.Sqlite ? " LIMIT 300" : string.Empty;
        var normalizedSearch = questionSearch?.Trim() ?? string.Empty;
        using var command = CreateCommand(connection, $"""
            SELECT {top}Id, PlayerId, CategoryNo, Question, AnswersJson, Status, Remark, SubmittedAt
            FROM PendingQuestions
            WHERE (@search = '' OR Question LIKE @pattern)
              AND (@playerId = 0 OR PlayerId = @playerId)
            ORDER BY SubmittedAt DESC{limit};
            """);
        AddParameter(command, "@search", normalizedSearch);
        AddParameter(command, "@pattern", $"%{normalizedSearch}%");
        AddParameter(command, "@playerId", playerId);
        using var reader = command.ExecuteReader();
        var result = new List<PendingQuestionRow>();
        while (reader.Read())
        {
            result.Add(new PendingQuestionRow(
                reader.GetInt32(0), reader.GetInt32(1), reader.GetInt32(2), reader.GetString(3), reader.GetString(4),
                reader.GetString(5), reader.IsDBNull(6) ? null : reader.GetString(6), reader.GetDateTime(7)));
        }
        return result;
    }

    public IReadOnlyList<UserQuestionRow> GetUserQuestions(string? questionSearch, int playerId)
    {
        using var connection = OpenGameConnection();
        var top = _settings.Provider == AdminDatabaseProvider.SqlServer ? "TOP (300) " : string.Empty;
        var limit = _settings.Provider == AdminDatabaseProvider.Sqlite ? " LIMIT 300" : string.Empty;
        var normalizedSearch = questionSearch?.Trim() ?? string.Empty;
        using var command = CreateCommand(connection, $"""
            SELECT {top}Id, PlayerId, CategoryNo, Question, AnswersJson, Ask, OkAnswer
            FROM UserQuestions
            WHERE (@search = '' OR Question LIKE @pattern)
              AND (@playerId = 0 OR PlayerId = @playerId)
            ORDER BY Id DESC{limit};
            """);
        AddParameter(command, "@search", normalizedSearch);
        AddParameter(command, "@pattern", $"%{normalizedSearch}%");
        AddParameter(command, "@playerId", playerId);
        using var reader = command.ExecuteReader();
        var result = new List<UserQuestionRow>();
        while (reader.Read())
        {
            result.Add(new UserQuestionRow(
                reader.GetInt32(0), reader.GetInt32(1), reader.GetInt32(2), reader.GetString(3), reader.GetString(4),
                reader.GetInt32(5), reader.GetInt32(6)));
        }
        return result;
    }

    public IReadOnlyList<FactoryQuestionCategoryCount> GetFactoryQuestionCategoryCounts()
    {
        using var connection = OpenGameConnection();
        var counts = Enumerable.Range(1, 16).ToDictionary(category => category, _ => 0);

        using (var command = CreateCommand(connection, """
            SELECT CategoryNo, COUNT(*)
            FROM FactoryQuestions
            WHERE CategoryNo BETWEEN 1 AND 16
            GROUP BY CategoryNo;
            """))
        using (var reader = command.ExecuteReader())
        {
            while (reader.Read())
                counts[reader.GetInt32(0)] = Convert.ToInt32(reader.GetValue(1));
        }

        counts[99] = Convert.ToInt32(ExecuteScalar(connection, "SELECT COUNT(*) FROM GuessQuestions;") ?? 0);
        return counts
            .OrderBy(item => item.Key)
            .Select(item => new FactoryQuestionCategoryCount(item.Key, item.Value))
            .ToArray();
    }

    public IReadOnlyList<FactoryQuestionRow> GetFactoryQuestions(
        int? categoryNo,
        string? search,
        bool reportedOnly,
        bool playerQuestionsOnly)
    {
        if (categoryNo is not null && categoryNo is not (>= 1 and <= 16) && categoryNo != 99)
            throw new InvalidOperationException("Hibás kategóriaszűrő.");

        using var connection = OpenGameConnection();
        var rows = new List<FactoryQuestionRow>();
        var normalizedSearch = search?.Trim() ?? string.Empty;

        if (categoryNo != 99)
        {
            var top = _settings.Provider == AdminDatabaseProvider.SqlServer ? "TOP (25) " : string.Empty;
            var limit = _settings.Provider == AdminDatabaseProvider.Sqlite ? " LIMIT 25" : string.Empty;
            using var command = CreateCommand(connection, $"""
                SELECT {top}Id, PlayerId, CategoryNo, Question, AnswersJson, Reported
                FROM FactoryQuestions
                WHERE (@categoryNo = 0 OR CategoryNo = @categoryNo)
                  AND (@search = '' OR Question LIKE @pattern)
                  AND (@reportedOnly = 0 OR Reported > 0)
                  AND (@playerQuestionsOnly = 0 OR PlayerId > 0)
                ORDER BY Id DESC{limit};
                """);
            AddParameter(command, "@categoryNo", categoryNo ?? 0);
            AddParameter(command, "@search", normalizedSearch);
            AddParameter(command, "@pattern", $"%{normalizedSearch}%");
            AddParameter(command, "@reportedOnly", reportedOnly ? 1 : 0);
            AddParameter(command, "@playerQuestionsOnly", playerQuestionsOnly ? 1 : 0);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                rows.Add(new FactoryQuestionRow(
                    reader.GetInt32(0),
                    reader.GetInt32(1) > 0 ? reader.GetInt32(1) : null,
                    reader.GetInt32(2),
                    reader.GetString(3),
                    reader.GetString(4),
                    reader.GetInt32(5),
                    false));
            }
        }

        if ((categoryNo is null or 99) && !playerQuestionsOnly)
        {
            var top = _settings.Provider == AdminDatabaseProvider.SqlServer ? "TOP (25) " : string.Empty;
            var limit = _settings.Provider == AdminDatabaseProvider.Sqlite ? " LIMIT 25" : string.Empty;
            using var command = CreateCommand(connection, $"""
                SELECT {top}Id, Question, Answer, Reported
                FROM GuessQuestions
                WHERE (@search = '' OR Question LIKE @pattern)
                  AND (@reportedOnly = 0 OR Reported > 0)
                ORDER BY Id DESC{limit};
                """);
            AddParameter(command, "@search", normalizedSearch);
            AddParameter(command, "@pattern", $"%{normalizedSearch}%");
            AddParameter(command, "@reportedOnly", reportedOnly ? 1 : 0);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                rows.Add(new FactoryQuestionRow(
                    reader.GetInt32(0),
                    null,
                    99,
                    reader.GetString(1),
                    Convert.ToDouble(reader.GetValue(2)).ToString("G15", CultureInfo.InvariantCulture),
                    reader.GetInt32(3),
                    true));
            }
        }

        return rows
            .OrderByDescending(row => row.Id)
            .ThenBy(row => row.CategoryNo)
            .Take(25)
            .ToArray();
    }

    public void UpdateFactoryQuestion(
        FactoryQuestionRow question,
        string text,
        IReadOnlyList<string> answers,
        int reported)
    {
        ValidateQuestion(question.CategoryNo, text, answers);
        ValidateReported(reported);
        using var connection = OpenGameConnection();
        using var command = CreateCommand(connection, """
            UPDATE FactoryQuestions
            SET Question = @question,
                AnswersJson = @answersJson,
                Reported = @reported
            WHERE Id = @id;
            """);
        AddParameter(command, "@question", text.Trim());
        AddParameter(command, "@answersJson", JsonSerializer.Serialize(answers));
        AddParameter(command, "@reported", reported);
        AddParameter(command, "@id", question.Id);
        if (command.ExecuteNonQuery() != 1)
            throw new InvalidOperationException("A Factory kérdés nem található.");
    }

    public void CreateFactoryQuestion(int categoryNo, string text, IReadOnlyList<string> answers)
    {
        ValidateQuestion(categoryNo, text, answers);
        using var connection = OpenGameConnection();
        using var command = CreateCommand(connection, """
            INSERT INTO FactoryQuestions (PlayerId, CategoryNo, Question, AnswersJson, Reported)
            VALUES (0, @categoryNo, @question, @answersJson, 0);
            """);
        AddParameter(command, "@categoryNo", categoryNo);
        AddParameter(command, "@question", text.Trim());
        AddParameter(command, "@answersJson", JsonSerializer.Serialize(answers));
        command.ExecuteNonQuery();
    }

    public void UpdateTipQuestion(FactoryQuestionRow question, string text, double answer, int reported)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new InvalidOperationException("A kérdésszöveg nem lehet üres.");
        ValidateReported(reported);
        using var connection = OpenGameConnection();
        using var command = CreateCommand(connection, """
            UPDATE GuessQuestions
            SET Question = @question,
                Answer = @answer,
                Reported = @reported
            WHERE Id = @id;
            """);
        AddParameter(command, "@question", text.Trim());
        AddParameter(command, "@answer", answer);
        AddParameter(command, "@reported", reported);
        AddParameter(command, "@id", question.Id);
        if (command.ExecuteNonQuery() != 1)
            throw new InvalidOperationException("A Tipp kérdés nem található.");
    }

    public void CreateTipQuestion(string text, double answer)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new InvalidOperationException("A kérdésszöveg nem lehet üres.");
        using var connection = OpenGameConnection();
        using var command = CreateCommand(connection, """
            INSERT INTO GuessQuestions (Question, Answer, Reported)
            VALUES (@question, @answer, 0);
            """);
        AddParameter(command, "@question", text.Trim());
        AddParameter(command, "@answer", answer);
        command.ExecuteNonQuery();
    }

    public void UpdatePendingQuestion(PendingQuestionRow question, string text, IReadOnlyList<string> answers, string status, string? remark)
    {
        ValidateQuestion(question.CategoryNo, text, answers);
        using var connection = OpenGameConnection();
        using var command = CreateCommand(connection, """
            UPDATE PendingQuestions
            SET Question = @question,
                AnswersJson = @answersJson,
                Status = @status,
                Remark = @remark
            WHERE Id = @id;
            """);
        AddParameter(command, "@question", text.Trim());
        AddParameter(command, "@answersJson", JsonSerializer.Serialize(answers));
        AddParameter(command, "@status", status);
        AddParameter(command, "@remark", string.IsNullOrWhiteSpace(remark) ? null : remark.Trim());
        AddParameter(command, "@id", question.Id);
        command.ExecuteNonQuery();
    }

    public void UpdateUserQuestion(UserQuestionRow question, string text, IReadOnlyList<string> answers)
    {
        ValidateQuestion(question.CategoryNo, text, answers);
        using var connection = OpenGameConnection();
        using var command = CreateCommand(connection, """
            UPDATE UserQuestions
            SET Question = @question,
                AnswersJson = @answersJson
            WHERE Id = @id;
            """);
        AddParameter(command, "@question", text.Trim());
        AddParameter(command, "@answersJson", JsonSerializer.Serialize(answers));
        AddParameter(command, "@id", question.Id);
        command.ExecuteNonQuery();
    }

    public void SendForgotPassword(string email)
    {
        using var handler = new HttpClientHandler();
        using var client = new HttpClient(handler) { BaseAddress = new Uri(_settings.ServerLocalBaseUrl) };
        using var response = client.PostAsJsonAsync("/forgotPassword", new { email }).GetAwaiter().GetResult();
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"Forgot password kérés sikertelen: {(int)response.StatusCode} {response.ReasonPhrase}");
    }

    public DateTimeOffset? GetApplicationMigrationExecution(Guid uploadId, string migrationId) =>
        GetMigrationExecution(OpenApplicationConnection, uploadId, migrationId);

    public DateTimeOffset? GetGameMigrationExecution(Guid uploadId, string migrationId) =>
        GetMigrationExecution(OpenGameConnection, uploadId, migrationId);

    public bool IsApplicationMigrationApplied(string migrationId) =>
        IsMigrationApplied(OpenApplicationConnection, migrationId);

    public bool IsGameMigrationApplied(string migrationId) =>
        IsMigrationApplied(OpenGameConnection, migrationId);

    public MigrationTrackingState GetApplicationMigrationTracking() =>
        GetMigrationTracking(OpenApplicationConnection);

    public MigrationTrackingState GetGameMigrationTracking() =>
        GetMigrationTracking(OpenGameConnection);

    public void InitializeApplicationMigrationTracking(DateTimeOffset appliedAtUtc) =>
        InitializeMigrationTracking(OpenApplicationConnection, appliedAtUtc);

    public void InitializeGameMigrationTracking(DateTimeOffset appliedAtUtc) =>
        InitializeMigrationTracking(OpenGameConnection, appliedAtUtc);

    public void UpdateApplicationMigrationExecution(long executionId, DateTimeOffset appliedAtUtc) =>
        UpdateMigrationExecution(OpenApplicationConnection, executionId, appliedAtUtc);

    public void UpdateGameMigrationExecution(long executionId, DateTimeOffset appliedAtUtc) =>
        UpdateMigrationExecution(OpenGameConnection, executionId, appliedAtUtc);

    private static DateTimeOffset? GetMigrationExecution(
        Func<DbConnection> openConnection,
        Guid uploadId,
        string migrationId)
    {
        using var connection = openConnection();
        if (!TableExists(connection, "kcops.MigrationExecutions"))
            return null;

        var sql = uploadId == Guid.Empty
            ? """
                SELECT TOP (1) AppliedAtUtc
                FROM [kcops].[MigrationExecutions]
                WHERE MigrationId = @migrationId
                ORDER BY Id DESC;
                """
            : """
                SELECT AppliedAtUtc
                FROM [kcops].[MigrationExecutions]
                WHERE UploadId = @uploadId AND MigrationId = @migrationId;
                """;
        using var command = CreateCommand(connection, sql);
        AddParameter(command, "@uploadId", uploadId);
        AddParameter(command, "@migrationId", migrationId);
        var value = command.ExecuteScalar();
        return value is null || value is DBNull
            ? null
            : AsUtc(Convert.ToDateTime(value));
    }

    private static MigrationTrackingState GetMigrationTracking(Func<DbConnection> openConnection)
    {
        using var connection = openConnection();
        if (!TableExists(connection, "kcops.MigrationExecutions"))
            return new MigrationTrackingState(false, null, null, null);

        using var command = CreateCommand(connection, """
            SELECT TOP (1) Id, MigrationId, AppliedAtUtc
            FROM [kcops].[MigrationExecutions]
            ORDER BY Id DESC;
            """);
        using var reader = command.ExecuteReader();
        return !reader.Read()
            ? new MigrationTrackingState(true, null, null, null)
            : new MigrationTrackingState(
                true,
                reader.GetInt64(0),
                reader.GetString(1),
                AsUtc(reader.GetDateTime(2)));
    }

    private static void InitializeMigrationTracking(
        Func<DbConnection> openConnection,
        DateTimeOffset appliedAtUtc)
    {
        using var connection = openConnection();
        if (TableExists(connection, "kcops.MigrationExecutions"))
            throw new InvalidOperationException("A migrációkövetés már inicializálva van.");

        using var transaction = connection.BeginTransaction();
        using (var create = CreateCommand(connection, """
            IF SCHEMA_ID(N'kcops') IS NULL
                EXEC(N'CREATE SCHEMA [kcops]');

            CREATE TABLE [kcops].[MigrationExecutions]
            (
                [Id] bigint IDENTITY(1,1) NOT NULL CONSTRAINT [PK_kcops_MigrationExecutions] PRIMARY KEY,
                [UploadId] uniqueidentifier NOT NULL,
                [MigrationId] nvarchar(150) NOT NULL,
                [AppliedAtUtc] datetime2(7) NOT NULL,
                CONSTRAINT [UQ_kcops_MigrationExecutions_UploadId] UNIQUE ([UploadId])
            );
            """, transaction))
        {
            create.ExecuteNonQuery();
        }

        var latestMigration = GetLatestEfMigration(connection, transaction);
        if (latestMigration is not null)
        {
            using var seed = CreateCommand(connection, """
                INSERT INTO [kcops].[MigrationExecutions] ([UploadId], [MigrationId], [AppliedAtUtc])
                VALUES (@uploadId, @migrationId, @appliedAtUtc);
                """, transaction);
            AddParameter(seed, "@uploadId", Guid.NewGuid());
            AddParameter(seed, "@migrationId", latestMigration);
            AddParameter(seed, "@appliedAtUtc", appliedAtUtc.UtcDateTime);
            seed.ExecuteNonQuery();
        }

        transaction.Commit();
    }

    private static void UpdateMigrationExecution(
        Func<DbConnection> openConnection,
        long executionId,
        DateTimeOffset appliedAtUtc)
    {
        using var connection = openConnection();
        using var command = CreateCommand(connection, """
            UPDATE [kcops].[MigrationExecutions]
            SET AppliedAtUtc = @appliedAtUtc
            WHERE Id = @executionId;
            """);
        AddParameter(command, "@appliedAtUtc", appliedAtUtc.UtcDateTime);
        AddParameter(command, "@executionId", executionId);
        if (command.ExecuteNonQuery() != 1)
            throw new InvalidOperationException("A migrációs végrehajtás nem található.");
    }

    private static string? GetLatestEfMigration(DbConnection connection, DbTransaction transaction)
    {
        if (!TableExists(connection, "dbo.__EFMigrationsHistory", transaction))
            return null;

        using var command = CreateCommand(connection, """
            SELECT TOP (1) MigrationId
            FROM [__EFMigrationsHistory]
            ORDER BY MigrationId DESC;
            """, transaction);
        return command.ExecuteScalar() as string;
    }

    private static bool TableExists(
        DbConnection connection,
        string tableName,
        DbTransaction? transaction = null)
    {
        using var command = CreateCommand(
            connection,
            "SELECT CASE WHEN OBJECT_ID(@tableName, N'U') IS NULL THEN 0 ELSE 1 END;",
            transaction);
        AddParameter(command, "@tableName", tableName);
        return Convert.ToInt32(command.ExecuteScalar()) == 1;
    }

    private static bool IsMigrationApplied(
        Func<DbConnection> openConnection,
        string migrationId)
    {
        using var connection = openConnection();
        using var command = CreateCommand(
            connection,
            "SELECT COUNT(*) FROM [__EFMigrationsHistory] WHERE [MigrationId] = @migrationId;");
        AddParameter(command, "@migrationId", migrationId);
        return Convert.ToInt32(command.ExecuteScalar()) > 0;
    }

    private static DateTimeOffset AsUtc(DateTime value) =>
        new(DateTime.SpecifyKind(value, DateTimeKind.Utc));

    private static void ValidateQuestion(int categoryNo, string text, IReadOnlyList<string> answers)
    {
        if (categoryNo is < 1 or > 16)
            throw new InvalidOperationException("A kategória 1 és 16 közötti érték lehet.");
        if (string.IsNullOrWhiteSpace(text))
            throw new InvalidOperationException("A kérdésszöveg nem lehet üres.");
        if (answers.Count != 4 || answers.Any(string.IsNullOrWhiteSpace))
            throw new InvalidOperationException("Pontosan négy nem üres válasz szükséges. Az első válasz a helyes.");
    }

    private static void ValidateReported(int reported)
    {
        if (reported < 0)
            throw new InvalidOperationException("A Reported értéke nem lehet negatív.");
    }

    private DbConnection OpenApplicationConnection() => Open(_settings.ApplicationConnectionString);
    private DbConnection OpenGameConnection() => Open(_settings.GameConnectionString);

    private DbConnection Open(string connectionString)
    {
        DbConnection connection = _settings.Provider switch
        {
            AdminDatabaseProvider.SqlServer => new SqlConnection(connectionString),
            AdminDatabaseProvider.Sqlite => new SqliteConnection(connectionString),
            _ => throw new InvalidOperationException("Nem támogatott adatbázis-provider.")
        };
        connection.Open();
        return connection;
    }

    private static DbCommand CreateCommand(DbConnection connection, string sql, DbTransaction? transaction = null)
    {
        var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Transaction = transaction;
        return command;
    }

    private static void AddParameter(DbCommand command, string name, object? value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value ?? DBNull.Value;
        command.Parameters.Add(parameter);
    }

    private static object? ExecuteScalar(DbConnection connection, string sql)
    {
        using var command = CreateCommand(connection, sql);
        return command.ExecuteScalar();
    }

    private static void ExecuteNonQuery(DbConnection connection, string sql, DbTransaction transaction, params (string Name, object? Value)[] parameters)
    {
        using var command = CreateCommand(connection, sql, transaction);
        foreach (var parameter in parameters)
            AddParameter(command, parameter.Name, parameter.Value);
        command.ExecuteNonQuery();
    }

    private static bool ReadBool(DbDataReader reader, int ordinal) => Convert.ToBoolean(reader.GetValue(ordinal));

    public void Dispose()
    {
    }
}
