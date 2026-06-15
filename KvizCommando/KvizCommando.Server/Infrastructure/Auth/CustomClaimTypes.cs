namespace KvizCommando.Server.Infrastructure.Auth
{
    /// <summary>
    /// Egyedi claim típusok a rendszerben. 
    /// GDPR-minimum elv szerint csak nem-PII jellegű, technikai állapotot hordozó értékek.
    /// </summary>
    internal static class CustomClaimTypes
    {
        /// <summary>
        /// Az aktuálisan felhasználó által elfogadott ÁSZF (Terms) ETag/Version azonosítója.
        /// Példa érték: "2025-09-01-ETAG".
        /// </summary>
        public const string TermsAcceptedEtag = "terms.accepted.etag";

        // Opcionális:
        // Csak akkor vezessük be, ha tényleges kliensoldali igény van rá.
        // public const string DisplayName = "display_name";
    }
}
