using KvizCommando.Client.Features.Question.Components;
using KvizCommando.Client.Helpers;
using KvizCommando.Client.Models.ViewModels;
using KvizCommando.Client.Services.Visual.UiService.Language;
using KvizCommando.Shared.Models.Dtos;


namespace KvizCommando.Client.Features.Question.Builders
{


    public sealed class QBoxSpecs : VmSpecs
    {
        internal Enum Key { get; init; } = default!;
        internal Func<QuestionExtendedInfo, bool> CheckEnable
        { get; init; } = _ => true;
        internal Func<ILanguageService, QuestionExtendedInfo, string> BuildBoxText
        { get; init; } = (_, _) => string.Empty;
    }

    public static class QuestionBoxSpecs
    {
        private const string IMAGE_ROOT = "images/buttons/question";

        public static readonly IReadOnlyList<QBoxSpecs> Specs =
        [
            new QBoxSpecs {
                Key = QBoxKeyRoot.Factory,
                TitleKey = "home.SubBox.Title.Question.Factory",
                ImageSrc = $"{IMAGE_ROOT}/fact.webp",
                Size =ContentBoxSize.BUTTON_WIDE,
                FooterDisplay = true, ClickId = (int)QBoxKeyRoot.Factory,
                BuildBoxText = (lang, qn) => lang["question.Box.Footer.FactorySlots"].FormatSafe(qn.NoFownQuestion),
                CheckEnable = (qn) =>
                    qn.CharCatMask.Any(hasCharacter => hasCharacter),
                LcdBackground = false,
                RenderContent = 0,
                BodyComp = null
            },
            new QBoxSpecs {
                Key = QBoxKeyRoot.Usr,
                TitleKey = "home.SubBox.Title.Question.Usr",
                ImageSrc =$"{IMAGE_ROOT}/usr.webp",
                Size = ContentBoxSize.BUTTON_WIDE,
                FooterDisplay = true, ClickId = (int)QBoxKeyRoot.Usr,
                BuildBoxText =(lang, qn) => lang["question.Box.Footer.UsrSlots"].FormatSafe(qn.OccupiedUserSlot,qn.AvailableUserSlot),
                CheckEnable = (qn) => qn.AvailableUserSlot>0,
                LcdBackground = false,
                RenderContent = 0,
                BodyComp = null
            },
            new QBoxSpecs {
                Key = QBoxKeyRoot.Pending,
                TitleKey = "home.SubBox.Title.Question.Pending",
                ImageSrc = $"{IMAGE_ROOT}/pending.webp",
                Size = ContentBoxSize.BUTTON_WIDE,
                FooterDisplay = true, ClickId = (int)QBoxKeyRoot.Pending,
                BuildBoxText = (lang, qn) => lang["question.Box.Footer.PendingSlots"].FormatSafe(qn.HandlePendingSlot),
                CheckEnable = (qn) => qn.AvailablePendingSlot>0,
                LcdBackground = false,
                RenderContent = 0,
                BodyComp = null
            },
            new QBoxSpecs {
                Key = QBoxKeyRoot.New,
                TitleKey = "home.SubBox.Title.Question.New",
                ImageSrc = $"{IMAGE_ROOT}/new.webp",
                Size = ContentBoxSize.BUTTON_WIDE,
                FooterDisplay = true, ClickId = (int)QBoxKeyRoot.New,
                BuildBoxText = (lang, qn) => lang["question.Box.Footer.New"].FormatSafe(qn.FreePendingSlot),
                CheckEnable = (qn) =>
                    qn.FreePendingSlot > 0 &&
                    qn.CharCatMask.Any(hasCharacter => hasCharacter),
                LcdBackground = false,
                RenderContent = 0,
                BodyComp = null
            },
            new QBoxSpecs {
                Key = QBoxKeyContent.FactSlots,
                TitleKey = "home.SubBox.Title.Question.Factory",
                ImageSrc = string.Empty,
                Size = ContentBoxSize.CONTENT_HALF_LARGE,
                FooterDisplay = false, ClickId = 0,
                LcdBackground = true,
                RenderContent = 1,
                BodyComp = typeof(FactorySlotsBase)
            },
            new QBoxSpecs {
                Key = QBoxKeyContent.UsrSlots,
                TitleKey = string.Empty,
                BuildBoxText = (lang, qn) => $"{lang["home.SubBox.Title.Question.Usr"]} ({qn.OccupiedUserSlot}/{qn.AvailableUserSlot})",
                ImageSrc = string.Empty,
                Size = ContentBoxSize.CONTENT_LARGE,
                FooterDisplay = false, ClickId = 0,
                LcdBackground = true,
                RenderContent = 1,
                BodyComp = typeof(UserSlotManager)

            },
            new QBoxSpecs {
                Key = QBoxKeyContent.PendigSlots,
                TitleKey = string.Empty,
                BuildBoxText = (lang, qn) => $"{lang["home.SubBox.Title.Question.Pending"]} ({qn.OccupiedPendingSlot}/{qn.AvailablePendingSlot})",
                ImageSrc = string.Empty,
                 Size = ContentBoxSize.CONTENT_LARGE,
                FooterDisplay = false, ClickId = 0,
                LcdBackground = true,
                RenderContent = 1,
                BodyComp = typeof(PendingSlotManager)
            },
            new QBoxSpecs {
                Key = QBoxKeyContent.NewSlot,
                TitleKey = "home.SubBox.Title.Question.New",
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
