using KvizCommando.Client.Features.Question.Components;
using KvizCommando.Client.Models.ViewModels;
using KvizCommando.Localization.Question;
using KvizCommando.Shared.Models.Dtos;
using Microsoft.Extensions.Localization;


namespace KvizCommando.Client.Features.Question.Builders
{


    public sealed class QBoxSpecs : VmSpecs
    {
        internal Enum Key { get; init; } = default!;
        internal Func<QuestionExtendedInfo, bool> CheckEnable
        { get; init; } = _ => true;
        internal Func<IStringLocalizer<QuestionResource>, QuestionExtendedInfo, string> BuildBoxText
        { get; init; } = (_, _) => string.Empty;
        internal Func<IReadOnlyDictionary<string, string>, QuestionExtendedInfo, string>? BuildHeader
        { get; init; }
    }

    public static class QuestionBoxSpecs
    {
        private const string IMAGE_ROOT = "images/buttons/question";

        public static readonly IReadOnlyList<QBoxSpecs> Specs =
        [
            new QBoxSpecs {
                Key = QBoxKeyRoot.Factory,
                TitleKey = "Question.Factory",
                ImageSrc = $"{IMAGE_ROOT}/fact.webp",
                Size =ContentBoxSize.BUTTON_WIDE,
                FooterDisplay = true, ClickId = (int)QBoxKeyRoot.Factory,
                BuildBoxText = (lang, qn) => lang["question.Box.Footer.FactorySlots", qn.NoFownQuestion],
                CheckEnable = (qn) =>
                    qn.CharCatMask.Any(hasCharacter => hasCharacter),
                LcdBackground = false,
                RenderContent = 0,
                BodyComp = null
            },
            new QBoxSpecs {
                Key = QBoxKeyRoot.Usr,
                TitleKey = "Question.Usr",
                ImageSrc =$"{IMAGE_ROOT}/usr.webp",
                Size = ContentBoxSize.BUTTON_WIDE,
                FooterDisplay = true, ClickId = (int)QBoxKeyRoot.Usr,
                BuildBoxText =(lang, qn) => lang["question.Box.Footer.UsrSlots", qn.OccupiedUserSlot, qn.AvailableUserSlot],
                CheckEnable = (qn) => qn.AvailableUserSlot>0,
                LcdBackground = false,
                RenderContent = 0,
                BodyComp = null
            },
            new QBoxSpecs {
                Key = QBoxKeyRoot.Pending,
                TitleKey = "Question.Pending",
                ImageSrc = $"{IMAGE_ROOT}/pending.webp",
                Size = ContentBoxSize.BUTTON_WIDE,
                FooterDisplay = true, ClickId = (int)QBoxKeyRoot.Pending,
                BuildBoxText = (lang, qn) => lang["question.Box.Footer.PendingSlots", qn.HandlePendingSlot],
                CheckEnable = (qn) => qn.AvailablePendingSlot>0,
                LcdBackground = false,
                RenderContent = 0,
                BodyComp = null
            },
            new QBoxSpecs {
                Key = QBoxKeyRoot.New,
                TitleKey = "Question.New",
                ImageSrc = $"{IMAGE_ROOT}/new.webp",
                Size = ContentBoxSize.BUTTON_WIDE,
                FooterDisplay = true, ClickId = (int)QBoxKeyRoot.New,
                BuildBoxText = (lang, qn) => lang["question.Box.Footer.New", qn.FreePendingSlot],
                CheckEnable = (qn) =>
                    qn.FreePendingSlot > 0 &&
                    qn.CharCatMask.Any(hasCharacter => hasCharacter),
                LcdBackground = false,
                RenderContent = 0,
                BodyComp = null
            },
            new QBoxSpecs {
                Key = QBoxKeyContent.FactSlots,
                TitleKey = "Question.Factory",
                ImageSrc = string.Empty,
                Size = ContentBoxSize.CONTENT_HALF_LARGE,
                FooterDisplay = false, ClickId = 0,
                LcdBackground = true,
                RenderContent = 1,
                BodyComp = typeof(FactorySlotsBase)
            },
            new QBoxSpecs {
                Key = QBoxKeyContent.UsrSlots,
                TitleKey = "Question.Usr",
                BuildHeader = (boxTitles, qn) =>
                    $"{boxTitles["Question.Usr"]} ({qn.OccupiedUserSlot}/{qn.AvailableUserSlot})",
                ImageSrc = string.Empty,
                Size = ContentBoxSize.CONTENT_LARGE,
                FooterDisplay = false, ClickId = 0,
                LcdBackground = true,
                RenderContent = 1,
                BodyComp = typeof(UserSlotManager)

            },
            new QBoxSpecs {
                Key = QBoxKeyContent.PendigSlots,
                TitleKey = "Question.Pending",
                BuildHeader = (boxTitles, qn) =>
                    $"{boxTitles["Question.Pending"]} ({qn.OccupiedPendingSlot}/{qn.AvailablePendingSlot})",
                ImageSrc = string.Empty,
                 Size = ContentBoxSize.CONTENT_LARGE,
                FooterDisplay = false, ClickId = 0,
                LcdBackground = true,
                RenderContent = 1,
                BodyComp = typeof(PendingSlotManager)
            },
            new QBoxSpecs {
                Key = QBoxKeyContent.NewSlot,
                TitleKey = "Question.New",
                ImageSrc = string.Empty,
                 Size = ContentBoxSize.CONTENT_LARGE,
                FooterDisplay = false, ClickId = 0,
                LcdBackground = false,
                RenderContent = 1,
                BodyComp = typeof(NewQuestionManager)
             }
        ];
    }

}
