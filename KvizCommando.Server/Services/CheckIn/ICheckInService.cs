
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
