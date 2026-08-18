using KvizCommando.Shared.Contracts.CheckIn;

namespace KvizCommando.Server.Services.CheckIn
{
    /// <summary>
    /// Az aktuális ÁSZF kultúrafüggő metaadatait biztosító szolgáltatás szerződése.
    /// </summary>
    public interface ITermsProvider
    {
        /// <summary>
        /// Visszaadja az aktuális, kultúrafüggő ÁSZF metaadatait.
        /// </summary>
        TermsMeta GetCurrentTerms();

        /// <summary>
        /// Jelzi, hogy a megadott verzió megfelel-e az aktuális ÁSZF-verziónak.
        /// </summary>
        /// <param name="version">Az ellenőrzendő feltételverzió.</param>
        /// <returns><see langword="true"/>, ha a verzió jelenleg elfogadható.</returns>
        bool IsValidVersion(string version);

        /// <summary>
        /// Az aktuális ÁSZF kultúrafüggő verzióazonosítója.
        /// </summary>
        string CurrentTermsEtag { get; }
    }
}
