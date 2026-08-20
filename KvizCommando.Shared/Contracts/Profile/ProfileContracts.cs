namespace KvizCommando.Shared.Contracts.Profile;

public enum ProfileRequestState
{
    Success,
    NotFound,
    SessionMismatch,
    RankLocked,
    CooldownActive,
    SameValue,
    TeamNameRejected,
    InvalidAvatar,
    ServerError
}

public enum TeamNameCheckState
{
    NotChecked,
    Available,
    Required,
    TooShort,
    TooLong,
    InvalidCharacters,
    Taken
}

public sealed class TeamProfileDto
{
    public string TeamName { get; set; } = string.Empty;
    public string CaptainAvatar { get; set; } = "1";
    public int RankEnum { get; set; }
    public int TeamNameRequiredRank { get; set; }
    public int AvatarRequiredRank { get; set; }
    public DateTime? TeamNameChangedUtc { get; set; }
    public DateTime? NextTeamNameChangeUtc { get; set; }
    public int AvatarCount { get; set; }
}

public sealed class ProfileLoadResponse
{
    public ProfileRequestState State { get; set; }
    public TeamProfileDto? Profile { get; set; }
}

public sealed class CheckTeamNameRequest
{
    public string SessionId { get; set; } = string.Empty;
    public string TeamName { get; set; } = string.Empty;
}

public sealed class CheckTeamNameResponse
{
    public ProfileRequestState State { get; set; }
    public TeamNameCheckState TeamNameState { get; set; }
    public string CheckedTeamName { get; set; } = string.Empty;
}

public sealed class SaveTeamNameRequest
{
    public string SessionId { get; set; } = string.Empty;
    public string TeamName { get; set; } = string.Empty;
}

public sealed class SaveAvatarRequest
{
    public string SessionId { get; set; } = string.Empty;
    public string CaptainAvatar { get; set; } = "1";
}

public sealed class SaveProfileResponse
{
    public ProfileRequestState State { get; set; }
    public TeamNameCheckState TeamNameState { get; set; }
    public TeamProfileDto? Profile { get; set; }
}
