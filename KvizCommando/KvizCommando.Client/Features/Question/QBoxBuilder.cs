using KvizCommando.Client.Features.Home;
using KvizCommando.Client.Helpers;
using KvizCommando.Client.Models.ViewModels;
using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Client.Services.Visual.UiService.Language;
using KvizCommando.Shared.Models.Dtos;
using System;

namespace KvizCommando.Client.Features.Question
{
    public static class QBoxBuilder
    {
        public static readonly string[] Root = Enum.GetNames<QBoxKeyRoot>();
        public static readonly string[] SubFact = [QBoxKeyContent.FactSlots.ToString()];
        public static readonly string[] SubUsr = [QBoxKeyContent.UsrSlots.ToString()];
        public static readonly string[] SubPend = [QBoxKeyContent.PendigSlots.ToString()];
        public static readonly string[] SubNew = [QBoxKeyContent.NewSlot.ToString()];

        public static Dictionary<string, ContentBoxVm> BuildBoxes(QuestionExtendedInfo qs, QuestionCallbacks qCallBacks , ILanguageService lang)
        {
            var dict = new Dictionary<string, ContentBoxVm>(QuestionBoxSpecs.Specs.Count);

            foreach (var spec in QuestionBoxSpecs.Specs)
            {
                var btn = spec;
                var footer = string.IsNullOrEmpty(spec.TitleKey);
                var bodyParams = spec.BodyComp is not null ? spec.BuildParams(qCallBacks) : [];

                dict[btn.Key.ToString()] = new ContentBoxVm
                {
                    Header = footer ? spec.BuildBoxText(lang, qs) : lang[spec.TitleKey],
                    Footer = !footer ? spec.BuildBoxText(lang, qs) : string.Empty,
                    FooterDisplay = spec.FooterDisplay,
                    Size = spec.Size,
                    ImageSrc = spec.ImageSrc,
                    IsClickable = spec.ClickId > 0 && btn.ClickId < 1000,
                    IsEnabled = btn.CheckEnable(qs),
                    ClickId = spec.ClickId,
                    LcdDisplay = spec.LcdBackground,
                    RenderContent = spec.RenderContent,
                    BodyComponent = spec.BodyComp,
                    BodyParameters= bodyParams
                };
            }
            
            return dict;
        }
       
    }
}
