using KvizCommando.Client.Features.VsGame.Components;
using KvizCommando.Client.Helpers;
using KvizCommando.Client.Models.ViewModels;
using KvizCommando.Client.Services.Visual.UiService.Language;
using KvizCommando.Shared.Models.Dtos;

namespace KvizCommando.Client.Features.VsGame.Builders;

public sealed class VsBoxSpecs : VmSpecs
{
    internal Enum Key { get; init; } = default!;
    internal Func<VsRootBoxInfo, bool> CheckEnable { get; init; } =
        _ => true;
    internal Func<ILanguageService, VsRootBoxInfo, string> BuildFooter
        { get; init; } = (_, _) => string.Empty;
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
            ClickId = 601,
            CheckEnable = root =>
                root.IsCreateBattlefieldEnabled
        },
        new()
        {
            Key = VsBoxKeyRoot.RtBtnJoinBattlefield,
            TitleKey = "vsgame.Box.Title.JoinBattlefield",
            ImageSrc = string.Empty,
            BgImageSrc = $"{IMAGE_ROOT}/private.webp",
            Size = ContentBoxSize.BUTTON_WIDE,
            FooterDisplay = false,
            ClickId = 602,
            CheckEnable = root =>
                root.IsJoinBattlefieldEnabled
        },
        new()
        {
            Key = VsBoxKeyRoot.RtBtnRankedBattlefields,
            TitleKey = "vsgame.Box.Title.RankedBattlefields",
            ImageSrc = string.Empty,
            BgImageSrc = $"{IMAGE_ROOT}/ranked.webp",
            Size = ContentBoxSize.BUTTON_WIDE,
            FooterDisplay = true,
            ClickId = 603,
            CheckEnable = root =>
                root.IsRankedBattlefieldsEnabled,
            BuildFooter = (lang, root) =>
                lang["vsgame.Box.Footer.RankedRequirements"]
                    .FormatSafe(
                        root.RequiredBattleReadyCharacterCount,
                        root.RequiredCreditBalance)
        },
        new()
        {
            Key = VsBoxKeyContent.RankedBattleTeamManager,
            TitleKey = "vsgame.Manager.Title",
            ImageSrc = string.Empty,
            BgImageSrc = string.Empty,
            Size = ContentBoxSize.CONTENT_LARGE,
            FooterDisplay = false,
            ClickId = 0,
            RenderContent = 1,
            LcdBackground = true,
            BodyComp = typeof(RankedBattleTeamManager)
        }
    ];
}
