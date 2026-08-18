using KvizCommando.Client;
using KvizCommando.Infrastructure.Email;
using KvizCommando.Server.Identity;
using KvizCommando.Server.Infrastructure.Options;
using Microsoft.AspNetCore.Builder.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Text;

namespace KvizCommando.Server.Infrastructure.Email;

public sealed class WhitelistedEmailSender : IEmailSender<ApplicationUser>
{
    private readonly ILogger<WhitelistedEmailSender> _logger;
    private readonly IStringLocalizer<WhitelistedEmailSender> _localizer;
    private readonly AppOptions _options;

    public WhitelistedEmailSender(
        ILogger<WhitelistedEmailSender> logger,
        IStringLocalizer<WhitelistedEmailSender> localizer,
        IOptions<AppOptions> appoptions)     
    {
        _logger = logger;
        _localizer = localizer;
        _options = appoptions.Value;
    }

    /// <summary>
    /// Előkészíti és fájlba írja a regisztrációt megerősítő levelet.
    /// </summary>
    /// <param name="user">Az érintett Identity-felhasználó.</param>
    /// <param name="email">A feldolgozandó e-mail-cím.</param>
    /// <param name="confirmationLink">A felhasználói fiók megerősítésére szolgáló hivatkozás.</param>
    public async Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink)
    {
        var culture = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.ToLowerInvariant();
        if (culture != "hu" && culture != "en")
            culture = "en";

        var (subject, htmlBody, textBody) =
            await LoadTemplateAsync("RegistrationConfirm", culture, confirmationLink,"confirm");

        _logger.LogInformation("Confirmation email sent to {Email}. Subject: {Subject}", user.Email, subject);

        var delivery = new FileEmailDelivery();
        await delivery.WriteAsync(user.Email, "no-reply@kvizcommando.local",
                                  subject, textBody, htmlBody, CancellationToken.None);
    }

    /// <summary>
    /// Előkészíti és fájlba írja a jelszó-visszaállító hivatkozást tartalmazó levelet.
    /// </summary>
    /// <param name="user">Az érintett Identity-felhasználó.</param>
    /// <param name="email">A feldolgozandó e-mail-cím.</param>
    /// <param name="resetLink">A jelszó visszaállítására szolgáló hivatkozás.</param>
    public async Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink)
    {
        var culture = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.ToLowerInvariant();
        if (culture != "hu" && culture != "en")
            culture = "en";

        var (subject, htmlBody, textBody) =
            await LoadTemplateAsync("ResetPassword", culture, resetLink, "reset-password");

        _logger.LogInformation("Password reset link sent to {Email}. Subject: {Subject}", user.Email, subject);

        var delivery = new FileEmailDelivery();
        await delivery.WriteAsync(user.Email, "no-reply@kvizcommando.local",
                                  subject, textBody, htmlBody, CancellationToken.None);
    }

    /// <summary>
    /// Előkészíti és fájlba írja a jelszó-visszaállító kódot tartalmazó levelet.
    /// </summary>
    /// <param name="user">Az érintett Identity-felhasználó.</param>
    /// <param name="email">A feldolgozandó e-mail-cím.</param>
    /// <param name="resetCode">A jelszó-visszaállítás egyszer használatos kódja.</param>
    public async Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode)
    {
        var resetLink = $"https://Localhost:7229/Auth/reset-password?email={Uri.EscapeDataString(email)}&code={Uri.EscapeDataString(resetCode)}\r\n";
        var culture = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.ToLowerInvariant();
        if (culture != "hu" && culture != "en")
            culture = "en";

        var (subject, htmlBody, textBody) =
            await LoadTemplateAsync("ResetPassword", culture, resetLink, "reset-password");

        _logger.LogInformation("Password reset email sent to {Email}. Subject: {Subject}", user.Email, subject);

        var delivery = new FileEmailDelivery();
        await delivery.WriteAsync(user.Email, "no-reply@kvizcommando.local",
                                  subject, textBody, htmlBody, CancellationToken.None);
    }
    
    private async Task<(string subject, string htmlBody, string textBody)> LoadTemplateAsync(
    string baseName, string culture, string confirmationLink, string page)
    {
        var appName = _options.Name;
        var supporEmail = _options.SupportEmail;
        var hours = _options.TokenValidityHours;
        var TempHost = _options.TempServerIp;
        var appUrl = _options.WebUrl;
        var uri = new Uri(confirmationLink);
        var query = uri.Query;
        var pagePath = $"/auth/{page}";

        // A callback jelenleg az ideiglenes publikus fejlesztői hostra mutat.
        // Az éles kézbesítési és hostkonfigurációt a docs/infrastructure-status.md rögzíti.
        var customLink = $"https://kviz-commando.ngrok.app{pagePath}{query}";

        var templateDir = Path.Combine(AppContext.BaseDirectory,
            "Infrastructure", "Email", "Templates", "EmailTemplates", "Auth");

        var htmlPath = Path.Combine(templateDir, $"{baseName}.{culture}.html");
        var txtPath = Path.Combine(templateDir, $"{baseName}.{culture}.txt");

        var htmlBody = File.Exists(htmlPath)
            ? await File.ReadAllTextAsync(htmlPath, Encoding.UTF8)
            : "";
        var textBody = File.Exists(txtPath)
            ? await File.ReadAllTextAsync(txtPath, Encoding.UTF8)
            : "";
       
        var DisplayName = _localizer["DisplayName.Fallback"];
        htmlBody = htmlBody
                    .Replace("{{AppName}}", appName)
                    .Replace("{{DisplayName}}", DisplayName)
                     .Replace("{{ConfirmUrl}}", customLink)
                     .Replace("{{TokenValidityHours}}", hours.ToString())
                     .Replace("{{SupportEmail}}", supporEmail)
                     .Replace("{{Year}}", DateTime.UtcNow.Year.ToString());

        textBody = textBody
            .Replace("{{AppName}}", appName)
            .Replace("{{DisplayName}}", DisplayName)
            .Replace("{{ConfirmUrl}}", customLink)
            .Replace("{{TokenValidityHours}}", hours.ToString())
            .Replace("{{SupportEmail}}", supporEmail)
            .Replace("{{Year}}", DateTime.UtcNow.Year.ToString());


        var subjectKey = baseName == "RegistrationConfirm"
            ? "Email.Confirm.Subject"
            : "Email.Reset.Subject";
       
        var subject = _localizer[subjectKey].Value;
        subject = subject.Replace("{{AppName}}", appName);  
        return (subject, htmlBody, textBody);
    }

}
