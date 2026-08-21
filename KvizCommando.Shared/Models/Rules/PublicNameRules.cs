namespace KvizCommando.Shared.Models.Rules;

public enum PublicNameValidationResult
{
    Valid,
    Required,
    TooShort,
    TooLong,
    InvalidCharacters
}

/// <summary>
/// A nyilvánosan megjelenő játékos- és csapatnevek szabályai.
/// </summary>
public static class PublicNameRules
{
    public const int NAME_MIN_LENGTH = 3;
    public const int NAME_MAX_LENGTH = 20;
    public const string NAME_ALLOWED_CHARACTERS =
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_";
    public const string TEAM_NAME_ALLOWED_CHARACTERS =
        NAME_ALLOWED_CHARACTERS + " '";

    public static PublicNameValidationResult Validate(string? value) =>
        Validate(value, NAME_ALLOWED_CHARACTERS);

    public static PublicNameValidationResult ValidateTeamName(string? value) =>
        Validate(value, TEAM_NAME_ALLOWED_CHARACTERS);

    private static PublicNameValidationResult Validate(
        string? value,
        string allowedCharacters)
    {
        if (string.IsNullOrWhiteSpace(value))
            return PublicNameValidationResult.Required;

        var name = value.Trim();

        if (name.Length < NAME_MIN_LENGTH)
            return PublicNameValidationResult.TooShort;

        if (name.Length > NAME_MAX_LENGTH)
            return PublicNameValidationResult.TooLong;

        return name.Any(character =>
            !allowedCharacters.Contains(character))
                ? PublicNameValidationResult.InvalidCharacters
                : PublicNameValidationResult.Valid;
    }
}
