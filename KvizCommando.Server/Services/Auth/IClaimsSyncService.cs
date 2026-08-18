using System;
using System.Threading;
using System.Threading.Tasks;
using KvizCommando.Server.Identity;

namespace KvizCommando.Server.Services.Auth
{
    /// <summary>
    /// „Gyári” claim-szinkron szolgáltatás: a felhasználó AspNetUserClaims rekordjaiban
    /// tartja karban az ÁSZF elfogadáshoz kapcsolódó claim-eket (ETag + AcceptedAt).
    /// Nem tárol PII-t; IP/UA kizárólag az audit táblában marad.
    /// </summary>
    public interface IClaimsSyncService
    {
        /// <summary>
        /// Upserteli a két ÁSZF-claimet (ETag + AcceptedAt) a megadott időbélyeggel,
        /// majd – ha van HTTP-kontekstus és cookie-alapú bejelentkezés – frissíti a bejelentkezést.
        /// </summary>
        /// <param name="user">Az érintett felhasználó.</param>
        /// <param name="termsEtag">Az elfogadott ÁSZF ETag/Version azonosítója.</param>
        /// <param name="acceptedAtUtc">Az elfogadás UTC időpontja.</param>
        /// <param name="cancellationToken">Művelet megszakításának jele.</param>
        Task UpsertTermsClaimsAsync(ApplicationUser user, string termsEtag, DateTime acceptedAtUtc, CancellationToken cancellationToken = default);

        /// <summary>
        /// Upserteli a két ÁSZF-claimet (ETag + AcceptedAt) a jelenlegi UTC idővel,
        /// majd – ha van HTTP-kontekstus és cookie-alapú bejelentkezés – frissíti a bejelentkezést.
        /// </summary>
        /// <param name="user">Az érintett felhasználó.</param>
        /// <param name="termsEtag">Az elfogadott ÁSZF ETag/Version azonosítója.</param>
        /// <param name="cancellationToken">Művelet megszakításának jele.</param>
        Task UpsertTermsClaimsNowAsync(ApplicationUser user, string termsEtag, CancellationToken cancellationToken = default);
    }
}
