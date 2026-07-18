using KvizCommando.Client.Helpers;
using KvizCommando.Client.Models.ViewModels;
using KvizCommando.Client.Services.Visual.UiService.Language;
using KvizCommando.Shared.Models.Dtos;

namespace KvizCommando.Client.Pages.Solo.Features
{

    public abstract class SgameBoxSpecs : VmSpecs
    {

        internal Enum Key { get; init; } = default!;
        internal Func<int, string> BuildImageSrc { get; init; } = default!;
        internal Func<SoloEnables, int, bool> BuildEnable { get; init; } = default!;
        internal Func<ILanguageService, SoloResults, int, string> BuildFooter { get; init; } = default!;
    }

    public sealed class SgameBoxRoot : SgameBoxSpecs
    {

    }
    public sealed class SgameBoxSub : SgameBoxSpecs
    {
        internal int BtnQnty { get; init; }
        internal Func<int, string, string> BuildTitle { get; init; } = default!;

    }




    public static class SoloBoxSpecs
    {
        private static readonly string[] CatFileName =
            [
                "","religion", "famousdates","music","sport","technology","naturalscience","famouspepole","sculpture_painting",
                   "mythology","history","movies","game","it","geo_astro","fashion","literature"
            ];
        private static readonly string[] OriFileName =
            [
                "","teologist","historian","artist","gamer","engineer","scientist","trendy","educated"
            ];
        public static readonly IReadOnlyList<SgameBoxRoot> RootSpecs =
        [
            new SgameBoxRoot {
                Key = SgameBoxKeyRoot.RtBtnCategory,
                TitleKey = "solo.Button.Title.Categories",
                ImageSrc = string.Empty, Size = "wide", FooterDisplay = true, ClickId = 401,
                BgImageSrc = "images/solo/categories.webp",
                BuildEnable = (se,ix) => se.EnaCategory,
                BuildFooter = (lang, sr,ix) => lang["solo.Button.Footer.Catandori"].FormatSafe(sr.CategoryResults[0].Points)
            },
            new SgameBoxRoot {
                Key = SgameBoxKeyRoot.RtBtnOrient,
                TitleKey = "solo.Button.Title.Orients",
                ImageSrc = string.Empty, Size = "wide", FooterDisplay = true, ClickId = 402,
                BgImageSrc = "images/solo/orients.webp",
                BuildEnable = (se,ix) => se.EnaOrient,
                BuildFooter =(lang, sr, ix) => lang["solo.Button.Footer.Catandori"].FormatSafe(sr.OrientResults[0].Points)
            },
            new SgameBoxRoot {
                Key = SgameBoxKeyRoot.RtBtnCampaign,
                TitleKey = "solo.Button.Title.Campaign",
                ImageSrc = string.Empty, Size = "wide", FooterDisplay = false, ClickId = 403,
                BgImageSrc = "images/solo/campaign.webp",
                BuildEnable = (se,ix) => se.EnaCampaign,
                BuildFooter = (lang, sr, ix) => ""
            },
        ];
        public static readonly IReadOnlyList<SgameBoxSub> SubSpecs = new[]
        {
              new SgameBoxSub {
                Key = SgameBoxKeySub.BtnCat,
                BtnQnty = (int)SgameBoxKeySub.BtnCat,
                BuildTitle = (ix, cult) => CategoryNameLocalizer.GetCategory(ix,cult),
                ImageSrc = string.Empty,
                BuildImageSrc = (ix) => $"images/categories/{CatFileName[ix]}.webp", Size ="small", FooterDisplay=true, ClickId=420,
                BuildEnable = (se,ix) => se.EnaCat[ix],
                BuildFooter = (lang,sr, ix) => lang["solo.Button.Footer.Games"].FormatSafe(sr.CategoryResults[ix].Points,sr.CategoryResults[ix].Time)
            },
            new SgameBoxSub {
                Key = SgameBoxKeySub.BtnOri,
                BtnQnty = (int)SgameBoxKeySub.BtnOri,
                BuildTitle = (ix, cult) => OrientationLocalizer.GetOrientation(ix,cult),
                ImageSrc = string.Empty,
                BuildImageSrc = (ix) => $"images/orients/{OriFileName[ix]}.webp", Size ="tall", FooterDisplay=true, ClickId=450,
                BuildEnable = (se,ix) => se.EnaOri[ix],
                BuildFooter = (lang,sr,ix) => lang["solo.Button.Footer.Games"].FormatSafe(sr.OrientResults[ix].Points,sr.OrientResults[ix].Time)
            }

        };
    }
}