using KvizCommando.Shared.Models.Enums;

namespace KvizCommando.Client.Features.Team.ViewModels;

public sealed record TeamMemberAction(
    int MemberNo,
    MembRemark Remark);
