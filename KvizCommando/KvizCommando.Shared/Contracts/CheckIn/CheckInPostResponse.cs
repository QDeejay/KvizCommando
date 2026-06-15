#nullable enable
using System.Collections.Generic;

namespace KvizCommando.Shared.Contracts.CheckIn
{
    /// <summary>
    /// POST /api/check-in válasz.
    /// Success: csak akkor true, ha minden feltétel teljesült (név rendben + aktuális Terms elfogadva).
    /// Errors: IdentityError Options-ban definiált kódok listája.
    /// CurrentTerms: mindig visszaküldjük (ha közben frissült, ebből tud a kliens újrarenderelni).
    /// </summary>
    public sealed class CheckInPostResponse
    {
        public bool Success { get; init; }

        public string SuggestedDisplayName { get; init; } = string.Empty;

        public List<string> Errors { get; init; } = new();

        public TermsMeta CurrentTerms { get; init; } = default!;
        /// <summary>
        /// Asztali/mobil (opaque bearer) ügyfeleknek jelzés: hívj /auth/refresh-et,
        /// hogy a friss claim (terms.accepted.*) bekerüljön az új access tokenbe.
        /// Cookie/WASM esetén ez hamis, mert a szerver RefreshSignInAsync-et hív.
        /// </summary>
        public bool RequiresTokenRefresh { get; set; }
    }
}
