using System.Threading.Tasks;

namespace KvizCommando.Server.Infrastructure.Logging;

public interface IAuditLogger
{
    /// <summary>
    /// Rögzíti a megadott biztonsági eseményt az aktív audittárolóban.
    /// </summary>
    /// <param name="entry">A strukturált auditbejegyzés.</param>
    /// <param name="cancellationToken">A művelet megszakítását jelző token.</param>
    Task LogAsync(
        AuditEntry entry,
        CancellationToken cancellationToken = default);
}
