using KvizCommando.Client.Data;
using KvizCommando.Client.Features.Solo.Builders;
using KvizCommando.Client.Features.VsGame.Components;
using KvizCommando.Client.Models.ViewModels;
using KvizCommando.Localization.VsGame;
using KvizCommando.Shared.Models.Dtos;
using Microsoft.Extensions.Localization;

namespace KvizCommando.Client.Features.VsGame.Builders;

public class VsBoxSpecs : VmSpecs
{
    internal Enum Key { get; init; } = default!;
    internal Func<VsGameDtos, int, bool> CheckEnable { get; init; } =
        (_, _) => true;
    internal Func<IStringLocalizer<VsGameResource>, VsGameDtos, int, string> BuildFooter
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
    internal Func<IReadOnlyDictionary<string, string>, int, string>
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
            TitleKey = "VsGame.CreateBattlefield",
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
            TitleKey = "VsGame.JoinBattlefield",
            ImageSrc = string.Empty,
            BgImageSrc = $"{IMAGE_ROOT}/private.webp",
            Size = ContentBoxSize.BUTTON_WIDE,
            FooterDisplay = false,
            ClickId = (int)VsBoxKeyRoot.JoinBattlefield,
            CheckEnable = (data, _) =>
                data.RootBoxInfo.IsJoinBattlefieldEnabled,
            BuildFooter = (lang, data, _) =>
                lang[
                    "vsgame.Box.Footer.Online",
                    data.RootBoxInfo.PrivatePlayerCount]
        },
        new()
        {
            Key = VsBoxKeyRoot.RankedBattlefields,
            TitleKey = "VsGame.RankedBattlefields",
            ImageSrc = string.Empty,
            BgImageSrc = $"{IMAGE_ROOT}/ranked.webp",
            Size = ContentBoxSize.BUTTON_WIDE,
            FooterDisplay = true,
            ClickId = (int)VsBoxKeyRoot.RankedBattlefields,
            CheckEnable = (data, _) =>
                data.RootBoxInfo.IsRankedBattlefieldsEnabled,
            BuildFooter = (lang, data, _) =>
                lang[
                    "vsgame.Box.Footer.Ranked",
                    data.RootBoxInfo.RankedPlayerCount,
                    data.RootBoxInfo.RankedHighScore
                        .ToString("0.0")]
        },
        new()
        {
            Key = VsBoxKeyContent.RankedBattleTeamManager,
            TitleKey = "VsGame.RankedBattleTeamManager",
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
            TitleKey = "VsGame.RankedMatchManager",
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
            BuildTitle = (boxTitles, id) =>
                boxTitles[$"VsGame.Classification.{id}"],
            CheckEnable = (data, id) => data.RankedBattlefields
                    .SavedSelection
                    .EligibleClassificationIds
                    .Contains(id) &&
                data.RootBoxInfo.CreditBalance >=
                    data.RankedBattlefields
                        .Classifications[id - 1].Stake,
            BuildFooter = (lang, data, id) =>
                lang[
                    "vsgame.Box.Footer.Online",
                    data.RankedBattlefields
                        .Classifications[id - 1].PlayerCount]
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
