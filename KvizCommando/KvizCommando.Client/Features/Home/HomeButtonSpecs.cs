using KvizCommando.Client.Helpers;
using KvizCommando.Client.Models.ViewModels;
using KvizCommando.Client.Services.Language;
using KvizCommando.Shared.Models.Dtos; // itt van a HomeDTOs, HomeScreen, ScreenButtonEntity

namespace KvizCommando.Client.Features.Home;

public sealed class ButtonSpec : ButtonVm
{
    internal HomeBoxKey Key { get; init; }
    internal Func<HomeScreen, ScreenButtonEntity> Pick { get; init; } = default!;
    internal Func<ILanguageService, ScreenButtonEntity, string> BuildFooter { get; init; } = default!;
}

public static class HomeButtonSpecs
{
    public static readonly IReadOnlyList<ButtonSpec> Specs = new[]
    {
        new ButtonSpec {
            Key = HomeBoxKey.GameVs, 
            TitleKey = "home.Box.Title.GameVs",
            ImageSrc = "images/buttons/game.webp", Size = "wide", FooterDisplay = true, ClickId = 3,
            Pick = s => s.VsGame,
            BuildFooter = (lang, b) => lang["home.Box.Footer.GameVs"].FormatSafe(b.FooterData1)
        },
        new ButtonSpec {
            Key = HomeBoxKey.GameSolo,
            TitleKey = "home.Box.Title.GameSolo",
            ImageSrc = "images/buttons/gamesolo.webp", Size = "small", FooterDisplay = false, ClickId = 4,
            Pick = s => s.SoloGame,
            BuildFooter = (lang, b) => ""
        },
        new ButtonSpec {
            Key = HomeBoxKey.Shop, 
            TitleKey = "home.Box.Title.Shop",
            ImageSrc = "images/buttons/shop.webp", Size = "medium", FooterDisplay = false, ClickId = 4,
            Pick = s => s.Shop,
            BuildFooter = (lang, b) => ""
        },
        new ButtonSpec {
            Key = HomeBoxKey.Rankings, 
            TitleKey = "home.Box.Title.Rankings",
            ImageSrc = "images/buttons/ranking.webp", Size = "medium", FooterDisplay = true, ClickId = 5,
            Pick = s => s.Ranking,
            BuildFooter = (lang, b) =>
                b.FooterData1 < 1
                ? lang["home.Box.Footer.Rankings2"]
                : lang["home.Box.Footer.Rankings1"].FormatSafe(b.FooterData1)
        },
        new ButtonSpec {
            Key = HomeBoxKey.Statistic, 
            TitleKey = "home.Box.Title.Statistic",
            ImageSrc = "images/buttons/statistic.webp", Size = "small", FooterDisplay = false, ClickId = 6,
            Pick = s => s.Statistic,
            BuildFooter = (lang, b) => ""
        },
        new ButtonSpec {
            Key = HomeBoxKey.Events, 
            TitleKey = "home.Box.Title.Events",
            ImageSrc = "images/buttons/events.webp", Size = "wide", FooterDisplay = true, ClickId = 7,
            Pick = s => s.Events,
            BuildFooter = (lang, b) => b.FooterData1 switch
            {
                99 => lang["home.Box.Footer.Events2"],
                > 0 => lang["home.Box.Footer.Events1"].FormatSafe(b.FooterData1),
                0   => lang["home.Box.Footer.Events3"].FormatSafe(b.FooterData2), // pl. duration
                _   => $"#{b.FooterData1}"
            }
        },
        new ButtonSpec {
            Key = HomeBoxKey.Community, 
            TitleKey = "home.Box.Title.Community",
            ImageSrc = "images/buttons/community.webp", Size = "medium", FooterDisplay = true, ClickId = 8,
            Pick = s => s.Community,
            BuildFooter = (lang, b) => lang["home.Box.Footer.Community"].FormatSafe(b.FooterData1, b.FooterData2)
        },
        new ButtonSpec {
            Key = HomeBoxKey.Messages, 
            TitleKey = "home.Box.Title.Messages",
            ImageSrc = "images/buttons/messages.webp", Size = "medium", FooterDisplay = true, ClickId = 9,
            Pick = s => s.Messages,
            BuildFooter = (lang, b) => lang["home.Box.Footer.Messages"].FormatSafe(b.FooterData1)
        },
        new ButtonSpec {
            Key = HomeBoxKey.Team, 
            TitleKey = "home.Box.Title.Team",
            ImageSrc = "images/buttons/team.webp", Size = "medium", FooterDisplay = true, ClickId = 2,
            Pick = s => s.Team,
            BuildFooter = (lang, b) => lang["home.Box.Footer.Team"].FormatSafe(b.FooterData1, b.FooterData2)
        },
        new ButtonSpec {
            Key = HomeBoxKey.Question, 
            TitleKey = "home.Box.Title.Question",
            ImageSrc = "images/buttons/questions.webp", Size = "medium",
            FooterDisplay = false,
            ClickId = 1,
            Pick = s => s.Question,
            BuildFooter = (lang, b) => ""
        },
        new ButtonSpec {
            Key = HomeBoxKey.InfoBoard, 
            TitleKey = "home.Title.BulleteinBoard",
            ImageSrc = string.Empty, Size = string.Empty,
            LcdBackground=true,
            FooterDisplay = false,
            ClickId = 0,
            Pick = s => s.InfoBoard,
            BuildFooter = (lang, b) => ""
        }
    };
}
