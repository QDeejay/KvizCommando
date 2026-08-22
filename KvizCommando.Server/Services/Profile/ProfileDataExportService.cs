using KvizCommando.Server.Application.Abstractions.Security;
using KvizCommando.Server.Domain.Entities.Billing;
using KvizCommando.Server.Domain.Entities.Compliance;
using KvizCommando.Server.Domain.Entities.Players;
using KvizCommando.Server.Domain.Entities.Questions;
using KvizCommando.Server.Domain.Entities.Statistics;
using KvizCommando.Server.Identity;
using KvizCommando.Server.Infrastructure.Persistence;
using KvizCommando.Server.Models;
using KvizCommando.Server.Services.PlayerCache;
using KvizCommando.Shared.Contracts.Profile;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System.Globalization;
using System.IO.Compression;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace KvizCommando.Server.Services.Profile;

public sealed class ProfileDataExportService : IProfileDataExportService
{
    private const int FORMAT_VERSION = 1;

    private static readonly JsonSerializerOptions JSON_OPTIONS = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
    };

    private readonly UserManager<ApplicationUser> _users;
    private readonly ApplicationDbContext _db;
    private readonly GameDbContext _gameDb;
    private readonly IProfileAccountService _accountService;
    private readonly IPlayerCacheService _cache;
    private readonly IRegistrationBenefitClaimService _benefitClaims;
    private readonly IStringLocalizer<ProfileDataExportService> _localizer;
    private readonly ILogger<ProfileDataExportService> _logger;

    public ProfileDataExportService(
        UserManager<ApplicationUser> users,
        ApplicationDbContext db,
        GameDbContext gameDb,
        IProfileAccountService accountService,
        IPlayerCacheService cache,
        IRegistrationBenefitClaimService benefitClaims,
        IStringLocalizer<ProfileDataExportService> localizer,
        ILogger<ProfileDataExportService> logger)
    {
        _users = users;
        _db = db;
        _gameDb = gameDb;
        _accountService = accountService;
        _cache = cache;
        _benefitClaims = benefitClaims;
        _localizer = localizer;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<ProfileDataExportServiceResult> ExportAsync(
        string userId,
        string currentPassword,
        CancellationToken ct = default)
    {
        try
        {
            var user = await _users.FindByIdAsync(userId);
            if (user is null)
                return Result(ProfileDataExportServiceState.NotFound);

            if (string.IsNullOrWhiteSpace(currentPassword) ||
                !await _users.CheckPasswordAsync(user, currentPassword))
                return Result(ProfileDataExportServiceState.InvalidPassword);

            var playerId = await _db.Players
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .Select(x => (int?)x.PlayerId)
                .SingleOrDefaultAsync(ct);

            if (playerId.HasValue)
            {
                await _cache.SaveDirtyLockedAsync(playerId.Value, ct);
                await _cache.SaveDirtyQuestionLockedAsync(playerId.Value, ct);
            }

            var source = await LoadSourceAsync(user, playerId, ct);
            var archive = BuildArchive(source);
            var displayName = string.IsNullOrWhiteSpace(user.DisplayName)
                ? "player"
                : user.DisplayName;
            var safeDisplayName = string.Concat(
                displayName.Select(character =>
                    Path.GetInvalidFileNameChars().Contains(character)
                        ? '-'
                        : character));

            return new ProfileDataExportServiceResult
            {
                State = ProfileDataExportServiceState.Success,
                FileName =
                    $"kvizcommando-{safeDisplayName}-adatexport-{DateTime.UtcNow:yyyyMMdd-HHmmss}.zip",
                Content = archive
            };
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Profile data export failed. UserId={UserId}",
                userId);
            return Result(ProfileDataExportServiceState.ServerError);
        }
    }

    private async Task<ProfileDataExportSource> LoadSourceAsync(
        ApplicationUser user,
        int? playerId,
        CancellationToken ct)
    {
        var accountResponse = await _accountService.GetAsync(user.Id, ct);
        var account = accountResponse.Account ?? new ProfileAccountDto
        {
            Email = user.Email ?? string.Empty
        };
        var logins = await _users.GetLoginsAsync(user);
        var benefitEligibleAgain = string.IsNullOrWhiteSpace(user.Email)
            ? null
            : await _benefitClaims.GetEligibleAgainAtUtcAsync(user.Email, ct);

        var termsConsents = await _db.TermsConsents
            .AsNoTracking()
            .Where(x => x.UserId == user.Id)
            .OrderBy(x => x.AcceptedAtUtc)
            .ToListAsync(ct);
        var marketingConsents = await _db.MarketingConsents
            .AsNoTracking()
            .Where(x => x.UserId == user.Id)
            .OrderBy(x => x.ChangedAtUtc)
            .ToListAsync(ct);
        var paymentMethods = await _db.UserPaymentMethods
            .AsNoTracking()
            .Where(x => x.UserId == user.Id)
            .OrderBy(x => x.Id)
            .ToListAsync(ct);

        Player? player = null;
        PlayerCharacter? characterData = null;
        PlayerLoadout? loadout = null;
        PlayerAskStats? askStats = null;
        TeamStatistic? teamStats = null;
        var categoryStats = new List<PlayerCategoryStat>();
        var orientStats = new List<PlayerOrientStat>();
        var userQuestions = new List<UserQuestion>();
        var pendingQuestions = new List<PendingQuestion>();

        if (playerId.HasValue)
        {
            player = await _db.Players.AsNoTracking()
                .SingleOrDefaultAsync(x => x.PlayerId == playerId.Value, ct);
            characterData = await _db.PlayerCharacters.AsNoTracking()
                .SingleOrDefaultAsync(x => x.PlayerId == playerId.Value, ct);
            loadout = await _db.PlayerLoadouts.AsNoTracking()
                .SingleOrDefaultAsync(x => x.PlayerId == playerId.Value, ct);
            askStats = await _db.PlayerAskStats.AsNoTracking()
                .SingleOrDefaultAsync(x => x.PlayerId == playerId.Value, ct);
            teamStats = await _db.TeamStatistics.AsNoTracking()
                .SingleOrDefaultAsync(x => x.PlayerId == playerId.Value, ct);
            categoryStats = await _db.PlayerCategoryStats.AsNoTracking()
                .Where(x => x.PlayerId == playerId.Value)
                .OrderBy(x => x.CategoryId)
                .ToListAsync(ct);
            orientStats = await _db.PlayerOrientStat.AsNoTracking()
                .Where(x => x.PlayerId == playerId.Value)
                .OrderBy(x => x.OrientId)
                .ToListAsync(ct);
            userQuestions = await _gameDb.UserQuestions.AsNoTracking()
                .Where(x => x.PlayerId == playerId.Value)
                .OrderBy(x => x.Id)
                .ToListAsync(ct);
            pendingQuestions = await _gameDb.PendingQuestions.AsNoTracking()
                .Where(x => x.PlayerId == playerId.Value)
                .OrderBy(x => x.Id)
                .ToListAsync(ct);
        }

        return new ProfileDataExportSource
        {
            User = user,
            Account = account,
            LoginProviders = logins.Select(x => x.LoginProvider).Distinct().ToArray(),
            BenefitEligibleAgainUtc = benefitEligibleAgain,
            TermsConsents = termsConsents,
            MarketingConsents = marketingConsents,
            PaymentMethods = paymentMethods,
            Player = player,
            Characters = ReadArray<CharachterSlot?>(characterData?.CharactersJson),
            Loadout = loadout,
            AskStats = askStats,
            CategoryStats = categoryStats,
            OrientStats = orientStats,
            TeamStats = teamStats,
            UserQuestions = userQuestions,
            PendingQuestions = pendingQuestions
        };
    }

    private byte[] BuildArchive(ProfileDataExportSource source)
    {
        var originalCulture = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = ResolveCulture(source.User.PreferredLocale);
            var document = BuildDocument(source);
            var json = JsonSerializer.Serialize(document, JSON_OPTIONS);

            using var output = new MemoryStream();
            using (var archive = new ZipArchive(
                       output,
                       ZipArchiveMode.Create,
                       leaveOpen: true))
            {
                var entry = archive.CreateEntry(
                    "kvizcommando-data.json",
                    CompressionLevel.Optimal);
                using var writer = new StreamWriter(
                    entry.Open(),
                    new UTF8Encoding(false));
                writer.Write(json);
            }

            return output.ToArray();
        }
        finally
        {
            CultureInfo.CurrentUICulture = originalCulture;
        }
    }

    private Dictionary<string, object?> BuildDocument(ProfileDataExportSource source)
    {
        var player = source.Player;
        return new Dictionary<string, object?>
        {
            [Localize("Section.Export")] = new Dictionary<string, object?>
            {
                [Localize("Field.FormatVersion")] = FORMAT_VERSION,
                [Localize("Field.ExportedUtc")] = DateTime.UtcNow,
                [Localize("Field.Language")] = source.User.PreferredLocale
            },
            [Localize("Section.Account")] = BuildAccount(source),
            [Localize("Section.Contact")] = BuildContact(source.Account),
            [Localize("Section.PaymentMethods")] = source.PaymentMethods.Select(BuildPaymentMethod).ToArray(),
            [Localize("Section.Consents")] = BuildConsents(source),
            [Localize("Section.RegistrationBenefit")] = new Dictionary<string, object?>
            {
                [Localize("Field.EligibleAgainUtc")] = source.BenefitEligibleAgainUtc
            },
            [Localize("Section.PlayerProfile")] = player is null ? null : BuildPlayer(player),
            [Localize("Section.Characters")] = BuildCharacters(source.Characters),
            [Localize("Section.GameSettings")] = BuildGameSettings(source.Loadout),
            [Localize("Section.AskStatistics")] = BuildAskStatistics(source.AskStats),
            [Localize("Section.CategoryStatistics")] = source.CategoryStats.Select(BuildCategoryStatistic).ToArray(),
            [Localize("Section.OrientationStatistics")] = source.OrientStats.Select(BuildOrientationStatistic).ToArray(),
            [Localize("Section.TeamStatistics")] = BuildTeamStatistics(source.TeamStats),
            [Localize("Section.UserQuestions")] = source.UserQuestions.Select(BuildUserQuestion).ToArray(),
            [Localize("Section.PendingQuestions")] = source.PendingQuestions.Select(BuildPendingQuestion).ToArray()
        };
    }

    private Dictionary<string, object?> BuildAccount(ProfileDataExportSource source) => new()
    {
        [Localize("Field.UserId")] = source.User.Id,
        [Localize("Field.Email")] = source.User.Email,
        [Localize("Field.EmailConfirmed")] = source.User.EmailConfirmed,
        [Localize("Field.DisplayName")] = source.User.DisplayName,
        [Localize("Field.PreferredLocale")] = source.User.PreferredLocale,
        [Localize("Field.RegisteredUtc")] = source.User.CreatedAtUtc,
        [Localize("Field.TwoFactorEnabled")] = source.User.TwoFactorEnabled,
        [Localize("Field.AccessFailedCount")] = source.User.AccessFailedCount,
        [Localize("Field.LockoutEnd")] = source.User.LockoutEnd,
        [Localize("Field.LoginProviders")] = source.LoginProviders
    };

    private Dictionary<string, object?> BuildContact(ProfileAccountDto account) => new()
    {
        [Localize("Field.Phone")] = new Dictionary<string, object?>
        {
            [Localize("Field.CountryCode")] = account.Phone.CountryCode,
            [Localize("Field.PhoneNumber")] = account.Phone.Number
        },
        [Localize("Field.BillingName")] = new Dictionary<string, object?>
        {
            [Localize("Field.LastName")] = account.BillingName.LastName,
            [Localize("Field.FirstName")] = account.BillingName.FirstName
        },
        [Localize("Field.BillingAddress")] = new Dictionary<string, object?>
        {
            [Localize("Field.PostalCode")] = account.BillingAddress.PostalCode,
            [Localize("Field.City")] = account.BillingAddress.City,
            [Localize("Field.AddressLine1")] = account.BillingAddress.AddressLine1,
            [Localize("Field.AddressLine2")] = account.BillingAddress.AddressLine2
        }
    };

    private Dictionary<string, object?> BuildPaymentMethod(UserPaymentMethod method) => new()
    {
        [Localize("Field.Processor")] = method.Processor,
        [Localize("Field.CardBrand")] = method.CardBrand,
        [Localize("Field.CardLast4")] = method.CardLast4,
        [Localize("Field.ExpirationMonth")] = method.ExpMonth,
        [Localize("Field.ExpirationYear")] = method.ExpYear,
        [Localize("Field.IsDefault")] = method.IsDefault,
        [Localize("Field.CreatedUtc")] = method.CreatedUtc,
        [Localize("Field.UpdatedUtc")] = method.UpdatedUtc
    };

    private Dictionary<string, object?> BuildConsents(ProfileDataExportSource source) => new()
    {
        [Localize("Field.TermsAccepted")] = source.User.AcceptTerms,
        [Localize("Field.MarketingConsentCurrent")] = source.User.MarketingConsent,
        [Localize("Field.TermsConsentHistory")] = source.TermsConsents.Select(x =>
            new Dictionary<string, object?>
            {
                [Localize("Field.Version")] = x.TermsVersion,
                [Localize("Field.AcceptedUtc")] = x.AcceptedAtUtc
            }).ToArray(),
        [Localize("Field.MarketingConsentHistory")] = source.MarketingConsents.Select(x =>
            new Dictionary<string, object?>
            {
                [Localize("Field.Granted")] = x.Granted,
                [Localize("Field.ChangedUtc")] = x.ChangedAtUtc
            }).ToArray()
    };

    private Dictionary<string, object?> BuildPlayer(Player player) => new()
    {
        [Localize("Field.PlayerId")] = player.PlayerId,
        [Localize("Field.TeamName")] = player.TeamName,
        [Localize("Field.CaptainAvatar")] = player.CaptainAvatar,
        [Localize("Field.TeamNameChangedUtc")] = player.TeamNameChangedUtc,
        [Localize("Field.Rank")] = player.RankEnum,
        [Localize("Field.Xp")] = player.XP,
        [Localize("Field.Credit")] = player.Credit,
        [Localize("Field.DevelopmentPoints")] = player.DevPoint,
        [Localize("Field.Voucher")] = player.Voucher,
        [Localize("Field.CreatedUtc")] = player.CreatedUtc,
        [Localize("Field.UpdatedUtc")] = player.UpdatedUtc
    };

    private object[] BuildCharacters(CharachterSlot?[] characters) =>
        characters.Select((character, index) => (character, index))
            .Where(x => x.character is not null)
            .Select(x => BuildCharacter(x.character!, x.index + 1))
            .ToArray();

    private Dictionary<string, object?> BuildCharacter(CharachterSlot character, int slot) => new()
    {
        [Localize("Field.CharacterSlot")] = slot,
        [Localize("Field.Name")] = character.Name,
        [Localize("Field.PictureCode")] = character.PictureCode,
        [Localize("Field.Rank")] = character.Rank,
        [Localize("Field.Pension")] = character.Pension,
        [Localize("Field.Xp")] = character.XP,
        [Localize("Field.DevelopmentPoints")] = character.DevPoints,
        [Localize("Field.Energy")] = character.EnergyPoints,
        [Localize("Field.NextHealingGameUtc")] = character.NextHealingGameUtc,
        [Localize("Field.Attributes")] = new Dictionary<string, object?>
        {
            [Localize("Field.MainOrientation")] = BuildAttitude(character.Attitude.Main),
            [Localize("Field.SecondaryOrientation")] = BuildAttitude(character.Attitude.Secondary),
            [Localize("Field.GeneralAttributes")] = BuildAttitude(character.Attitude.Gender)
        },
        [Localize("Field.Statistics")] = new Dictionary<string, object?>
        {
            [Localize("Field.PlayedDuels")] = character.CharStatistic.PlayDuels,
            [Localize("Field.WonDuels")] = character.CharStatistic.WinDuels,
            [Localize("Field.SoloBestScore")] = character.CharStatistic.SoloBestScore
        }
    };

    private Dictionary<string, object?> BuildAttitude(AttitudeBranch branch) => new()
    {
        [Localize("Field.Categories")] = branch.CatNo,
        [Localize("Field.Levels")] = branch.Level
    };

    private Dictionary<string, object?> BuildGameSettings(PlayerLoadout? loadout) => new()
    {
        [Localize("Field.FactoryQuestionSlots")] = ReadArray<int>(loadout?.FactorySlotsJson),
        [Localize("Field.HelpLevels")] = ReadArray<int>(loadout?.HelpLevelsJson)
    };

    private Dictionary<string, object?> BuildAskStatistics(PlayerAskStats? stats) => new()
    {
        [Localize("Field.TotalQuestionsAsked")] = stats?.TotalQuestionsAsked ?? 0,
        [Localize("Field.TotalAskPointsEarned")] = stats?.TotalAskPointsEarned ?? 0,
        [Localize("Field.AveragePointsPerAsk")] = stats?.AveragePointsPerAsk ?? 0
    };

    private Dictionary<string, object?> BuildCategoryStatistic(PlayerCategoryStat stats) => new()
    {
        [Localize("Field.Category")] = stats.CategoryId,
        [Localize("Field.Answered")] = stats.Answered,
        [Localize("Field.Correct")] = stats.Correct,
        [Localize("Field.Ratio")] = stats.Ratio,
        [Localize("Field.HighScore")] = stats.HighScore,
        [Localize("Field.HighScoreTime")] = stats.HighScoreTime
    };

    private Dictionary<string, object?> BuildOrientationStatistic(PlayerOrientStat stats) => new()
    {
        [Localize("Field.Orientation")] = stats.OrientId,
        [Localize("Field.HighScore")] = stats.HighScore,
        [Localize("Field.HighScoreTime")] = stats.HighScoreTime
    };

    private Dictionary<string, object?> BuildTeamStatistics(TeamStatistic? stats)
    {
        if (stats is null)
            return [];

        var placements = ReadObject(
            stats.RankedPlacementsJson,
            new RankedPlacementStatistic());
        return new Dictionary<string, object?>
        {
            [Localize("Field.RankedPlayed")] = stats.RankedPlayed,
            [Localize("Field.RankedWon")] = stats.RankedWon,
            [Localize("Field.RankedHighScore")] = stats.RankedHighScore,
            [Localize("Field.RankedHighScoreTime")] = stats.RankedHighScoreTime,
            [Localize("Field.RankedGuessCount")] = stats.RankedGuessCount,
            [Localize("Field.RankedGuessErrorTotal")] = stats.RankedGuessErrorTotal,
            [Localize("Field.RankedGuessErrorRatio")] = stats.RankedGuessErrorRatio,
            [Localize("Field.Placements")] = new Dictionary<string, object?>
            {
                [Localize("Field.TwoPlayerMatches")] = placements.Players2,
                [Localize("Field.ThreePlayerMatches")] = placements.Players3,
                [Localize("Field.FourPlayerMatches")] = placements.Players4
            }
        };
    }

    private Dictionary<string, object?> BuildUserQuestion(UserQuestion question) => new()
    {
        [Localize("Field.QuestionId")] = question.Id,
        [Localize("Field.Category")] = question.CategoryNo,
        [Localize("Field.Question")] = question.Question,
        [Localize("Field.Answers")] = ReadArray<string>(question.AnswersJson),
        [Localize("Field.AskCount")] = question.Ask,
        [Localize("Field.CorrectAnswerCount")] = question.OkAnswer,
        [Localize("Field.Ratio")] = question.Ratio,
        [Localize("Field.ReportCount")] = question.Reported
    };

    private Dictionary<string, object?> BuildPendingQuestion(PendingQuestion question) => new()
    {
        [Localize("Field.QuestionId")] = question.Id,
        [Localize("Field.Category")] = question.CategoryNo,
        [Localize("Field.Question")] = question.Question,
        [Localize("Field.Answers")] = ReadArray<string>(question.AnswersJson),
        [Localize("Field.Status")] = Localize($"QuestionStatus.{question.Status}"),
        [Localize("Field.Remark")] = question.Remark,
        [Localize("Field.SubmittedUtc")] = question.SubmittedAt,
        [Localize("Field.ReportCount")] = question.Reported
    };

    private string Localize(string key) => _localizer[key].Value;

    private static CultureInfo ResolveCulture(string? preferredLocale) =>
        CultureInfo.GetCultureInfo(
            preferredLocale?.StartsWith("en", StringComparison.OrdinalIgnoreCase) == true
                ? "en-US"
                : "hu-HU");

    private static T[] ReadArray<T>(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return [];

        try
        {
            return JsonSerializer.Deserialize<T[]>(json) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static T ReadObject<T>(string? json, T fallback)
        where T : class
    {
        if (string.IsNullOrWhiteSpace(json))
            return fallback;

        try
        {
            return JsonSerializer.Deserialize<T>(json) ?? fallback;
        }
        catch (JsonException)
        {
            return fallback;
        }
    }

    private static ProfileDataExportServiceResult Result(
        ProfileDataExportServiceState state) => new()
        {
            State = state
        };

    private sealed class ProfileDataExportSource
    {
        public ApplicationUser User { get; set; } = default!;
        public ProfileAccountDto Account { get; set; } = new();
        public string[] LoginProviders { get; set; } = [];
        public DateTime? BenefitEligibleAgainUtc { get; set; }
        public List<TermsConsent> TermsConsents { get; set; } = [];
        public List<MarketingConsent> MarketingConsents { get; set; } = [];
        public List<UserPaymentMethod> PaymentMethods { get; set; } = [];
        public Player? Player { get; set; }
        public CharachterSlot?[] Characters { get; set; } = [];
        public PlayerLoadout? Loadout { get; set; }
        public PlayerAskStats? AskStats { get; set; }
        public List<PlayerCategoryStat> CategoryStats { get; set; } = [];
        public List<PlayerOrientStat> OrientStats { get; set; } = [];
        public TeamStatistic? TeamStats { get; set; }
        public List<UserQuestion> UserQuestions { get; set; } = [];
        public List<PendingQuestion> PendingQuestions { get; set; } = [];
    }
}