using Microsoft.Extensions.Options;
using System.Text;

namespace KvizCommando.Server.Infrastructure.Email;

/// <summary>
/// A leveleket fejlesztési és bemutatási célból helyi fájlokba menti.
/// </summary>
public sealed class FileEmailDelivery : IEmailDelivery
{
    private readonly EmailOptions _options;
    private readonly ILogger<FileEmailDelivery> _logger;

    public FileEmailDelivery(
        IOptions<EmailOptions> options,
        ILogger<FileEmailDelivery> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task DeliverAsync(
        EmailMessage message,
        CancellationToken cancellationToken = default)
    {
        ValidateHeaderValue(message.To, nameof(message.To));
        ValidateHeaderValue(message.From, nameof(message.From));
        ValidateHeaderValue(message.Subject, nameof(message.Subject));

        var messageDirectory = Path.Combine(
            _options.OutputRoot,
            GetDirectoryName(message.Type));
        Directory.CreateDirectory(messageDirectory);

        var baseFileName = $"{DateTime.UtcNow:yyyyMMdd_HHmmssfff}_{Guid.NewGuid():N}";
        var emlPath = Path.Combine(messageDirectory, baseFileName + ".eml");
        var htmlPath = Path.Combine(messageDirectory, baseFileName + ".html");
        var textPath = Path.Combine(messageDirectory, baseFileName + ".txt");

        await File.WriteAllTextAsync(
            emlPath,
            BuildEml(message),
            Encoding.UTF8,
            cancellationToken);
        await File.WriteAllTextAsync(
            htmlPath,
            message.HtmlBody,
            Encoding.UTF8,
            cancellationToken);
        await File.WriteAllTextAsync(
            textPath,
            message.TextBody,
            Encoding.UTF8,
            cancellationToken);

        _logger.LogInformation(
            "A {EmailType} típusú fejlesztési levél fájljai elkészültek a következő könyvtárban: {Directory}",
            message.Type,
            messageDirectory);
    }

    public static string GetDirectoryName(EmailMessageType type) => type switch
    {
        EmailMessageType.Registration => "Registration",
        EmailMessageType.PasswordReset => "PasswordReset",
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
    };

    private static string BuildEml(EmailMessage message)
    {
        var boundary = $"KvizCommando_{Guid.NewGuid():N}";
        var encodedSubject = Convert.ToBase64String(
            Encoding.UTF8.GetBytes(message.Subject));
        var builder = new StringBuilder();

        builder.AppendLine($"From: {message.From}");
        builder.AppendLine($"To: {message.To}");
        builder.AppendLine($"Date: {DateTimeOffset.UtcNow:R}");
        builder.AppendLine($"Subject: =?UTF-8?B?{encodedSubject}?=");
        builder.AppendLine("MIME-Version: 1.0");
        builder.AppendLine($"Content-Type: multipart/alternative; boundary=\"{boundary}\"");
        builder.AppendLine();

        AppendBodyPart(builder, boundary, "text/plain", message.TextBody);
        AppendBodyPart(builder, boundary, "text/html", message.HtmlBody);
        builder.AppendLine($"--{boundary}--");

        return builder.ToString();
    }

    private static void AppendBodyPart(
        StringBuilder builder,
        string boundary,
        string mediaType,
        string body)
    {
        builder.AppendLine($"--{boundary}");
        builder.AppendLine($"Content-Type: {mediaType}; charset=utf-8");
        builder.AppendLine("Content-Transfer-Encoding: 8bit");
        builder.AppendLine();
        builder.AppendLine(body);
    }

    private static void ValidateHeaderValue(string value, string parameterName)
    {
        if (value.Contains('\r') || value.Contains('\n'))
        {
            throw new ArgumentException(
                "Az e-mail fejlécmező nem tartalmazhat sortörést.",
                parameterName);
        }
    }
}
