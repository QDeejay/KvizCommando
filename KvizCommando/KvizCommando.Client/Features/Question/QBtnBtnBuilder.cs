using KvizCommando.Client.Features.Home;
using KvizCommando.Client.Helpers;
using KvizCommando.Client.Models.ViewModels;
using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Client.Services.Language;
using KvizCommando.Shared.Models.Dtos;
using System;

namespace KvizCommando.Client.Features.Question
{
    public static class QBtnBtnBuilder
    {

        public static List<ContentBoxVm> BuildButtons(QuestionExtendedInfo qs, ILanguageService lang)
        {
            var list = new List<ContentBoxVm>(QuestionButtonSpecs.Specs.Count);

            foreach (var spec in QuestionButtonSpecs.Specs)
            {
                var btn = spec; 
                list.Add(new ContentBoxVm
                {
                    Header = lang[spec.TitleKey],
                    Footer = spec.BuildFooter(lang, qs),
                    FooterDisplay = spec.FooterDisplay,
                    Size = spec.Size,
                    ImageSrc = spec.ImageSrc,
                    IsClickable = true,
                    IsEnabled = btn.CheckEnable(qs),
                    ClickId = spec.ClickId
                });
            }
            return list;
        }
       
    }
}
