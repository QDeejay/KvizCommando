#nullable enable
using KvizCommando.Shared.Models.Rules;

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
            return PublicNameRules.Validate(displayName) switch
            {
                PublicNameValidationResult.Valid => [],
                PublicNameValidationResult.Required =>
                    [IdentityErrorCodes.DISPLAY_NAME_REQUIRED],
                PublicNameValidationResult.TooShort =>
                    [IdentityErrorCodes.DISPLAY_NAME_TOO_SHORT],
                PublicNameValidationResult.TooLong =>
                    [IdentityErrorCodes.DISPLAY_NAME_TOO_LONG],
                PublicNameValidationResult.InvalidCharacters =>
                    [IdentityErrorCodes.DISPLAY_NAME_INVALID_CHARACTERS],
                _ => [IdentityErrorCodes.DISPLAY_NAME_INVALID_CHARACTERS]
            };
        }
    }
}
