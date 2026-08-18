
#nullable enable
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using KvizCommando.Shared.Contracts.CheckIn;

namespace KvizCommando.Server.Services.CheckIn
{
    public interface ICheckInService
    {
        /// <summary>
        /// Visszaadja a felhasználó aktuális beléptetési követelményeit.
        /// </summary>
        /// <param name="userId">Az Identity-felhasználó azonosítója.</param>
        /// <param name="sessionid">A kliens aktuális munkamenet-azonosítója.</param>
        /// <param name="ct">A művelet megszakítását jelző token.</param>
        Task<CheckInGetResponse> GetStatusAsync(string userId, string sessionid, CancellationToken ct);
        /// <summary>
        /// Befejezi a beléptetést; siker esetén üres, hiba esetén lokalizálható hibakódlistát ad vissza.
        /// </summary>
        Task<(IReadOnlyList<string> Errors, string Suggested, bool PreviousSessionReplaced)> CompleteAsync(
            string userId,
            CheckInPostRequest request,
            CancellationToken ct);
    }
}
