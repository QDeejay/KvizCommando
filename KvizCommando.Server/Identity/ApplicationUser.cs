
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace KvizCommando.Server.Identity
{
    /// <summary>
    /// Az Identity-fiókot a játékosprofilhoz és a hozzájárulások aktuális állapotához kapcsolja.
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        /// <summary>Játékban megjelenő név.</summary>
        public string? DisplayName { get; set; }

        /// <summary>A játékosnév normalizált változata, amelyen az egyediségvizsgálat történik.</summary>
        public string? NormalizedDisplayName { get; set; }

        /// <summary>A felhasználó előnyben részesített nyelve IETF-formátumban, például <c>hu-HU</c>.</summary>
        public string PreferredLocale { get; set; } = "hu-HU";

        /// <summary>A regisztráció UTC időpontja.</summary>
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        /// <summary>Az aktuális ÁSZF elfogadási állapota; az elfogadások történetét külön auditrekordok őrzik.</summary>
        public bool AcceptTerms { get; set; }

        /// <summary>A marketingcélú megkereséshez adott hozzájárulás aktuális állapota.</summary>
        public bool MarketingConsent { get; set; }

        /// <summary>Jelzi, hogy a fiók logikailag törölt állapotban van-e.</summary>
        public bool IsDeleted { get; set; }

    }
}

/// <summary>
/// Az Identity által kiadott felhasználói token lejárati időponttal kiegészített perzisztált alakja.
/// </summary>
public class ApplicationUserToken : IdentityUserToken<string>
{
    public DateTimeOffset ExpiresAt { get; set; }
}
