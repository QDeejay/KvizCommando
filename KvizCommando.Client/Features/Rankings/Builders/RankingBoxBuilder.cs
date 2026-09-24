using KvizCommando.Client.Features.Rankings.Components;
using KvizCommando.Client.Models.ViewModels;
using KvizCommando.Localization.Rankings;
using KvizCommando.Shared.Models.Dtos;
using Microsoft.Extensions.Localization;
using KvizCommando.Shared.Models.Enums;

namespace KvizCommando.Client.Features.Rankings.Builders;

/// <summary>Összeállítja a ranglista képernyő dobozait.</summary>
public static class RankingBoxBuilder
{
    /// <summary>A gyökérképernyő dobozainak sorrendje.</summary>
    public static readonly string[] Root =
    [
        RankingBoxKey.Solo.ToString(),
        RankingBoxKey.Vs.ToString(),
        RankingBoxKey.Memorial.ToString()
    ];

    /// <summary>A Solo nézet tartalmi doboza.</summary>
    public static readonly string[] Solo = [RankingBoxKey.SoloList.ToString()];

    /// <summary>A VS nézet tartalmi doboza.</summary>
    public static readonly string[] Vs = [RankingBoxKey.VsList.ToString()];

    /// <summary>A közös boxcímekkel összeállítja a gyökér- és tartalmi dobozokat.</summary>
    /// <param name="boxTitles">A közös dobozcímek lokalizált értékei.</param>
    /// <param name="rankings">A játékos ranglistaeredményei.</param>
    /// <param name="lang">A láblécek lokalizált feliratai.</param>
    /// <returns>A dobozok kulcs szerinti nézetmodelljei.</returns>
    public static Dictionary<string, ContentBoxVm> BuildBoxes(
        IReadOnlyDictionary<string, string> boxTitles,
        RankingDtos? rankings,
        IStringLocalizer<RankingResource> lang)
    {
        var boxes = new Dictionary<string, ContentBoxVm>();

        foreach (var spec in RankingsBoxSpecs.Specs)
        {
            var key = spec.Key.ToString();
            boxes.Add(key, new ContentBoxVm
            {
                DictKey = key,
                Header = boxTitles[spec.TitleKey],
                Footer = spec.FooterDisplay
                    ? spec.BuildFooter(lang, rankings)
                    : string.Empty,
                FooterDisplay = spec.FooterDisplay,
                Size = spec.Size,
                BgImageSrc = spec.BgImageSrc,
                IsEnabled = spec.Enabled,
                IsClickable = spec.Enabled && spec.ClickId > 0,
                ClickId = spec.ClickId,
                RenderContent = spec.RenderContent,
                BodyComponent = spec.BodyComp,
                BodyParameters = spec.Key switch
                {
                    RankingBoxKey.SoloList => new()
                    {
                        [nameof(RankingBoard.InitialList)] = RankingList.SoloOverall
                    },
                    RankingBoxKey.VsList => new()
                    {
                        [nameof(RankingBoard.InitialList)] = RankingList.VsAllTime
                    },
                    _ => []
                }
            });
        }

        return boxes;
    }
}
