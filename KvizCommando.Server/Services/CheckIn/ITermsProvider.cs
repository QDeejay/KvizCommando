using KvizCommando.Shared.Contracts.CheckIn;

namespace KvizCommando.Server.Services.CheckIn
{
    /// <summary>
    /// Központi Terms (ÁSZF) információ-szolgáltató.
    /// Fő feladata a jelenlegi (aktuális) Terms metaadatainak biztosítása.
    /// </summary>
    public interface ITermsProvider
    {
        /// <summary>
        /// Az aktuális Terms metaadatai (kultúrafüggő).
        /// </summary>
        TermsMeta GetCurrentTerms();

        /// <summary>
        /// Gyors ellenőrzéshez: érvényes-e a megadott verzió az aktuális Terms-hez képest?
        /// </summary>
        bool IsValidVersion(string version);

        /// <summary>
        /// Az aktuális Terms ETag/Version azonosítója (kultúrafüggő).
        /// Funkcionálisan <see cref="GetCurrentTerms"/>.Version rövidítése.
        /// </summary>
        string CurrentTermsEtag { get; }
    }
}
