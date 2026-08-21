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
    public string Phone { get; set; } = string.Empty;
    public string BillingName { get; set; } = string.Empty;
    public string BillingAddress { get; set; } = string.Empty;
}

public sealed class SaveProfileAccountRequest
{
    public string Phone { get; set; } = string.Empty;
    public string BillingName { get; set; } = string.Empty;
    public string BillingAddress { get; set; } = string.Empty;
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
