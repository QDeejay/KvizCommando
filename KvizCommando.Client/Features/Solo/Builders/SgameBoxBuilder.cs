using KvizCommando.Client.Models.ViewModels;
using KvizCommando.Client.Services.Visual.UiService.Language;
using KvizCommando.Shared.Models.Dtos;

namespace KvizCommando.Client.Features.Solo.Builders
{
    public static class SgameBoxBuilder
    {
        public static readonly string[] Root = [SgameBoxKeyRoot.Category.ToString(), SgameBoxKeyRoot.Orientation.ToString(), SgameBoxKeyRoot.Campaign.ToString()];
        public static readonly string[] GameCat = [SgameBoxKeySub.GameBoxCat.ToString()];
        public static readonly string[] GameOri = [SgameBoxKeySub.GameBoxOri.ToString()];
        public static readonly string[] SubCat = BuildNames(SgameBoxKeySub.BtnCat, SoloBoxSpecs.CATEGORY_BOX_COUNT);
        public static readonly string[] SubOri = BuildNames(SgameBoxKeySub.BtnOri, SoloBoxSpecs.ORIENTATION_BOX_COUNT);

        public static Dictionary<string, ContentBoxVm> BuildBoxes(
            SoloGameDtos ss,
            SoloComponentParameters parameters,
            string cult,
            ILanguageService lang)
        {
            var dict = new Dictionary<string, ContentBoxVm>();

            foreach (var spec in SoloBoxSpecs.Specs)
            {
                var key = spec.Key.ToString();
                var isEnabled = spec.BuildEnable(ss.Enables, 1);

                dict.Add(key, new ContentBoxVm
                {
                    DictKey = key,
                    Header = lang[spec.TitleKey],
                    Footer = spec.BuildFooter(lang, ss.Results, 0),
                    FooterDisplay = spec.FooterDisplay,
                    Size = spec.Size,
                    ImageSrc = spec.ImageSrc,
                    BgImageSrc = spec.BgImageSrc,
                    IsClickable = isEnabled && spec.ClickId > 0,
                    IsEnabled = isEnabled,
                    ClickId = spec.ClickId,
                    LcdDisplay = spec.LcdBackground,
                    RenderContent = spec.RenderContent,
                    BodyComponent = spec.BodyComp,
                    BodyParameters = spec.BodyComp is not null
                        ? spec.BuildParams(parameters, 0)
                        : []
                });

            }
            foreach (var spec in SoloBoxSpecs.SubSpecs)
            {
                for (int i = 1; i <= spec.BtnQnty; i++)
                {
                    var key = $"{spec.Key}{i}";
                    var isEnabled = spec.BuildEnable(ss.Enables, i - 1);

                    dict.Add(key, new ContentBoxVm
                    {
                        DictKey = key,
                        Header = spec.BuildTitle(i, cult),
                        Footer = spec.BuildFooter(lang, ss.Results, i),
                        FooterDisplay = spec.FooterDisplay,
                        Size = spec.Size,
                        ImageSrc = spec.ImageSrc,
                        BgImageSrc = spec.BuildImageSrc(i),
                        IsClickable = isEnabled,
                        IsEnabled = isEnabled,
                        ClickId = spec.ClickId + i,
                        LcdDisplay = spec.LcdBackground,
                        RenderContent = spec.RenderContent,
                        BodyComponent = spec.BodyComp,
                        BodyParameters = spec.BodyComp is not null
                            ? spec.BuildParams(parameters, i)
                            : []
                    });
                }
            }

            return dict;
        }
        private static string[] BuildNames(SgameBoxKeySub name, int count)
        {
            string[] names = new string[count];
            for (int i = 0; i < count; i++)
            {
                names[i] = $"{name.ToString()}{i + 1}";
            }
            return names;
        }
    }

}
