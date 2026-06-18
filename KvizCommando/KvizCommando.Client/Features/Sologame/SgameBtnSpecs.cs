using KvizCommando.Client.Features.Question;
using KvizCommando.Client.Helpers;
using KvizCommando.Client.Models.ViewModels;
using KvizCommando.Client.Services.Language;
using KvizCommando.Shared.Models.Dtos;

namespace KvizCommando.Client.Features.Sologame
{ 

   
    public abstract class SgameBtnSpecs : ButtonVm
    {

        internal Enum Key { get; init; } = default!;
        internal Func<int, string> BuildImageSrc { get; init; } = default!;
        internal Func<SoloEnables,int, bool> BuildEnable { get; init; } = default!;
    }

    public sealed class SgameBtnRoot : SgameBtnSpecs
    {
        internal Func<ILanguageService, SoloResults, string> BuildFooter { get; init; } = default!;
    }
    public sealed class SgameBtnContent : SgameBtnSpecs
    {
        internal int BtnQnty { get; init; }
        internal Func<int, string, string> BuildTitle { get; init; } = default!;
        internal Func<ILanguageService, SoloResults, int, string> BuildFooter { get; init; } = default!;
    }


    

    public static class SoloButtonSpecs
    {
     
        private const int CatQnty = 16;
        private const int OriQnty = 8;

        private static readonly string[] CatFileName = 
            {
                "","religion", "famousdates","music","sport","technology","naturalscience","famouspepole","sculpture_painting",
                   "mythology","history","movies","game","it","geo_astro","fashion","literature"
            };
        private static readonly string[] OriFileName =
           {
                "","teologist","historian","artist","gamer","engineer","scientist","trendy","educated"
            };
        public static readonly IReadOnlyList<SgameBtnRoot> RootSpecs = new[]
        {
            new SgameBtnRoot {
                Key = SgameBoxKeyRoot.RtBtnCategory,
                TitleKey = "solo.Button.Title.Categories",
                ImageSrc = "images/solo/categories.webp", Size = "wide", FooterDisplay = true, ClickId = 401,
                BuildEnable = (se,ix) => se.EnaCategory,
                BuildFooter = (lang, sr) => lang["solo.Button.Footer.Catandori"].FormatSafe(sr.CategoryResults[0].Points)
            },
            new SgameBtnRoot {
                Key = SgameBoxKeyRoot.RtBtnOrient,
                TitleKey = "solo.Button.Title.Orients",
                ImageSrc = "images/solo/orients.webp", Size = "wide", FooterDisplay = true, ClickId = 402,
                BuildEnable = (se,ix) => se.EnaOrient,
                BuildFooter =(lang, sr) => lang["solo.Button.Footer.Catandori"].FormatSafe(sr.OrientResults[0].Points)
            },
            new SgameBtnRoot {
                Key = SgameBoxKeyRoot.RtBtnCampaign,
                TitleKey = "solo.Button.Title.Campaign",
                ImageSrc = "images/solo/campaign.webp", Size = "wide", FooterDisplay = false, ClickId = 403,
                BuildEnable = (se,ix) => se.EnaCampaign,
                BuildFooter = (lang, sr) => ""
            },
        };
        public static readonly IReadOnlyList<SgameBtnContent> ContentSpecs = new[]
        {
              new SgameBtnContent {
                Key = SgameBoxKeySub.BtnCat,
                BtnQnty = CatQnty,
                BuildTitle = (ix, cult) => CategoryNameLocalizer.GetCategory(ix,cult),
                BuildImageSrc = (ix) => $"images/categories/{CatFileName[ix]}.webp", Size ="small", FooterDisplay=true, ClickId=420,
                BuildEnable = (se,ix) => se.EnaCat[ix],
                BuildFooter = (lang,sr, ix) => lang["solo.Button.Footer.Games"].FormatSafe(sr.CategoryResults[ix].Points,sr.CategoryResults[ix].Time)
            },
            new SgameBtnContent {
                Key = SgameBoxKeySub.BtnOri,
                BtnQnty = OriQnty,
                BuildTitle = (ix, cult) => OrientationLocalizer.GetOrientation(ix,cult),
                BuildImageSrc = (ix) => $"images/orients/{OriFileName[ix]}.webp", Size ="tall", FooterDisplay=true, ClickId=450,
                BuildEnable = (se,ix) => se.EnaOri[ix],
                BuildFooter = (lang,sr,ix) => lang["solo.Button.Footer.Games"].FormatSafe(sr.OrientResults[ix].Points,sr.OrientResults[ix].Time)
            }

        };
    }
}