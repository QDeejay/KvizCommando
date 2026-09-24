using KvizCommando.Client.Features.Rankings.Components;
using KvizCommando.Client.Models.ViewModels;
using KvizCommando.Localization.Rankings;
using KvizCommando.Shared.Models.Dtos;
using Microsoft.Extensions.Localization;

namespace KvizCommando.Client.Features.Rankings.Builders;

/// <summary>Egy ranglistaoldali doboz megjelenítési adatai.</summary>
public sealed class RankingBoxSpecs : VmSpecs
{
    internal RankingBoxKey Key { get; init; }
    internal bool Enabled { get; init; } = true;
    internal Func<IStringLocalizer<RankingResource>, RankingDtos?, string> BuildFooter
        { get; init; } = (_, _) => string.Empty;
}

/// <summary>A ranglisták gyökér- és tartalmi dobozainak meghatározásai.</summary>
public static class RankingsBoxSpecs
{
    private const string IMAGE_ROOT = "images/buttons/rankings";

    /// <summary>A ranglistákhoz tartozó összes doboz specifikációja.</summary>
    public static readonly IReadOnlyList<RankingBoxSpecs> Specs =
    [
        new()
        {
            Key = RankingBoxKey.Solo,
            TitleKey = "Ranking.Solo",
            BgImageSrc = $"{IMAGE_ROOT}/solo.webp",
            FooterDisplay = true,
            BuildFooter = (lang, rankings) =>
                rankings?.SoloOverall.CurrentPosition is int position && position > 0
                    ? lang["ranking.Footer.Position", position]
                    : lang["ranking.Footer.Unranked"],
            Size = ContentBoxSize.BUTTON_WIDE,
            ClickId = (int)RankingBoxKey.Solo
        },
        new()
        {
            Key = RankingBoxKey.Vs,
            TitleKey = "Ranking.Vs",
            BgImageSrc = $"{IMAGE_ROOT}/vs.webp",
            FooterDisplay = true,
            BuildFooter = (lang, rankings) =>
                rankings?.VsAllTime.CurrentPosition is int position && position > 0
                    ? lang["ranking.Footer.Position", position]
                    : lang["ranking.Footer.Unranked"],
            Size = ContentBoxSize.BUTTON_WIDE,
            ClickId = (int)RankingBoxKey.Vs
        },
        new()
        {
            Key = RankingBoxKey.Memorial,
            TitleKey = "Ranking.Memorial",
            BgImageSrc = $"{IMAGE_ROOT}/memorial.webp",
            FooterDisplay = true,
            BuildFooter = (lang, _) => lang["ranking.Footer.Unranked"],
            Size = ContentBoxSize.BUTTON_WIDE,
            ClickId = (int)RankingBoxKey.Memorial,
            Enabled = false
        },
        new()
        {
            Key = RankingBoxKey.SoloList,
            TitleKey = "Ranking.Solo",
            Size = ContentBoxSize.CONTENT_FLEXIBLE,
            RenderContent = 1,
            BodyComp = typeof(RankingBoard)
        },
        new()
        {
            Key = RankingBoxKey.VsList,
            TitleKey = "Ranking.Vs",
            Size = ContentBoxSize.CONTENT_FLEXIBLE,
            RenderContent = 1,
            BodyComp = typeof(RankingBoard)
        }
    ];
}
