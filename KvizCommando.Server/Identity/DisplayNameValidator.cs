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
        /// <param name="displayName">A menteni kívánt nyilvános játékosnév.</param>
        public static IReadOnlyList<string> Validate(string? displayName)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(displayName))
            {
                errors.Add(IdentityErrorCodes.DISPLAY_NAME_REQUIRED);
                return errors;
            }

            var name = displayName.Trim();

            if (name.Length < CheckInValidationOptions.DISPLAY_NAME_MIN_LENGTH)
                errors.Add(IdentityErrorCodes.DISPLAY_NAME_TOO_SHORT);

            if (name.Length > CheckInValidationOptions.DISPLAY_NAME_MAX_LENGTH)
                errors.Add(IdentityErrorCodes.DISPLAY_NAME_TOO_LONG);

            // Karakterkészlet ellenőrzése (csak az engedélyezett készletből származhat minden karakter).
            // Az engedélyezett készlet jelenleg ASCII; Unicode támogatásakor a validációs szabályt is bővíteni kell.
            if (!string.IsNullOrEmpty(CheckInValidationOptions.DISPLAY_NAME_ALLOWED_CHARS))
            {
                var allowed = CheckInValidationOptions.DISPLAY_NAME_ALLOWED_CHARS.ToHashSet();
                if (name.Any(ch => !allowed.Contains(ch)))
                    errors.Add(IdentityErrorCodes.DISPLAY_NAME_INVALID_CHARACTERS);
            }

            return errors;
        }
    }
}
