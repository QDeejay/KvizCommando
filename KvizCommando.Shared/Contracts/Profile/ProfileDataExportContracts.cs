namespace KvizCommando.Shared.Contracts.Profile;

public sealed class ProfileDataExportRequest
{
    public string CurrentPassword { get; set; } = string.Empty;
}

public enum ProfileDataExportState
{
    Success,
    InvalidPassword,
    RateLimited,
    ServerError
}

public sealed class ProfileDataExportResult
{
    public ProfileDataExportState State { get; set; }
    public string FileName { get; set; } = string.Empty;
    public byte[] Content { get; set; } = [];
}
