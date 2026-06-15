// src/Shared/Contracts/CheckIn/CheckInGetResponse.cs
namespace KvizCommando.Shared.Contracts.CheckIn
{
    public sealed class CheckInGetResponse
    {
        public bool Success { get; set; } = false; // user jogosult a check-in-re 
        /// <summary>
        /// Hiányzik-e a felhasználó DisplayName-je (üres/null → true).
        /// </summary>
        public bool NeedsDisplayName { get; set; }

        /// <summary>
        /// El kell-e fogadni a legfrissebb Terms-et (nincs elfogadva vagy régebbi verziót fogadott el → true).
        /// </summary>
        public bool NeedsTermsAcceptance { get; set; }

        /// <summary>
        /// Az éppen aktuális Terms metainformáció (verzió/hash).
        /// </summary>
        public TermsMeta CurrentTerms { get; init; } = default!;

    }
}

