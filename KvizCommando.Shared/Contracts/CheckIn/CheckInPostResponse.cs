#nullable enable
using System.Collections.Generic;

namespace KvizCommando.Shared.Contracts.CheckIn
{
    /// <summary>
    /// A beléptetési adatok mentésének eredménye és az aktuális ÁSZF metaadatai.
    /// </summary>
    public sealed class CheckInPostResponse
    {
        public bool Success { get; init; }

        public string SuggestedDisplayName { get; init; } = string.Empty;

        public List<string> Errors { get; init; } = new();

        public TermsMeta CurrentTerms { get; init; } = default!;
        /// <summary>
        /// Jelzi, hogy a bearer-alapú kliensnek frissítenie kell a hozzáférési tokent
        /// az új ÁSZF-claim átvételéhez. Cookie-alapú belépésnél mindig hamis.
        /// </summary>
        public bool RequiresTokenRefresh { get; set; }

        public bool PreviousSessionReplaced { get; init; }
    }
}
