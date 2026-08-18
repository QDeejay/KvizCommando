using KvizCommando.Client.Pages.Home.Features;

namespace KvizCommando.Client.Features.Team.Builders;

internal enum TBoxKeyRoot
{
    TeamOverview = (int)HomeBoxKey.Team * 100 + 1,
    Members = (int)HomeBoxKey.Team * 100 + 2,
    Recruit = (int)HomeBoxKey.Team * 100 + 3
}

internal enum TBoxKeyContent
{
    Team,
    Member,
    RecruitContent
}

public sealed record TeamComponentParameters
{
    public required Func<int, Task> OnMemberSelected { get; init; }
    public required Func<Task> OnHireCompleted { get; init; }
    public int SelectedMember { get; init; }
}
