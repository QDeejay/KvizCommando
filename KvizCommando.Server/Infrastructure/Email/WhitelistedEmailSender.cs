using KvizCommando.Server.Identity;
using KvizCommando.Server.Infrastructure.Options;
using KvizCommando.Server.Infrastructure.Persistence;
using KvizCommando.Shared.Models.Ranks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Net;
using System.Text;

namespace KvizCommando.Server.Infrastructure.Email;

/// <summary>Elkészíti az Identity által igényelt, lokalizált rendszerleveleket.</summary>
public sealed class WhitelistedEmailSender : IEmailSender<ApplicationUser>, IAccountNotificationSender
{
    private readonly ILogger<WhitelistedEmailSender> _logger;
    private readonly IStringLocalizer<WhitelistedEmailSender> _localizer;
    private readonly AppOptions _appOptions;
    private readonly EmailOptions _emailOptions;
    private readonly IEmailDelivery _delivery;
    private readonly ICallbackUrlValidator _callbackUrlValidator;
    private readonly IServiceScopeFactory _scopeFactory;

    public WhitelistedEmailSender(
        ILogger<WhitelistedEmailSender> logger,
        IStringLocalizer<WhitelistedEmailSender> localizer,
        IOptions<AppOptions> appOptions,
        IOptions<EmailOptions> emailOptions,
        IEmailDelivery delivery,
        ICallbackUrlValidator callbackUrlValidator,
        IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _localizer = localizer;
        _appOptions = appOptions.Value;
        _emailOptions = emailOptions.Value;
        _delivery = delivery;
        _callbackUrlValidator = callbackUrlValidator;
        _scopeFactory = scopeFactory;
    }

    /// <inheritdoc />
    public Task SendConfirmationLinkAsync(
        ApplicationUser user,
        string email,
        string confirmationLink)
    {
        var decodedLink = WebUtility.HtmlDecode(confirmationLink);
        var query = GetQuery(decodedLink);

        var isEmailChange = query.Contains(
            "changedEmail=",
            StringComparison.OrdinalIgnoreCase);

        return CreateAndDeliverAsync(
            user,
            isEmailChange
                ? EmailMessageType.EmailChange
                : EmailMessageType.Registration,
            email,
            isEmailChange
                ? "EmailChangeConfirm"
                : "RegistrationConfirm",
            "auth/confirm",
            query,
            CancellationToken.None);
    }
    /// <inheritdoc />
    public Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink) =>
        CreateAndDeliverAsync(user, EmailMessageType.PasswordReset, email, "ResetPassword",
            "auth/reset-password", GetQuery(resetLink), CancellationToken.None);

    /// <inheritdoc />
    public Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode)
    {
        var query = QueryString.Create(new Dictionary<string, string?>
        {
            ["email"] = email,
            ["code"] = resetCode
        }).Value;
        return CreateAndDeliverAsync(user, EmailMessageType.PasswordReset, email, "ResetPassword",
            "auth/reset-password", query, CancellationToken.None);
    }

    /// <inheritdoc />
    public Task SendPasswordChangedAsync(ApplicationUser user, CancellationToken ct = default) =>
        CreateAndDeliverAsync(user, EmailMessageType.PasswordChanged, user.Email ?? string.Empty,
            "PasswordChanged", string.Empty, string.Empty, ct);

    private async Task CreateAndDeliverAsync(ApplicationUser user, EmailMessageType type,
        string recipient, string templateName, string targetPath, string query, CancellationToken ct)
    {
        var culture = GetSupportedCulture();
        var targetUrl = string.IsNullOrEmpty(targetPath) ? string.Empty : BuildTargetUrl(targetPath, query);
        var rankEnum = await GetRankEnumAsync(user.Id, ct);
        var content = await LoadTemplateAsync(templateName, culture, targetUrl,
            RankCatalog.GetName(rankEnum, culture), ct);

        await _delivery.DeliverAsync(new EmailMessage(type, recipient,
            "no-reply@kvizcommando.local", content.Subject, content.TextBody, content.HtmlBody), ct);
        _logger.LogInformation("A {EmailType} típusú levél átadásra került.", type);
    }

    private async Task<(string Subject, string HtmlBody, string TextBody)> LoadTemplateAsync(
        string baseName, string culture, string targetUrl, string rankName, CancellationToken ct)
    {
        var directory = Path.Combine(AppContext.BaseDirectory, "Infrastructure", "Email",
            "Templates", "EmailTemplates", "Auth");
        var htmlPath = Path.Combine(directory, $"{baseName}.{culture}.html");
        var textPath = Path.Combine(directory, $"{baseName}.{culture}.txt");
        if (!File.Exists(htmlPath) || !File.Exists(textPath))
            throw new FileNotFoundException($"Hiányzik a(z) {baseName}.{culture} e-mail-sablon.");

        var html = ReplaceTokens(await File.ReadAllTextAsync(htmlPath, Encoding.UTF8, ct), rankName, targetUrl);
        var text = ReplaceTokens(await File.ReadAllTextAsync(textPath, Encoding.UTF8, ct), rankName, targetUrl);
        var subjectKey = baseName switch
        {
            "RegistrationConfirm" => "Email.Confirm.Subject",
            "EmailChangeConfirm" => "Email.Subject.EmailChange",
            "PasswordChanged" => "Email.Subject.PasswordChanged",
            _ => "Email.Subject.ResetPassword"
        };
        return (_localizer[subjectKey].Value.Replace("{{AppName}}", _appOptions.Name), html, text);
    }

    private string ReplaceTokens(string template, string rankName, string targetUrl) => template
        .Replace("{{AppName}}", _appOptions.Name).Replace("{{RankName}}", rankName)
        .Replace("{{DisplayName}}", rankName).Replace("{{ConfirmUrl}}", targetUrl)
        .Replace("{{TokenValidityHours}}", _appOptions.TokenValidityHours.ToString())
        .Replace("{{SupportEmail}}", _appOptions.SupportEmail)
        .Replace("{{Year}}", DateTime.UtcNow.Year.ToString());

    private string BuildTargetUrl(string targetPath, string query)
    {
        var builder = new UriBuilder(_emailOptions.GetActiveBaseUri())
        {
            Path = targetPath.TrimStart('/'),
            Query = query.TrimStart('?')
        };
        var url = builder.Uri.AbsoluteUri;
        if (!_callbackUrlValidator.IsAllowedAbsoluteUrl(url))
            throw new InvalidOperationException("Az e-mail visszahívási URL-je nem engedélyezett hostra mutat.");
        return url;
    }

    private static string GetQuery(string absoluteUrl) =>
        Uri.TryCreate(absoluteUrl, UriKind.Absolute, out var uri)
            ? uri.Query
            : throw new InvalidOperationException("Az Identity érvénytelen visszahívási URL-t adott át.");

    private static string GetSupportedCulture()
    {
        var culture = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.ToLowerInvariant();
        return culture is "hu" or "en" ? culture : "en";
    }

    private async Task<int> GetRankEnumAsync(
        string userId,
        CancellationToken ct)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();

        var db = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        return await db.Players
            .AsNoTracking()
            .Where(player => player.UserId == userId)
            .Select(player => (int?)player.RankEnum)
            .SingleOrDefaultAsync(ct) ?? 0;
    }
}
