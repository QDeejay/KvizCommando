namespace KvizCommando.Shared.Contracts.Profile;

public sealed class ProfileAccountDeletionRequest
{
    public string CurrentPassword { get; set; } = string.Empty;
}

public enum ProfileAccountDeletionState
{
    Success,
    InvalidPassword,
    RateLimited,
    NotFound,
    ServerError
}
