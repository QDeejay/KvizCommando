namespace KvizCommando.Client.Features.Team.Builders;

internal enum TBoxKeyRoot
{
    RtBtnTeam,
    RtBtnMembers,
    RtBtnRecruit
}

internal enum TBoxKeyContent
{
    Team,
    Member,
    Recruit
}

public sealed record TeamComponentParameters
{
    public required Func<int, Task> OnMemberSelected { get; init; }
    public required Func<Task> OnHireCompleted { get; init; }
    public int SelectedMember { get; init; }
}
