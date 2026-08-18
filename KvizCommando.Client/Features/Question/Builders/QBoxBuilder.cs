using KvizCommando.Client.Models.ViewModels;
using KvizCommando.Client.Services.Visual.UiService.Language;
using KvizCommando.Shared.Models.Dtos;

namespace KvizCommando.Client.Features.Question.Builders;

public static class QBoxBuilder
{
    public static readonly string[] Root = Enum.GetNames<QBoxKeyRoot>();
    public static readonly string[] SubFact = [QBoxKeyContent.FactSlots.ToString()];
    public static readonly string[] SubUsr = [QBoxKeyContent.UsrSlots.ToString()];
    public static readonly string[] SubPend = [QBoxKeyContent.PendigSlots.ToString()];
    public static readonly string[] SubNew = [QBoxKeyContent.NewSlot.ToString()];

    /// <summary>
    /// Összeállítja a képernyő tartalmi dobozait.
    /// </summary>
    public static Dictionary<string, ContentBoxVm> BuildBoxes(
        QuestionExtendedInfo questionInfo,
        ILanguageService lang)
    {
        var boxes = new Dictionary<string, ContentBoxVm>(
            QuestionBoxSpecs.Specs.Count);

        foreach (var spec in QuestionBoxSpecs.Specs)
        {
            var titleInFooter = string.IsNullOrEmpty(spec.TitleKey);

            boxes[spec.Key.ToString()] = new ContentBoxVm
            {
                Header = titleInFooter
                    ? spec.BuildBoxText(lang, questionInfo)
                    : lang[spec.TitleKey],
                Footer = titleInFooter
                    ? string.Empty
                    : spec.BuildBoxText(lang, questionInfo),
                FooterDisplay = spec.FooterDisplay,
                Size = spec.Size,
                ImageSrc = string.Empty,
                BgImageSrc = spec.ImageSrc,
                IsClickable = spec.CheckEnable(questionInfo) &&
                              spec.ClickId is > 0 and < 1000,
                IsEnabled = spec.CheckEnable(questionInfo),
                ClickId = spec.ClickId,
                LcdDisplay = spec.LcdBackground,
                RenderContent = spec.RenderContent,
                BodyComponent = spec.BodyComp
            };
        }

        return boxes;
    }
}
