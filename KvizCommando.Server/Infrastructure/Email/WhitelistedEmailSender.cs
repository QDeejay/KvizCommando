using KvizCommando.Server.Identity;
using KvizCommando.Server.Infrastructure.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Text;

namespace KvizCommando.Server.Infrastructure.Email;

/// <summary>
/// Elkészíti az Identity által igényelt, lokalizált rendszerleveleket.
/// A kézbesítés módját az <see cref="IEmailDelivery"/> aktuális implementációja határozza meg.
/// </summary>
public sealed class WhitelistedEmailSender : IEmailSender<ApplicationUser>
{
    private readonly ILogger<WhitelistedEmailSender> _logger;
    private readonly IStringLocalizer<WhitelistedEmailSender> _localizer;
    private readonly AppOptions _appOptions;
    private readonly EmailOptions _emailOptions;
    private readonly IEmailDelivery _delivery;
    private readonly ICallbackUrlValidator _callbackUrlValidator;

    public WhitelistedEmailSender(
        ILogger<WhitelistedEmailSender> logger,
        IStringLocalizer<WhitelistedEmailSender> localizer,
        IOptions<AppOptions> appOptions,
        IOptions<EmailOptions> emailOptions,
        IEmailDelivery delivery,
        ICallbackUrlValidator callbackUrlValidator)
    {
        _logger = logger;
        _localizer = localizer;
        _appOptions = appOptions.Value;
        _emailOptions = emailOptions.Value;
        _delivery = delivery;
        _callbackUrlValidator = callbackUrlValidator;
    }

    /// <inheritdoc />
    public Task SendConfirmationLinkAsync(
        ApplicationUser user,
        string email,
        string confirmationLink)
    {
        return CreateAndDeliverAsync(
            EmailMessageType.Registration,
            email,
            "RegistrationConfirm",
            "auth/confirm",
            GetQuery(confirmationLink));
    }

    /// <inheritdoc />
    public Task SendPasswordResetLinkAsync(
        ApplicationUser user,
        string email,
        string resetLink)
    {
        return CreateAndDeliverAsync(
            EmailMessageType.PasswordReset,
            email,
            "ResetPassword",
            "auth/reset-password",
            GetQuery(resetLink));
    }

    /// <inheritdoc />
    public Task SendPasswordResetCodeAsync(
        ApplicationUser user,
        string email,
        string resetCode)
    {
        var query = QueryString.Create(new Dictionary<string, string?>
        {
            ["email"] = email,
            ["code"] = resetCode
        }).Value;

        return CreateAndDeliverAsync(
            EmailMessageType.PasswordReset,
            email,
            "ResetPassword",
            "auth/reset-password",
            query);
    }

    private async Task CreateAndDeliverAsync(
        EmailMessageType type,
        string recipient,
        string templateName,
        string targetPath,
        string query)
    {
        var culture = GetSupportedCulture();
        var targetUrl = BuildTargetUrl(targetPath, query);
        var content = await LoadTemplateAsync(
            templateName,
            culture,
            targetUrl);

        var message = new EmailMessage(
            type,
            recipient,
            "no-reply@kvizcommando.local",
            content.Subject,
            content.TextBody,
            content.HtmlBody);

        await _delivery.DeliverAsync(message, CancellationToken.None);
        _logger.LogInformation(
            "A {EmailType} típusú Identity-levél átadásra került a kézbesítő adapternek.",
            type);
    }

    private async Task<(string Subject, string HtmlBody, string TextBody)> LoadTemplateAsync(
        string baseName,
        string culture,
        string targetUrl)
    {
        var templateDirectory = Path.Combine(
            AppContext.BaseDirectory,
            "Infrastructure",
            "Email",
            "Templates",
            "EmailTemplates",
            "Auth");

        var htmlPath = Path.Combine(templateDirectory, $"{baseName}.{culture}.html");
        var textPath = Path.Combine(templateDirectory, $"{baseName}.{culture}.txt");

        if (!File.Exists(htmlPath) || !File.Exists(textPath))
        {
            throw new FileNotFoundException(
                $"Hiányzik a(z) {baseName}.{culture} e-mail-sablon.");
        }

        var htmlBody = await File.ReadAllTextAsync(htmlPath, Encoding.UTF8);
        var textBody = await File.ReadAllTextAsync(textPath, Encoding.UTF8);
        var displayName = _localizer["DisplayName.Fallback"].Value;

        htmlBody = ReplaceTemplateTokens(htmlBody, displayName, targetUrl);
        textBody = ReplaceTemplateTokens(textBody, displayName, targetUrl);

        var subjectKey = baseName == "RegistrationConfirm"
            ? "Email.Confirm.Subject"
            : "Email.Subject.ResetPassword";
        var subject = _localizer[subjectKey].Value
            .Replace("{{AppName}}", _appOptions.Name);

        return (subject, htmlBody, textBody);
    }

    private string ReplaceTemplateTokens(
        string template,
        string displayName,
        string targetUrl) => template
        .Replace("{{AppName}}", _appOptions.Name)
        .Replace("{{DisplayName}}", displayName)
        .Replace("{{ConfirmUrl}}", targetUrl)
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

        var targetUrl = builder.Uri.AbsoluteUri;
        if (!_callbackUrlValidator.IsAllowedAbsoluteUrl(targetUrl))
        {
            throw new InvalidOperationException(
                "Az e-mail visszahívási URL-je nem engedélyezett hostra mutat.");
        }

        return targetUrl;
    }

    private static string GetQuery(string absoluteUrl)
    {
        if (!Uri.TryCreate(absoluteUrl, UriKind.Absolute, out var uri))
        {
            throw new InvalidOperationException(
                "Az Identity érvénytelen visszahívási URL-t adott át az e-mail-küldőnek.");
        }

        return uri.Query;
    }

    private static string GetSupportedCulture()
    {
        var culture = CultureInfo.CurrentUICulture
            .TwoLetterISOLanguageName
            .ToLowerInvariant();

        return culture is "hu" or "en" ? culture : "en";
    }
}
