using KvizCommando.Shared.Models.Enums;

namespace KvizCommando.Client.Features.Team.ViewModels;

/// <summary>
/// A kiválasztott csapattagot és a rajta végrehajtandó műveletet azonosítja.
/// </summary>
/// <param name="MemberNo">A csapattag helyének sorszáma.</param>
/// <param name="Remark">A végrehajtandó csapattagművelet típusa.</param>
public sealed record TeamMemberAction(
    int MemberNo,
    MembRemark Remark);
