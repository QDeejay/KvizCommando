
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace KvizCommando.Server.Identity
{
    /// <summary>
    /// Bővített Identity felhasználó a játékhoz.
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        /// <summary>Játékban megjelenő név.</summary>
        public string? DisplayName { get; set; }

        /// <summary>Játékban megjelenő név normalizált (pl. FELSŐ) változata, egyediséghez.</summary>
        public string? NormalizedDisplayName { get; set; }

        /// <summary>Felhasználó preferált nyelve IETF formátumban (pl. "hu-HU").</summary>
        public string PreferredLocale { get; set; } = "hu-HU";

        /// <summary>Regisztráció időpontja (UTC).</summary>
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        /// <summary>ÁSZF elfogadása (aktuális állapot-flag, audit külön táblában).</summary>
        public bool AcceptTerms { get; set; }

        /// <summary>Marketing célú megkereséshez adott hozzájárulás (aktuális állapot-flag, audit külön táblában).</summary>
        public bool MarketingConsent { get; set; }

        /// <summary>Soft delete jelző (logikai törléshez).</summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// Számított, perzisztált oszlop: akkor igaz, ha a check-in feltételek teljesülnek
        /// (DisplayName nem null és AcceptTerms = 1). EF konfiguráció állítja be a számított SQL-t.
        /// </summary>
        //[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        //public bool IsCheckInCompleted { get; private set; }
    }
}


public class ApplicationUserToken : IdentityUserToken<string>
{
    public DateTimeOffset ExpiresAt { get; set; }
}



