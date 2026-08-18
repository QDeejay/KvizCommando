namespace KvizCommando.Server.Infrastructure.Email;

public interface IEmailDelivery
{
    /// <summary>
    /// Átadja a már elkészített levelet az aktív kézbesítési adapternek.
    /// </summary>
    /// <param name="message">A kézbesítendő levél teljes tartalma.</param>
    /// <param name="cancellationToken">A művelet megszakítását jelző token.</param>
    Task DeliverAsync(
        EmailMessage message,
        CancellationToken cancellationToken = default);
}
