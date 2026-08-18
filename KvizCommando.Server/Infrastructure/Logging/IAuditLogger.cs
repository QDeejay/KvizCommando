using System.Threading.Tasks;

namespace KvizCommando.Server.Infrastructure.Logging;

public interface IAuditLogger
{
    /// <summary>
    /// Auditbejegyzést ír a megadott eseményről.
    /// </summary>
    /// <param name="eventName">Az auditnaplóban rögzített esemény stabil neve.</param>
    /// <param name="userId">Az Identity-felhasználó azonosítója.</param>
    /// <param name="ipAddress">A kérés forrásának IP-címe, ha rendelkezésre áll.</param>
    Task LogAsync(string eventName, string? userId, string? ipAddress);
}
