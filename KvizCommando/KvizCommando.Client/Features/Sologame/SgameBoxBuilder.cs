using KvizCommando.Client.Features.Home;
using KvizCommando.Client.Features.Question;
using KvizCommando.Client.Helpers;
using KvizCommando.Client.Models.ViewModels;
using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Client.Services.Visual.UiService.Language;
using KvizCommando.Shared.Models.Dtos;
using System;
using System.Collections.Generic;

namespace KvizCommando.Client.Features.Sologame
{
    public static class SgameBoxBuilder
    {
        public static readonly string[] Root = Enum.GetNames<SgameBoxKeyRoot>();
        public static readonly string[] SubCat = BuildNames(SgameBoxKeySub.BtnCat);
        public static readonly string[] SubOri = BuildNames(SgameBoxKeySub.BtnOri);

        public static Dictionary<string, ContentBoxVm> BuildBoxes(SoloGameDtos ss, string cult, ILanguageService lang)
        {
            var dict = new Dictionary<string, ContentBoxVm>();
            //var spec = new SgameBtnSpecs();

            foreach (var spec in SoloBoxSpecs.RootSpecs)
            {
                dict.Add(spec.Key.ToString(), new ContentBoxVm
                {
                    Header = lang[spec.TitleKey],
                    Footer = spec.BuildFooter(lang, ss.Results),
                    FooterDisplay = spec.FooterDisplay,
                    Size = spec.Size,
                    ImageSrc = spec.ImageSrc,
                    IsClickable = spec.BuildEnable(ss.Enables, 1),
                    IsEnabled = spec.BuildEnable(ss.Enables, 1),
                    ClickId = spec.ClickId
                });

            }
            foreach (var spec in SoloBoxSpecs.SubSpecs)
            {
                for (int i = 1; i <= spec.BtnQnty; i++)
                {
                    dict.Add($"{spec.Key.ToString()}{i}", new ContentBoxVm
                    {
                        Header = CategoryNameLocalizer.GetCategory(i, cult),
                        Footer = spec.BuildFooter(lang, ss.Results, i),
                        FooterDisplay = spec.FooterDisplay,
                        Size = spec.Size,
                        ImageSrc = spec.BuildImageSrc(i),
                        IsClickable = spec.BuildEnable(ss.Enables, i-1),
                        IsEnabled = spec.BuildEnable(ss.Enables, i-1),
                        ClickId = spec.ClickId + i
                    });

                } 
            }

            return dict;
        }
        private static string[] BuildNames(SgameBoxKeySub name)
        {
            var count = (int)name;
            string[] names = new string[count];
            for (int i = 0; i < count; i++)
            {
                names[i] = $"{name.ToString()}{i + 1}";
            }
            return names;
        }
    }

}
