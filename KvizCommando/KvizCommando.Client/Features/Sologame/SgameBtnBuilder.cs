using KvizCommando.Client.Features.Home;
using KvizCommando.Client.Features.Question;
using KvizCommando.Client.Helpers;
using KvizCommando.Client.Models.ViewModels;
using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Client.Services.Language;
using KvizCommando.Shared.Models.Dtos;
using System;
using System.Collections.Generic;

namespace KvizCommando.Client.Features.Sologame
{
    public static class SgameBtnBuilder
    {

        public static Dictionary<string, ContentBoxVm> BuildBoxes(SoloGameDtos ss, string cult, ILanguageService lang)
        {
            var dict = new Dictionary<string, ContentBoxVm>();
            //var spec = new SgameBtnSpecs();
            var mask = ss.ActiveOrients.Concat(ss.ActiveOrients).ToArray();
            var rootEna = i < 2 ? mask.Any(x => x) : false;
            Console.WriteLine($"mask:{mask.ToString()}");
          
            foreach (var spec in SoloButtonSpecs.RootSpecs)
            {
                
               
                dict.Add(spec.Key,new ContentBoxVm
                {
                    Header = lang[spec.TitleKey],
                    Footer = spec.BuildFooter(lang, ss),
                    FooterDisplay = spec.FooterDisplay,
                    Size = spec.Size,
                    ImageSrc = spec.ImageSrc,
                    IsClickable = ena,
                    IsEnabled = ena,
                    ClickId = spec.ClickId
                });

            }
            foreach (var spec in SoloButtonSpecs.ContentSpecs)
            {
                for (int i = 1; i < spec.BtnQnty; i++)
                {
                    dict.Add($"{spec.Key.ToString()}{i}", new ContentBoxVm
                    {
                        Header = CategoryNameLocalizer.GetCategory(i, cult),
                        Footer = spec.BuildFooter(lang, ss.CategoryResults![i]),
                        FooterDisplay = spec.FooterDisplay,
                        Size = spec.Size,
                        ImageSrc = spec.BuildImageSrc(i),
                        IsClickable = mask[i - 1],
                        IsEnabled = mask[i - 1],
                        ClickId = spec.ClickId + i
                    });

                }



            
            bool ena;
            for (int i = 0; i < 3; i++)
            {
                spec = SoloButtonSpecs.Specs[i]; 
                var points = i == 0 
                    ? ss.CategoryResults?.Sum(r => r?.Points ?? 0) ?? 0
                    : i == 0 
                    ? ss.CategoryResults?.Sum(r => r?.Points ?? 0) ?? 0 
                    : 0;
                ena = i < 2 ? mask.Any(x => x) : false;
                list.Add(new ContentBoxVm 
                { 
                    Header = lang[spec.TitleKey],
                    Footer = spec.BuildFooter(lang,new ResultDtos { Points = points}),
                    FooterDisplay = spec.FooterDisplay,
                    Size = spec.Size,
                    ImageSrc = spec.ImageSrc,
                    IsClickable = ena,
                    IsEnabled = ena,
                    ClickId =spec.ClickId
                });
            }
            




            return dict;
        }
       
    }
}
/*
 list.Add(new ContentBoxVm
                {
                    Header = CategoryNameLocalizer.GetCategory(i, cult),
                    Footer = spec.BuildFooter(lang, ss.CategoryResults[i]),
                    FooterDisplay = spec.FooterDisplay,
                    Size = spec.Size,
                    ImageSrc = spec.BuildImageSrc(i),
                    IsClickable = mask[i - 1],
                    IsEnabled = mask[i - 1],
                    ClickId = spec.ClickId + i
                });
 
 spec = SoloButtonSpecs.Specs[3];  // a harmadik elem a kategóriákhoz tartozik
            for (int i = 1; i < 17; i++)
            {
                dict.Add($"cat{i}", new ContentBoxVm
                {
                    Header = CategoryNameLocalizer.GetCategory(i, cult),
                    Footer = spec.BuildFooter(lang, ss.CategoryResults[i]),
                    FooterDisplay = spec.FooterDisplay,
                    Size = spec.Size,
                    ImageSrc = spec.BuildImageSrc(i),
                    IsClickable = mask[i - 1],
                    IsEnabled = mask[i - 1],
                    ClickId = spec.ClickId + i
                });
                
            }
            mask = ss.ActiveOrients;
            spec = SoloButtonSpecs.Specs[4]; // a negyedik elem az orientációkhoz tartozik
            for (int i = 1; i < 9; i++)
            {
                list.Add(new ContentBoxVm
                {
                    Header = OrientationLocalizer.GetOrientation(i, cult),
                    Footer = spec.BuildFooter(lang, ss.OrientResults[i]),
                    FooterDisplay = spec.FooterDisplay,
                    Size = spec.Size,
                    ImageSrc = spec.BuildImageSrc(i),
                    IsClickable = mask[i - 1],
                    IsEnabled = mask[i - 1],
                    ClickId = spec.ClickId + i
                });
            }
spec = SoloButtonSpecs.Specs[i];
                var points = i == 0
                    ? ss.CategoryResults?.Sum(r => r?.Points ?? 0) ?? 0
                    : i == 0
                    ? ss.CategoryResults?.Sum(r => r?.Points ?? 0) ?? 0
                    : 0;
 */