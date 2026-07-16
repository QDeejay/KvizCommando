using KvizCommando.Client.Models.ViewModels;
using KvizCommando.Client.Services.Visual.UiService.Language;
using KvizCommando.Shared.Models.Dtos;

namespace KvizCommando.Client.Pages.Team.Features
{
    public static class TBoxBuilder
    {
        public static readonly string[] Root = Enum.GetNames<TBoxKeyRoot>();
        public static readonly string[] SubTeam = [TBoxKeyContent.Team.ToString()];
        public static readonly string[] SubMember = [TBoxKeyContent.Member.ToString()];
        public static readonly string[] SubRecruit = [TBoxKeyContent.Recruit.ToString()];
        public static Dictionary<string, ContentBoxVm> BuildBoxes(TeamRootBoxInfo rb, TeamCallbacks tCallBacks, ILanguageService lang)
        {
            var dict = new Dictionary<string, ContentBoxVm>(TeamBoxSpecs.Specs.Count);

            foreach (var spec in TeamBoxSpecs.Specs)
            {
                var btn = spec;
                var bodyParams = spec.BodyComp is not null ? spec.BuildParams(tCallBacks) : [];
                dict[btn.Key.ToString()] = new ContentBoxVm
                {
                    Header = lang[spec.TitleKey],
                    Footer = spec.FooterDisplay ? spec.BuildBoxText(lang, rb) : string.Empty,
                    FooterDisplay = spec.FooterDisplay,
                    Size = string.IsNullOrEmpty(spec.Size) ? spec.SizeBuilder(rb) : spec.Size,
                    ImageSrc = spec.ImageSrc,
                    IsClickable = btn.CheckEnable(rb) && spec.ClickId > 0 && spec.ClickId < 1000,
                    IsEnabled = btn.CheckEnable(rb),
                    ClickId = spec.ClickId,
                    LcdDisplay = spec.LcdBackground,
                    RenderContent = spec.RenderContent,
                    BodyComponent = spec.BodyComp,
                    BodyParameters = bodyParams
                };
            }

            return dict;
        }
    }

}