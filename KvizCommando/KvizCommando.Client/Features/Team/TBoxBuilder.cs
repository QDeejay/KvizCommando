using KvizCommando.Client.Features.Question;
using KvizCommando.Client.Features.Sologame;
using KvizCommando.Client.Features.Team;
using KvizCommando.Client.Models.ViewModels;
using KvizCommando.Client.Services.Visual.UiService.Language;
using KvizCommando.Shared.Models.Dtos;
using Microsoft.AspNetCore.Components.Routing;

namespace KvizCommando.Client.Features.Team
{
    public static class TBoxBuilder
    {
        public static readonly string[] Root = Enum.GetNames<TBoxKeyRoot>();
        public static readonly string[] SubTeam = [TBoxKeyContent.Team.ToString()];
        public static readonly string[] SubMember = [TBoxKeyContent.Member.ToString()];
        public static readonly string[] SubRecruit = [TBoxKeyContent.Recruit.ToString()];
        public static readonly string[] SubRecruitOri = SgameBoxBuilder.SubOri;
        public static Dictionary<string, ContentBoxVm> BuildBoxes(TeamRootBoxInfo rb, ILanguageService lang)
        {
            var dict = new Dictionary<string, ContentBoxVm>(TeamBoxSpecs.Specs.Count);

            foreach (var spec in TeamBoxSpecs.Specs)
            {
                var btn = spec;
                dict[btn.Key.ToString()] = new ContentBoxVm
                {
                    Header = lang[spec.TitleKey],
                    Footer = spec.FooterDisplay ? spec.BuildBoxText(lang, rb) : string.Empty,
                    FooterDisplay = spec.FooterDisplay,
                    Size = spec.Size,
                    ImageSrc = spec.ImageSrc,
                    IsClickable = spec.ClickId > 0 && btn.ClickId < 1000,
                    IsEnabled = btn.CheckEnable(rb),
                    ClickId = spec.ClickId,
                    LcdDisplay = spec.LcdBackground,
                    RenderContent = spec.RenderContent,
                    BodyComponent = spec.BodyComp,
                    //BodyParameters = bodyParams
                };
            }
           
            return dict;
        }
        public static Dictionary<string, ContentBoxVm> BuildRecruitSubBoxes(bool[] cm,CandidateDto[] cds, ILanguageService lang, string cult)
        {
            var dict = new Dictionary<string, ContentBoxVm>(TeamBoxSpecs.Specs.Count);

            var subbtn = TeamBoxSpecs.SubSpecs[0];
            for (int i = 1; i <= subbtn.BtnQty; i++)
            {
                if (!cm[i])
                    dict.Add($"{subbtn.Key.ToString()}{i}", new ContentBoxVm
                    {
                        Header = subbtn.BuildTitle(i, cult),
                        Footer = string.Empty,
                        FooterDisplay = false,
                        Size = subbtn.Size,
                        ImageSrc = subbtn.BuildImageSrc(i),
                        IsClickable = subbtn.CheckEnable(cds[i]),
                        IsEnabled = subbtn.CheckEnable(cds[i]),
                        ClickId = subbtn.ClickId + i,
                        DisableTextTp=lang[subbtn.DisableText],
                       
                    });
            }
            return dict;
        }
    }
}


