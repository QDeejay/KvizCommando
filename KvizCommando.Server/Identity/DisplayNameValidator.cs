#nullable enable
using System.Collections.Generic;
using System.Linq;

namespace KvizCommando.Server.Identity
{
    /// <summary>
    /// DisplayName validátor, kizárólag a CheckInValidationOptions állandóira támaszkodva.
    /// Egyetlen igazságforrás: nincsenek duplikált szabályok máshol.
    /// </summary>
    public static class DisplayNameValidator
    {
        /// <summary>
        /// Ellenőrzi a megjelenített név (DisplayName) érvényességét és visszaadja a hibakódok listáját.
        /// Üres lista esetén a név érvényes.
        /// </summary>
        public static IReadOnlyList<string> Validate(string? displayName)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(displayName))
            {
                errors.Add(IdentityErrorCodes.DisplayNameRequired);
                return errors;
            }

            var name = displayName.Trim();

            if (name.Length < CheckInValidationOptions.DisplayNameMinLength)
                errors.Add(IdentityErrorCodes.DisplayNameTooShort);

            if (name.Length > CheckInValidationOptions.DisplayNameMaxLength)
                errors.Add(IdentityErrorCodes.DisplayNameTooLong);

            // Karakterkészlet ellenőrzése (csak az engedélyezett készletből származhat minden karakter).
            // Megjegyzés: az AllowedChars ASCII készlet; ha később bővülne Unicode-ra, itt kell módosítani.
            if (!string.IsNullOrEmpty(CheckInValidationOptions.DisplayNameAllowedChars))
            {
                // HashSet a gyors kereséshez
                var allowed = CheckInValidationOptions.DisplayNameAllowedChars.ToHashSet();
                if (name.Any(ch => !allowed.Contains(ch)))
                    errors.Add(IdentityErrorCodes.DisplayNameInvalidCharacters);
            }

            return errors;
        }
    }
}
