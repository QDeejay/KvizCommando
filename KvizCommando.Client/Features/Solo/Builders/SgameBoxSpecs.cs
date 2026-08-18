using KvizCommando.Client.Features.Solo.Components;
using KvizCommando.Client.Helpers;
using KvizCommando.Client.Models.ViewModels;
using KvizCommando.Client.Services.Visual.UiService.Language;
using KvizCommando.Shared.Models.Dtos;

namespace KvizCommando.Client.Features.Solo.Builders
{

    public class SgameBoxSpecs : VmSpecs
    {

        internal Enum Key { get; init; } = default!;
        internal Func<int, string> BuildImageSrc 
         { get; init; } =  _ => string.Empty;
        internal Func<SoloEnables, int, bool> BuildEnable 
        { get; init; } = (_, _) => true;
        internal Func<ILanguageService, SoloResults, int, string> BuildFooter
        { get; init; } = (_, _, _) => string.Empty; 
        internal Func<SoloComponentParameters, int, Dictionary<string, object?>> BuildParams
        { get; init; } = (_, _) => [];
    }

    public sealed class SgameBoxSub : SgameBoxSpecs
    {
        internal int BtnQnty { get; init; }
        internal Func<int, string, string> BuildTitle { get; init; } = default!;

    }

    public static class SoloBoxSpecs
    {
        public const int CATEGORY_BOX_COUNT = 16;
        public const int ORIENTATION_BOX_COUNT = 8;
        private const string IMAGE_ROOT = "images/buttons/solo";
        private static readonly string[] CatFileName =
            [
                "","religion", "famousdates","music","sport","technology","naturalscience","famouspepole","sculpture_painting",
                   "mythology","history","movies","game","it","geo_astro","fashion","literature"
            ];
        private static readonly string[] OriFileName =
            [
                "","teologist","historian","artist","gamer","engineer","scientist","trendy","educated"
            ];
        public static readonly IReadOnlyList<SgameBoxSpecs> Specs =
        [
            new SgameBoxSpecs {
                Key = SgameBoxKeyRoot.Category,
                TitleKey = "home.SubBox.Title.GameSolo.Category",
                ImageSrc = string.Empty, Size = "wide", FooterDisplay = true, ClickId = (int)SgameBoxKeyRoot.Category,
                BgImageSrc = $"{IMAGE_ROOT}/categories.webp",
                BuildEnable = (se,ix) => se.EnaCategory,
                BuildFooter = (lang, sr,ix) => lang["solo.Button.Footer.Catandori"].FormatSafe(sr.CategoryResults[0].Points)
            },
            new SgameBoxSpecs {
                Key = SgameBoxKeyRoot.Orientation,
                TitleKey = "home.SubBox.Title.GameSolo.Orientation",
                ImageSrc = string.Empty, Size = "wide", FooterDisplay = true, ClickId = (int)SgameBoxKeyRoot.Orientation,
                BgImageSrc = $"{IMAGE_ROOT}/orients.webp",
                BuildEnable = (se,ix) => se.EnaOrient,
                BuildFooter =(lang, sr, ix) => lang["solo.Button.Footer.Catandori"].FormatSafe(sr.OrientResults[0].Points)
            },
            new SgameBoxSpecs {
                Key = SgameBoxKeyRoot.Campaign,
                TitleKey = "home.SubBox.Title.GameSolo.Campaign",
                ImageSrc = string.Empty, Size = "wide", FooterDisplay = false, ClickId = (int)SgameBoxKeyRoot.Campaign,
                BgImageSrc = $"{IMAGE_ROOT}/campaign.webp",
                BuildEnable = (se,ix) => se.EnaCampaign,
            },
             new SgameBoxSpecs {
                Key = SgameBoxKeySub.GameBoxCat,
                TitleKey = "solo.Label.Title.Game.Category",
                ImageSrc = string.Empty, Size = ContentBoxSize.CONTENT_FLEXIBLE, FooterDisplay = false, ClickId = 0,
                BgImageSrc = string.Empty,
                RenderContent = 1, LcdBackground = false, BodyComp = typeof(SoloGameManager),
                BuildParams = (parameters, _) => BuildGameParameters(parameters)
             },
            new SgameBoxSpecs {
                Key = SgameBoxKeySub.GameBoxOri,
                TitleKey = "solo.Label.Title.Game.Orient",
                ImageSrc = string.Empty, Size = ContentBoxSize.CONTENT_FLEXIBLE, FooterDisplay = false, ClickId = 0,
                BgImageSrc = string.Empty,
                RenderContent = 1, LcdBackground = false, BodyComp = typeof(SoloGameManager),
                BuildParams = (parameters, _) => BuildGameParameters(parameters)
             }


        ];
        public static readonly IReadOnlyList<SgameBoxSub> SubSpecs =
        [
              new SgameBoxSub {
                Key = SgameBoxKeySub.BtnCat,
                BtnQnty = SoloBoxSpecs.CATEGORY_BOX_COUNT,
                BuildTitle = (ix, cult) => CategoryNameLocalizer.GetCategory(ix,cult),
                ImageSrc = string.Empty,
                BuildImageSrc = (ix) => $"{IMAGE_ROOT}/categories/{CatFileName[ix]}.webp", Size ="small", FooterDisplay=true, ClickId=(int)SgameBoxKeyRoot.Category,
                BuildEnable = (se,ix) => se.EnaCat[ix],
                BuildFooter = (lang,sr, ix) => lang["solo.Button.Footer.Games"].FormatSafe(sr.CategoryResults[ix].Points,sr.CategoryResults[ix].TimeStr),
                RenderContent = 1, LcdBackground = false, BodyComp = typeof(SoloCategoryStatus),
                BuildParams = (_, position) => new Dictionary<string, object?>
                    { [nameof(SoloCategoryStatus.CategoryPosition)] = position }
            },
            new SgameBoxSub {
                Key = SgameBoxKeySub.BtnOri,
                BtnQnty = SoloBoxSpecs.ORIENTATION_BOX_COUNT,
                BuildTitle = (ix, cult) => OrientationLocalizer.GetOrientation(ix,cult),
                ImageSrc = string.Empty,
                BuildImageSrc = (ix) => $"{IMAGE_ROOT}/orients/{OriFileName[ix]}.webp", Size ="tall", FooterDisplay=true, ClickId=(int)SgameBoxKeyRoot.Orientation,
                BuildEnable = (se,ix) => se.EnaOri[ix],
                BuildFooter = (lang,sr,ix) => lang["solo.Button.Footer.Games"].FormatSafe(sr.OrientResults[ix].Points,sr.OrientResults[ix].TimeStr),
                RenderContent = 1, LcdBackground = false, BodyComp = typeof(SoloOrientationStatus),
                BuildParams = (_, position) => new Dictionary<string, object?>
                    { [nameof(SoloOrientationStatus.CharacterPosition)] = position }
            }

        ];

        private static Dictionary<string, object?> BuildGameParameters(
            SoloComponentParameters parameters) =>
            new()
            {
                [nameof(SoloGameManager.Mode)] = parameters.Mode,
                [nameof(SoloGameManager.SelectionId)] = parameters.SelectionId,
                [nameof(SoloGameManager.Title)] = parameters.Title,
                [nameof(SoloGameManager.OnGameCompletedChanged)] = parameters.OnGameCompletedChanged,
                [nameof(SoloGameManager.OnTeamLevelChanged)] = parameters.OnTeamLevelChanged
            };
    }
}
