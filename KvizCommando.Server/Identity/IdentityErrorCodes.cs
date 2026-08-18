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
        public const string DisplayNameRequired = "DisplayNameRequired";
        public const string DisplayNameTooShort = "DisplayNameTooShort";
        public const string DisplayNameTooLong = "DisplayNameTooLong";
        public const string DisplayNameInvalidCharacters = "DisplayNameInvalidCharacters";
        public const string DisplayNameAlreadyTaken = "DisplayNameAlreadyTaken";

        // Terms hibák
        public const string TermsNotAccepted = "TermsNotAccepted";
        public const string TermsVersionOutdated = "TermsVersionOutdated";
    }
}
