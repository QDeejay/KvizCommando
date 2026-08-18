
#nullable enable
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using KvizCommando.Shared.Contracts.CheckIn;

namespace KvizCommando.Server.Services.CheckIn
{
    public interface ICheckInService
    {
        Task<CheckInGetResponse> GetStatusAsync(string userId, string sessionid, CancellationToken ct);
        /// <summary>
        /// Visszatér: üres lista = siker; különben IdentityErrorCodes kulcsok listája.
        /// </summary>
        Task<(IReadOnlyList<string> Errors, string Suggested, bool PreviousSessionReplaced)> CompleteAsync(
            string userId,
            CheckInPostRequest request,
            CancellationToken ct);
    }
}
