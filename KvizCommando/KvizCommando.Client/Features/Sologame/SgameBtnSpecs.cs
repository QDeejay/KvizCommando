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
        internal Func<SoloGameDtos,int, bool> BuildEnable { get; init; } = default!;
    }

    public sealed class SgameBtnRoot : SgameBtnSpecs
    {
        internal Func<ILanguageService, SoloGameDtos, string> BuildFooter { get; init; } = default!;
    }
    public sealed class SgameBtnContent : SgameBtnSpecs
    {
        internal int BtnQnty { get; init; }
        internal Func<int, string, string> BuildTitle { get; init; } = default!;
        internal Func<ILanguageService, ResultDto, string> BuildFooter { get; init; } = default!;
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
                BuildEnable = (sg,ix) => sg.GameCategoryEna,
                BuildFooter = (lang, sg) => lang["solo.Button.Footer.Catandori"].FormatSafe(sg.CategoryOverall)
            },
            new SgameBtnRoot {
                Key = SgameBoxKeyRoot.RtBtnOrient,
                TitleKey = "solo.Button.Title.Orients",
                ImageSrc = "images/solo/orients.webp", Size = "wide", FooterDisplay = true, ClickId = 402,
                BuildEnable = (sg,ix) => sg.GameOrientsEna,
                BuildFooter =(lang, sg) => lang["solo.Button.Footer.Catandori"].FormatSafe(sg.OrientOverall)
            },
            new SgameBtnRoot {
                Key = SgameBoxKeyRoot.RtBtnCampaign,
                TitleKey = "solo.Button.Title.Campaign",
                ImageSrc = "images/solo/campaign.webp", Size = "wide", FooterDisplay = false, ClickId = 403,
                BuildEnable = (sg,ix) => sg.GameCampaignEna,
                BuildFooter = (lang, sg) => ""
            },

        };
        public static readonly IReadOnlyList<SgameBtnContent> ContentSpecs = new[]
        {
              new SgameBtnContent {
                Key = SgameBoxKeySub.BtnCat,
                BtnQnty = CatQnty,
                BuildTitle = (ix, cult) => CategoryNameLocalizer.GetCategory(ix,cult),
                BuildImageSrc = (ix) => $"images/categories/{CatFileName[ix]}.webp", Size ="small", FooterDisplay=true, ClickId=420,
                BuildEnable = (sg,ix) => sg.CatEna[ix],
                BuildFooter = (lang,rd) => lang["solo.Button.Footer.Games"].FormatSafe(rd.Points,rd.Time)
            },
            new SgameBtnContent {
                Key = SgameBoxKeySub.BtnOri,
                BtnQnty = OriQnty,
                BuildTitle = (ix, cult) => OrientationLocalizer.GetOrientation(ix,cult),
                BuildImageSrc = (ix) => $"images/orients/{OriFileName[ix]}.webp", Size ="tall", FooterDisplay=true, ClickId=450,
                BuildEnable = (sg,ix) => sg.OrientEna[ix],
                BuildFooter = (lang,rd) => lang["solo.Button.Footer.Games"].FormatSafe(rd.Points,rd.Time)
            }

        };
    }
}