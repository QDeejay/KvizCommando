using KvizCommando.Client.Data;
using KvizCommando.Client.Features.Solo.Builders;
using KvizCommando.Client.Features.VsGame.Components;
using KvizCommando.Client.Helpers;
using KvizCommando.Client.Models.ViewModels;
using KvizCommando.Client.Services.Visual.UiService.Language;
using KvizCommando.Shared.Models.Dtos;

namespace KvizCommando.Client.Features.VsGame.Builders;

public class VsBoxSpecs : VmSpecs
{
    internal Enum Key { get; init; } = default!;
    internal bool ReSizable { get; init; }
    internal Func<VsGameDtos, int, bool> CheckEnable { get; init; } =
        (_, _) => true;
    internal Func<ILanguageService, VsGameDtos, int, string> BuildFooter
    { get; init; } = (_, _, _) => string.Empty;
    internal Func<VsGameDtos, string> SizeBuilder { get; init; } =
        _ => string.Empty;
    internal Func<VsComponentParameters, Dictionary<string, object?>>
        BuildParams
    { get; init; } = _ => [];
}
public sealed class VsBoxSub : VsBoxSpecs
{
    internal int BtnQnty { get; init; }
    internal Func<ILanguageService, int, string>
        BuildTitle
    { get; init; } = default!;
    internal Func<int, string> BuildImageSrc { get; init; } = default!;
}


public static class VsGameBoxSpecs
{
    private const string IMAGE_ROOT = "images/buttons/vsgame";

    public static readonly IReadOnlyList<VsBoxSpecs> Specs =
    [
        new()
        {
            Key = VsBoxKeyRoot.RtBtnCreateBattlefield,
            TitleKey = "vsgame.Box.Title.CreateBattlefield",
            ImageSrc = string.Empty,
            BgImageSrc = $"{IMAGE_ROOT}/create.webp",
            Size = ContentBoxSize.BUTTON_WIDE,
            FooterDisplay = false,
            ClickId = 301,
            CheckEnable = (data, _) =>
                data.RootBoxInfo.IsCreateBattlefieldEnabled
        },
        new()
        {
            Key = VsBoxKeyRoot.RtBtnJoinBattlefield,
            TitleKey = "vsgame.Box.Title.JoinBattlefield",
            ImageSrc = string.Empty,
            BgImageSrc = $"{IMAGE_ROOT}/private.webp",
            Size = ContentBoxSize.BUTTON_WIDE,
            FooterDisplay = false,
            ClickId = 302,
            CheckEnable = (data, _) =>
                data.RootBoxInfo.IsJoinBattlefieldEnabled
        },
        new()
        {
            Key = VsBoxKeyRoot.RtBtnRankedBattlefields,
            TitleKey = "vsgame.Box.Title.RankedBattlefields",
            ImageSrc = string.Empty,
            BgImageSrc = $"{IMAGE_ROOT}/ranked.webp",
            Size = ContentBoxSize.BUTTON_WIDE,
            FooterDisplay = true,
            ClickId = 303,
            CheckEnable = (data, _) =>
                data.RootBoxInfo.IsRankedBattlefieldsEnabled,
            BuildFooter = (lang, data, _) =>
                lang["vsgame.Box.Footer.RankedRequirements"]
                    .FormatSafe(
                        data.RootBoxInfo
                            .RequiredBattleReadyCharacterCount,
                        data.RootBoxInfo.RequiredCreditBalance)
        },
        new()
        {
            Key = VsBoxKeyContent.RankedBattleTeamManager,
            TitleKey = "vsgame.Manager.Title",
            ImageSrc = string.Empty,
            BgImageSrc = string.Empty,
            Size = string.Empty,
            SizeBuilder = data => ResolveManagerSize(data.RankedBattlefields),
            FooterDisplay = false,
            ReSizable = true,
            ClickId = 0,
            RenderContent = 1,
            LcdBackground = true,
            BodyComp = typeof(RankedBattleTeamManager),
            BuildParams = parameters => new Dictionary<string, object?>
                { [nameof( RankedBattleTeamManager.OnTeamSaved)] =  parameters.OnTeamSaved  }
        }
    ];
    public static readonly IReadOnlyList<VsBoxSub> SubSpecs =
    [
        new()
        {
            Key = VsBoxKeyRanked.Classification,
            BtnQnty = (int)VsBoxKeyRanked.Classification,
            ImageSrc = string.Empty,
            BgImageSrc = string.Empty,
            BuildImageSrc = (ix) => $"{IMAGE_ROOT}/tier{ix}.webp", 
            Size = ContentBoxSize.BUTTON_WIDE,
            FooterDisplay = true,
            ClickId = 310,
            BuildTitle = (lang, id) => lang[$"vsgame.Classification.Title.{id}"],
            CheckEnable = (data, id) => data.RankedBattlefields
                    .SavedSelection
                    .EligibleClassificationIds
                    .Contains(id),
            BuildFooter = (lang, data, id) => lang["vsgame.Classification.Footer.Requirements"].FormatSafe(
                RankNameTable.Data[data.RankedBattlefields.Classifications[id - 1].MinimumTeamRank].PublicLevel ?? string.Empty,
                data.RankedBattlefields.Classifications[id - 1].RequiredPartySize,
                data.RankedBattlefields.Classifications[id - 1].RequiredMembersInRankClassRange,
                data.RankedBattlefields.Classifications[id - 1].MemberMinimumRankClass,
                data.RankedBattlefields.Classifications[id - 1].MemberMaximumRankClass)
        }
    ];



    private static string ResolveManagerSize(
        VsRankedBattlefieldsDto ranked)
    {
        var slots =
            ranked.SavedSelection.SelectedSlotNumbers;
        var selectableSlots = ranked.TeamMembers
            .Where(member => member.IsSelectable)
            .Select(member => member.SlotNumber)
            .ToHashSet();

        return slots.Length > 0 &&
               slots.All(slot =>
                   slot > 0 &&
                   selectableSlots.Contains(slot))
            ? ContentBoxSize.MINIMALIZED
            : ContentBoxSize.CONTENT_LARGE;
    }
}
