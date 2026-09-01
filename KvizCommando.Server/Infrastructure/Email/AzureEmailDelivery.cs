using Azure;
using Azure.Communication.Email;
using Azure.Identity;
using Microsoft.Extensions.Options;

namespace KvizCommando.Server.Infrastructure.Email;

/// <summary>
/// Azure Communication Services Email használatával kézbesíti a production rendszerleveleket.
/// </summary>
public sealed class AzureEmailDelivery : IEmailDelivery
{
    private readonly EmailClient _emailClient;
    private readonly ILogger<AzureEmailDelivery> _logger;

    public AzureEmailDelivery(
        IOptions<EmailOptions> options,
        ILogger<AzureEmailDelivery> logger)
    {
        var azureOptions = options.Value.Azure;
        var credential = new ClientSecretCredential(
            azureOptions.TenantId,
            azureOptions.ClientId,
            azureOptions.ClientSecret);

        _emailClient = new EmailClient(
            new Uri(azureOptions.Endpoint),
            credential);
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task DeliverAsync(
        EmailMessage message,
        CancellationToken cancellationToken = default)
    {
        var operation = await _emailClient.SendAsync(
            WaitUntil.Completed,
            senderAddress: message.From,
            recipientAddress: message.To,
            subject: message.Subject,
            htmlContent: message.HtmlBody,
            plainTextContent: message.TextBody,
            cancellationToken: cancellationToken);

        _logger.LogInformation(
            "Az Azure Communication Services átvette a(z) {EmailType} típusú levelet. OperationId: {OperationId}, Status: {Status}",
            message.Type,
            operation.Id,
            operation.Value.Status);
    }
}
