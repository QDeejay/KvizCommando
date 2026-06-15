using KvizCommando.Client.Helpers;
using KvizCommando.Client.Models.ViewModels;
using KvizCommando.Client.Services.Language;
using KvizCommando.Shared.Models.Dtos;

namespace KvizCommando.Client.Features.Sologame
{ 

   
    public sealed class SgameBtnSpecs : ButtonVm
    {
        public Func<int, string> BuildImageSrc { get; init; } = default!;
        public Func<ILanguageService, ResultDtos, string> BuildFooter { get; init; } = default!;
    }

    public static class SoloButtonSpecs
    {
        private static readonly string[] CatFileName = 
            {
                "","religion", "famousdates","music","sport","technology","naturalscience","famouspepole","sculpture_painting",
                   "mythology","history","movies","game","it","geo_astro","fashion","literature"
            };
        private static readonly string[] OriFileName =
           {
                "","teologist","historian","artist","gamer","engineer","scientist","trendy","educated"
            };
        public static readonly IReadOnlyList<SgameBtnSpecs> Specs = new[]
        {
            new SgameBtnSpecs {
                TitleKey = "solo.Button.Title.Categories",
                ImageSrc = "images/solo/categories.webp", Size = "wide", FooterDisplay = true, ClickId = 10,
                BuildFooter = (lang, sg) => lang["solo.Button.Footer.Catandori"].FormatSafe(sg.Points)
            },
            new SgameBtnSpecs {
                TitleKey = "solo.Button.Title.Orients",
                ImageSrc = "images/solo/orients.webp", Size = "wide", FooterDisplay = true, ClickId = 11,
                BuildFooter =(lang, sg) => lang["solo.Button.Footer.Catandori"].FormatSafe(sg.Points)
            },
            new SgameBtnSpecs {
                TitleKey = "solo.Button.Title.Campaign",
                ImageSrc = "images/solo/campaign.webp", Size = "wide", FooterDisplay = false, ClickId = 12,
                BuildFooter = (lang, sg) => ""
            },

            new SgameBtnSpecs {
                //BuildTitle = (ix, cult) => CategoryNameLocalizer.GetCategory(ix,cult),
                BuildImageSrc = (ix) => $"images/categories/{CatFileName[ix]}.webp", Size ="small", FooterDisplay=true, ClickId=20,
                BuildFooter = (lang,sg) => lang["solo.Button.Footer.Games"].FormatSafe(sg.Points,sg.Time)
            },
            new SgameBtnSpecs {
                //BuildTitle = (ix, cult) => CategoryNameLocalizer.GetCategory(ix,cult),
                BuildImageSrc = (ix) => $"images/orients/{OriFileName[ix]}.webp", Size ="tall", FooterDisplay=true, ClickId=50,
                BuildFooter = (lang,sg) => lang["solo.Button.Footer.Games"].FormatSafe(sg.Points,sg.Time)
            }
        };
    }
}