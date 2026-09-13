using KvizCommando.Client.Models.ViewModels;
using KvizCommando.Localization.Question;
using KvizCommando.Shared.Models.Dtos;
using Microsoft.Extensions.Localization;

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
    /// <param name="questionInfo">A kérdésképernyő dobozainak forrásadata.</param>
    /// <param name="boxTitles">A közös boxcímek lokalizált értékei.</param>
    /// <param name="lang">A további feliratok feloldásához használt nyelvi szolgáltatás.</param>
    public static Dictionary<string, ContentBoxVm> BuildBoxes(
        QuestionExtendedInfo questionInfo,
        IReadOnlyDictionary<string, string> boxTitles,
        IStringLocalizer<QuestionResource> lang)
    {
        var boxes = new Dictionary<string, ContentBoxVm>(
            QuestionBoxSpecs.Specs.Count);

        foreach (var spec in QuestionBoxSpecs.Specs)
        {
            boxes[spec.Key.ToString()] = new ContentBoxVm
            {
                Header = spec.BuildHeader is null
                    ? boxTitles[spec.TitleKey]
                    : spec.BuildHeader(boxTitles, questionInfo),
                Footer = spec.FooterDisplay
                    ? spec.BuildBoxText(lang, questionInfo)
                    : string.Empty,
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
