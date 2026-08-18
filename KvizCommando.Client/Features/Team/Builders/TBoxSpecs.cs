using KvizCommando.Client.Features.Team.Components;
using KvizCommando.Client.Helpers;
using KvizCommando.Client.Models.ViewModels;
using KvizCommando.Client.Services.Visual.UiService.Language;
using KvizCommando.Shared.Models.Dtos;

namespace KvizCommando.Client.Features.Team.Builders;

public sealed class TBoxSpecs : VmSpecs
{
    internal Enum Key { get; init; } = default!;
    internal Func<TeamRootBoxInfo, string> SizeBuilder
    { get; init; } = _ => string.Empty;
    internal Func<TeamRootBoxInfo, bool> CheckEnable
    { get; init; } = _ => true;
    internal Func<ILanguageService, TeamRootBoxInfo, string> BuildBoxText
    { get; init; } = (_, _) => string.Empty;
    internal Func<TeamComponentParameters, Dictionary<string, object?>> BuildParams
    { get; init; } = _ => [];
}

public static class TeamBoxSpecs
{
    private const string IMAGE_ROOT = "images/buttons/team";

    public static readonly IReadOnlyList<TBoxSpecs> Specs =
    [
        new TBoxSpecs
        {
            Key = TBoxKeyRoot.TeamOverview,
            TitleKey = "home.SubBox.Title.Team.TeamOverview",
            ImageSrc = $"{IMAGE_ROOT}/team.webp",
            Size = ContentBoxSize.BUTTON_WIDE,
            FooterDisplay = true,
            ClickId = (int)TBoxKeyRoot.TeamOverview,
            BuildBoxText = (lang, root) => lang["team.Box.Footer.Team"].FormatSafe( root.TeamOpRequired),
            CheckEnable = root => root.IsTeamEnable
        },
        new TBoxSpecs
        {
            Key = TBoxKeyRoot.Members,
            TitleKey = "home.SubBox.Title.Team.Members",
            ImageSrc = $"{IMAGE_ROOT}/members.webp",
            Size = ContentBoxSize.BUTTON_WIDE,
            FooterDisplay = true,
            ClickId = (int)TBoxKeyRoot.Members,
            BuildBoxText = (lang, root) => lang["team.Box.Footer.Member"].FormatSafe(root.MemberOpRequired),
            CheckEnable = root => root.IsMemberEnable
        },
        new TBoxSpecs
        {
            Key = TBoxKeyRoot.Recruit,
            TitleKey = "home.SubBox.Title.Team.Recruit",
            ImageSrc = $"{IMAGE_ROOT}/recruit.webp",
            Size = ContentBoxSize.BUTTON_WIDE,
            FooterDisplay = true,
            ClickId = (int)TBoxKeyRoot.Recruit,
            BuildBoxText = (lang, root) => lang["team.Box.Footer.Recruit"].FormatSafe( root.FreePositions),
            CheckEnable = root => root.IsRecruitEnable
        },
        new TBoxSpecs
        {
            Key = TBoxKeyContent.Team,
            TitleKey = "home.SubBox.Title.Team.TeamOverview",
            ImageSrc = string.Empty,
            Size = ContentBoxSize.CONTENT_LARGE,
            FooterDisplay = false,
            ClickId = 0,
            CheckEnable = _ => true,
            LcdBackground = true,
            RenderContent = 1,
            BodyComp = typeof(TeamManager),
            BuildParams = parameters => new Dictionary<string, object?>
                { [nameof(TeamManager.OnMemberSelected)] =  parameters.OnMemberSelected }
        },
        new TBoxSpecs
        {
            Key = TBoxKeyContent.Member,
            TitleKey = "home.SubBox.Title.Team.Members",
            ImageSrc = string.Empty,
            Size = ContentBoxSize.CONTENT_LARGE,
            FooterDisplay = false,
            ClickId = 0,
            CheckEnable = _ => true,
            LcdBackground = true,
            RenderContent = 1,
            BodyComp = typeof(MemberManager),
            BuildParams = parameters => new Dictionary<string, object?>
                { [nameof(MemberManager.InitialPosition)] = parameters.SelectedMember }
        },
        new TBoxSpecs
        {
            Key = TBoxKeyContent.RecruitContent,
            TitleKey = "home.SubBox.Title.Team.Recruit",
            ImageSrc = string.Empty,
            Size = string.Empty,
            FooterDisplay = false,
            ClickId = 0,
            SizeBuilder = root => root.AbleToHire == 0 ? ContentBoxSize.CONTENT_CLOSED_LARGE : ContentBoxSize.CONTENT_FLEXIBLE,
            CheckEnable = _ => true,
            LcdBackground = false,
            RenderContent = 1,
            BodyComp = typeof(RecruitManager),
            BuildParams = parameters => new Dictionary<string, object?>
                { [nameof(RecruitManager.OnHireCompleted)] =  parameters.OnHireCompleted }
        }
    ];
}
