namespace KvizCommando.Shared.Contracts.Profile;

public enum ProfileAccountRequestState
{
    Success,
    NotFound,
    ValidationFailed,
    IdentityError,
    ServerError
}

public sealed class ProfileAccountDto
{
    public string Email { get; set; } = string.Empty;
    public string PreferredLocale { get; set; } = "hu-HU";
    public ProfilePhoneDto Phone { get; set; } = new();
    public BillingNameDto BillingName { get; set; } = new();
    public BillingAddressDto BillingAddress { get; set; } = new();
}

public sealed class SaveProfileAccountRequest
{
    public ProfilePhoneDto Phone { get; set; } = new();
    public BillingNameDto BillingName { get; set; } = new();
    public BillingAddressDto BillingAddress { get; set; } = new();
}

public sealed class ProfilePhoneDto
{
    public string CountryCode { get; set; } = "+36";
    public string Number { get; set; } = string.Empty;
}

public sealed class BillingNameDto
{
    public string LastName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
}

public sealed class BillingAddressDto
{
    public string PostalCode { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string AddressLine1 { get; set; } = string.Empty;
    public string AddressLine2 { get; set; } = string.Empty;
}

public sealed class ProfileAccountResponse
{
    public ProfileAccountRequestState State { get; set; }
    public ProfileAccountDto? Account { get; set; }
    public List<string> Errors { get; set; } = [];
}

public sealed class ProfileIdentityUpdateResponse
{
    public bool Success { get; set; }
    public List<string> Errors { get; set; } = [];
}
