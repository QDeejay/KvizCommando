using KvizCommando.Client.Features.Home;
using KvizCommando.Client.Helpers;
using KvizCommando.Client.Models.ViewModels;
using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Client.Services.Language;
using KvizCommando.Shared.Models.Dtos;
using System;

namespace KvizCommando.Client.Features.Question
{
    public static class QBtnBoxBuilder
    {


        public static Dictionary<string, ContentBoxVm> BuildBoxes(QuestionExtendedInfo qs, ILanguageService lang)
        {
            var dict = new Dictionary<string, ContentBoxVm>(QuestionButtonSpecs.Specs.Count);

            foreach (var spec in QuestionButtonSpecs.Specs)
            {
                var btn = spec;
                var footer = string.IsNullOrEmpty(spec.TitleKey);
                dict[btn.Key.ToString()] = new ContentBoxVm 
                {
                    Header = !footer ? spec.BuildBoxText(lang, qs) : lang[spec.TitleKey],
                    Footer = footer ?  spec.BuildBoxText(lang, qs) : string.Empty,
                    FooterDisplay = spec.FooterDisplay,
                    Size = spec.Size,
                    ImageSrc = spec.ImageSrc,
                    IsClickable = true,
                    IsEnabled = btn.CheckEnable(qs),
                    ClickId = spec.ClickId
                };
            }
            return dict;
        }
       
    }
}
