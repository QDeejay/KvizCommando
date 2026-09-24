using KvizCommando.Client.Models.ViewModels;
using KvizCommando.Localization.Home;
using KvizCommando.Shared.Models.Dtos; // itt van a HomeDTOs, HomeScreen, ScreenButtonEntity
using Microsoft.Extensions.Localization;

namespace KvizCommando.Client.Features.Home.Builders;

/// <summary>
/// Egy kezdőképernyő-doboz megjelenítési szabályait tartalmazza.
/// </summary>
public sealed class HomeSpecs : VmSpecs
{
    internal HomeBoxKey Key { get; init; }
    internal Func<HomeScreen, ScreenButtonEntity> Pick { get; init; } = default!;
    internal Func<IStringLocalizer<HomeResource>, ScreenButtonEntity, string> BuildFooter
        { get; init; } = (_, _) => string.Empty;
}

/// <summary>
/// A kezdőképernyő összes dobozának rögzített megjelenítési leírása.
/// </summary>
public static class HomeBoxSpecs
{
    private const string IMAGE_ROOT = "images/buttons/home";

    /// <summary>A kezdőképernyő dobozainak sorrendbe rendezett leírása.</summary>
    public static readonly IReadOnlyList<HomeSpecs> Specs = new[]
    {
        new HomeSpecs {
            Key = HomeBoxKey.GameVs,
            TitleKey = "Root.GameVs",
            ImageSrc = string.Empty,
            BgImageSrc =$"{IMAGE_ROOT}/game.webp",
            Size = ContentBoxSize.BUTTON_WIDE,
            FooterDisplay = true, ClickId = (int)HomeBoxKey.GameVs,
            Pick = s => s.VsGame,
            BuildFooter = (lang, b) =>
                lang["home.Box.Footer.GameVs", b.FooterData1]
        },
        new HomeSpecs {
            Key = HomeBoxKey.GameSolo,
            TitleKey = "Root.GameSolo",
            ImageSrc = string.Empty,
            BgImageSrc = $"{IMAGE_ROOT}/gamesolo.webp",
            Size = ContentBoxSize.BUTTON_SMALL,
            FooterDisplay = false, ClickId = (int)HomeBoxKey.GameSolo,
            Pick = s => s.SoloGame,
        
        },
        new HomeSpecs {
            Key = HomeBoxKey.Shop,
            TitleKey = "Root.Shop",
            ImageSrc = string.Empty,
            BgImageSrc = $"{IMAGE_ROOT}/shop.webp",
            Size = ContentBoxSize.BUTTON_MEDIUM,
            FooterDisplay = false, ClickId = (int)HomeBoxKey.Shop,
            Pick = s => s.Shop,
          
        },
        new HomeSpecs {
            Key = HomeBoxKey.Rankings,
            TitleKey = "Root.Rankings",
            ImageSrc = string.Empty,
            BgImageSrc = $"{IMAGE_ROOT}/ranking.webp",
            Size = ContentBoxSize.BUTTON_MEDIUM,
            FooterDisplay = false, ClickId = (int)HomeBoxKey.Rankings,
            Pick = s => s.Ranking
        },
        new HomeSpecs {
            Key = HomeBoxKey.Statistic,
            TitleKey = "Root.Statistic",
            BgImageSrc = $"{IMAGE_ROOT}/statistic.webp",
            ImageSrc = string.Empty,
            Size =ContentBoxSize.BUTTON_SMALL,
            FooterDisplay = false, ClickId = (int)HomeBoxKey.Statistic,
            Pick = s => s.Statistic,
           
        },
        new HomeSpecs {
            Key = HomeBoxKey.Events,
            TitleKey = "Root.Events",
            ImageSrc = string.Empty,
            BgImageSrc = $"{IMAGE_ROOT}/events.webp",
            Size = ContentBoxSize.BUTTON_WIDE,
            FooterDisplay = true, ClickId = (int)HomeBoxKey.Events,
            Pick = s => s.Events,
            BuildFooter = (lang, b) => b.FooterData1 switch
            {
                99 => lang["home.Box.Footer.Events2"],
                > 0 => lang["home.Box.Footer.Events1", b.FooterData1],
                0   => lang["home.Box.Footer.Events3", b.FooterData2], // pl. duration
                _   => $"#{b.FooterData1}"
            }
        },
        new HomeSpecs {
            Key = HomeBoxKey.Community,
            TitleKey = "Root.Community",
            ImageSrc = string.Empty,
            BgImageSrc = $"{IMAGE_ROOT}/community.webp",
            Size = ContentBoxSize.BUTTON_MEDIUM,
            FooterDisplay = true, ClickId = (int)HomeBoxKey.Community,
            Pick = s => s.Community,
            BuildFooter = (lang, b) =>
                lang["home.Box.Footer.Community", b.FooterData1, b.FooterData2]
        },
        new HomeSpecs {
            Key = HomeBoxKey.Messages,
            TitleKey = "Root.Messages",
            ImageSrc = string.Empty,
            BgImageSrc = $"{IMAGE_ROOT}/messages.webp",
            Size = ContentBoxSize.BUTTON_MEDIUM,
            FooterDisplay = true, ClickId = (int)HomeBoxKey.Messages,
            Pick = s => s.Messages,
            BuildFooter = (lang, b) =>
                lang["home.Box.Footer.Messages", b.FooterData1]
        },
        new HomeSpecs {
            Key = HomeBoxKey.Team,
            TitleKey = "Root.Team",
            ImageSrc = string.Empty,
            BgImageSrc = $"{IMAGE_ROOT}/team.webp",
            Size = ContentBoxSize.BUTTON_MEDIUM,
            FooterDisplay = true, ClickId = (int)HomeBoxKey.Team,
            Pick = s => s.Team,
            BuildFooter = (lang, b) =>
                lang["home.Box.Footer.Team", b.FooterData1, b.FooterData2]
        },
        new HomeSpecs {
            Key = HomeBoxKey.Question,
            TitleKey = "Root.Question",
            ImageSrc = string.Empty,
            BgImageSrc = $"{IMAGE_ROOT}/questions.webp",
            Size = ContentBoxSize.BUTTON_MEDIUM,
            FooterDisplay = false,
            ClickId = (int)HomeBoxKey.Question,
            Pick = s => s.Question
        },
        new HomeSpecs {
            Key = HomeBoxKey.InfoBoard,
            TitleKey = "home.Title.BulleteinBoard",
            ImageSrc = string.Empty,
            BgImageSrc = string.Empty, 
            Size = ContentBoxSize.CONTENT_LARGE,
            ReSizable = true,
            RenderContent = 1,
            LcdBackground=true,
            FooterDisplay = false,
            ClickId = 0,
            Pick = s => s.InfoBoard
        }
    };
}
