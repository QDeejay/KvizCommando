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
    public const int CLASSIFICATION_BOX_COUNT = 5;
    private const string IMAGE_ROOT = "images/buttons/vsgame";

    public static readonly IReadOnlyList<VsBoxSpecs> Specs =
    [
        new()
        {
            Key = VsBoxKeyRoot.CreateBattlefield,
            TitleKey = "home.SubBox.Title.GameVs.CreateBattlefield",
            ImageSrc = string.Empty,
            BgImageSrc = $"{IMAGE_ROOT}/create.webp",
            Size = ContentBoxSize.BUTTON_WIDE,
            FooterDisplay = false,
            ClickId = (int)VsBoxKeyRoot.CreateBattlefield,
            CheckEnable = (data, _) =>
                data.RootBoxInfo.IsCreateBattlefieldEnabled
        },
        new()
        {
            Key = VsBoxKeyRoot.JoinBattlefield,
            TitleKey = "home.SubBox.Title.GameVs.JoinBattlefield",
            ImageSrc = string.Empty,
            BgImageSrc = $"{IMAGE_ROOT}/private.webp",
            Size = ContentBoxSize.BUTTON_WIDE,
            FooterDisplay = false,
            ClickId = (int)VsBoxKeyRoot.JoinBattlefield,
            CheckEnable = (data, _) =>
                data.RootBoxInfo.IsJoinBattlefieldEnabled,
             BuildFooter = (_, data, _) =>
                $"Online: {data.RootBoxInfo.PrivatePlayerCount}"
        },
        new()
        {
            Key = VsBoxKeyRoot.RankedBattlefields,
            TitleKey = "home.SubBox.Title.GameVs.RankedBattlefields",
            ImageSrc = string.Empty,
            BgImageSrc = $"{IMAGE_ROOT}/ranked.webp",
            Size = ContentBoxSize.BUTTON_WIDE,
            FooterDisplay = true,
            ClickId = (int)VsBoxKeyRoot.RankedBattlefields,
            CheckEnable = (data, _) =>
                data.RootBoxInfo.IsRankedBattlefieldsEnabled,
            BuildFooter = (lang, data, _) =>
                lang["vsgame.Box.Footer.Ranked"].FormatSafe(
                    data.RootBoxInfo.RankedPlayerCount,
                    data.RootBoxInfo.RankedHighScore
                        .ToString("0.0"))
        },
        new()
        {
            Key = VsBoxKeyContent.RankedBattleTeamManager,
            TitleKey = "vsgame.Manager.Title",
            ImageSrc = string.Empty,
            BgImageSrc = string.Empty,
            Size = ContentBoxSize.CONTENT_LARGE,
            SizeBuilder = data => ResolveManagerSize(data.RankedBattlefields),
            FooterDisplay = false,
            ReSizable = true,
            ClickId = 0,
            RenderContent = 1,
            LcdBackground = true,
            BodyComp = typeof(RankedBattleTeamManager),
            BuildParams = parameters => new Dictionary<string, object?>
                { [nameof( RankedBattleTeamManager.OnTeamSaved)] =  parameters.OnTeamSaved  }
        },
        new()
        {
            Key = VsBoxKeyContent.RankedMatchManager,
            TitleKey = "vsgame.Match.Title",
            ImageSrc = string.Empty,
            BgImageSrc = string.Empty,
            Size = ContentBoxSize.CONTENT_FLEXIBLE,
            FooterDisplay = false,
            ClickId = 0,
            RenderContent = 1,
            LcdBackground = false,
            BodyComp = typeof(VsMatchManager),
            BuildParams = parameters => new Dictionary<string, object?>
            {
                [nameof(VsMatchManager.ClassificationId)] =
                    parameters.ClassificationId,
                [nameof(VsMatchManager.OnQuitConfirmationChanged)] =
                    parameters.OnQuitConfirmationChanged,
                [nameof(VsMatchManager.OnTeamLevelChanged)] =
                    parameters.OnTeamLevelChanged
            }
        }
    ];
    public static readonly IReadOnlyList<VsBoxSub> SubSpecs =
    [
        new()
        {
            Key = VsBoxKeyRanked.Classification,
            BtnQnty = VsGameBoxSpecs.CLASSIFICATION_BOX_COUNT,
            ImageSrc = string.Empty,
            BgImageSrc = string.Empty,
            BuildImageSrc = (ix) => $"{IMAGE_ROOT}/tier{ix}.webp", 
            Size = ContentBoxSize.BUTTON_WIDE,
            FooterDisplay = true,
            ClickId = (int)VsBoxKeyRanked.Classification,
            BuildTitle = (lang, id) => lang[$"vsgame.Classification.Title.{id}"],
            CheckEnable = (data, id) => data.RankedBattlefields
                    .SavedSelection
                    .EligibleClassificationIds
                    .Contains(id) &&
                data.RootBoxInfo.CreditBalance >=
                    data.RankedBattlefields
                        .Classifications[id - 1].Stake,
            BuildFooter = (_, data, id) =>
                $"Online: {data.RankedBattlefields.Classifications[id - 1].PlayerCount}"
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
