using System;
using System.Threading;
using System.Threading.Tasks;
using KvizCommando.Server.Identity;

namespace KvizCommando.Server.Services.Auth
{
    /// <summary>
    /// Az elfogadott ÁSZF verzióazonosítóját szinkronizálja az Identity claimjeivel.
    /// </summary>
    public interface IClaimsSyncService
    {
        /// <summary>
        /// Létrehozza vagy frissíti az ÁSZF verzióazonosítóját tartalmazó claimet, majd lehetőség szerint frissíti a hitelesítési cookie-t.
        /// </summary>
        /// <param name="user">Az érintett felhasználó.</param>
        /// <param name="termsEtag">Az elfogadott ÁSZF ETag/Version azonosítója.</param>
        /// <param name="acceptedAtUtc">Az elfogadás UTC időpontja.</param>
        /// <param name="cancellationToken">Művelet megszakításának jele.</param>
        Task UpsertTermsClaimsAsync(ApplicationUser user, string termsEtag, DateTime acceptedAtUtc, CancellationToken cancellationToken = default);

        /// <summary>
        /// Az aktuális UTC-időponttal hozza létre vagy frissíti az ÁSZF-verziót tartalmazó claimet.
        /// </summary>
        /// <param name="user">Az érintett felhasználó.</param>
        /// <param name="termsEtag">Az elfogadott ÁSZF ETag/Version azonosítója.</param>
        /// <param name="cancellationToken">Művelet megszakításának jele.</param>
        Task UpsertTermsClaimsNowAsync(ApplicationUser user, string termsEtag, CancellationToken cancellationToken = default);
    }
}
