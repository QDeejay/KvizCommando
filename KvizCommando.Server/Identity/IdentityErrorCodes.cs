#nullable enable
namespace KvizCommando.Server.Identity
{
    /// <summary>
    /// Egységes hiba-kulcsok a Check-In folyamathoz (és kapcsolódó UI-validációkhoz).
    /// Ezeket a kódokat adja vissza a szerver a CheckInPostResponse.Errors listában.
    /// </summary>
    public static class IdentityErrorCodes
    {
        // DisplayName hibák
        public const string DISPLAY_NAME_REQUIRED = "DisplayNameRequired";
        public const string DISPLAY_NAME_TOO_SHORT = "DisplayNameTooShort";
        public const string DISPLAY_NAME_TOO_LONG = "DisplayNameTooLong";
        public const string DISPLAY_NAME_INVALID_CHARACTERS = "DisplayNameInvalidCharacters";
        public const string DISPLAY_NAME_ALREADY_TAKEN = "DisplayNameAlreadyTaken";

        // Terms hibák
        public const string TERMS_NOT_ACCEPTED = "TermsNotAccepted";
        public const string TERMS_VERSION_OUTDATED = "TermsVersionOutdated";
    }
}
