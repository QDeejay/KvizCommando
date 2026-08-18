using KvizCommando.Client.Models.ViewModels;
using KvizCommando.Client.Services.Visual.UiService.Language;
using KvizCommando.Shared.Models.Dtos;

namespace KvizCommando.Client.Features.Team.Builders;

public static class TBoxBuilder
{
    public static readonly string[] Root =
        Enum.GetNames<TBoxKeyRoot>();
    public static readonly string[] SubTeam =
        [TBoxKeyContent.Team.ToString()];
    public static readonly string[] SubMember =
        [TBoxKeyContent.Member.ToString()];
    public static readonly string[] SubRecruit =
        [TBoxKeyContent.RecruitContent.ToString()];

    /// <summary>
    /// Összeállítja a képernyő tartalmi dobozait.
    /// </summary>
    public static Dictionary<string, ContentBoxVm> BuildBoxes(
        TeamRootBoxInfo rootInfo,
        TeamComponentParameters parameters,
        ILanguageService lang)
    {
        var boxes = new Dictionary<string, ContentBoxVm>(
            TeamBoxSpecs.Specs.Count);

        foreach (var spec in TeamBoxSpecs.Specs)
        {
            boxes.Add(spec.Key.ToString(), new ContentBoxVm
            {
                Header = lang[spec.TitleKey],
                Footer = spec.FooterDisplay
                    ? spec.BuildBoxText(lang, rootInfo)
                    : string.Empty,
                FooterDisplay = spec.FooterDisplay,
                Size = string.IsNullOrEmpty(spec.Size)
                    ? spec.SizeBuilder(rootInfo)
                    : spec.Size,
                BgImageSrc = spec.ImageSrc,
                ImageSrc = string.Empty,
                IsClickable = spec.CheckEnable(rootInfo) &&
                              spec.ClickId is > 0 and < 1000,
                IsEnabled = spec.CheckEnable(rootInfo),
                ClickId = spec.ClickId,
                LcdDisplay = spec.LcdBackground,
                RenderContent = spec.RenderContent,
                BodyComponent = spec.BodyComp,
                BodyParameters = spec.BodyComp is null
                    ? []
                    : spec.BuildParams(parameters)
            });
        }

        return boxes;
    }
}
