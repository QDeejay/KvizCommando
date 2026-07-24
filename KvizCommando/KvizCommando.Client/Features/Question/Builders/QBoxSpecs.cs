using KvizCommando.Client.Helpers;
using KvizCommando.Client.Models.ViewModels;
using KvizCommando.Client.Features.Question.Components;
using KvizCommando.Client.Services.Visual.UiService.Language;
using KvizCommando.Shared.Models.Dtos;


namespace KvizCommando.Client.Features.Question.Builders
{


    public sealed class QBoxSpecs : VmSpecs
    {
        internal Enum Key { get; init; } = default!;
        internal Func<QuestionExtendedInfo, bool> CheckEnable { get; init; } = default!;
        internal Func<ILanguageService, QuestionExtendedInfo, string> BuildBoxText { get; init; } = default!;
    }

    public static class QuestionBoxSpecs
    {
        private const string IMAGE_ROOT = "images/buttons/question";

        public static readonly IReadOnlyList<QBoxSpecs> Specs =
        [
            new QBoxSpecs {
                Key = QBoxKeyRoot.RtBtnFactory,
                TitleKey = "question.Box.Title.FactorySlots",
                ImageSrc = $"{IMAGE_ROOT}/fact.webp",
                Size =ContentBoxSize.BUTTON_WIDE,
                FooterDisplay = true, ClickId = 101,
                BuildBoxText = (lang, qn) => lang["question.Box.Footer.FactorySlots"].FormatSafe(qn.NoFownQuestion),
                CheckEnable = (qn) => true,
                LcdBackground = false,
                RenderContent = 0,
                BodyComp = null
            },
            new QBoxSpecs {
                Key = QBoxKeyRoot.RtBtnUsr,
                TitleKey = "question.Box.Title.UsrSlots.NoData",
                ImageSrc =$"{IMAGE_ROOT}/usr.webp",
                Size = ContentBoxSize.BUTTON_WIDE,
                FooterDisplay = true, ClickId = 102,
                BuildBoxText =(lang, qn) => lang["question.Box.Footer.UsrSlots"].FormatSafe(qn.OccupiedUserSlot,qn.AvailableUserSlot),
                CheckEnable = (qn) => qn.AvailableUserSlot>0,
                LcdBackground = false,
                RenderContent = 0,
                BodyComp = null
            },
            new QBoxSpecs {
                Key = QBoxKeyRoot.RtBtnPendig,
                TitleKey = "question.Box.Title.PendingSlots.NoData",
                ImageSrc = $"{IMAGE_ROOT}/pending.webp",
                Size = ContentBoxSize.BUTTON_WIDE,
                FooterDisplay = true, ClickId = 103,
                BuildBoxText = (lang, qn) => lang["question.Box.Footer.PendingSlots"].FormatSafe(qn.HandlePendingSlot),
                CheckEnable = (qn) => qn.AvailablePendingSlot>0,
                LcdBackground = false,
                RenderContent = 0,
                BodyComp = null
            },
            new QBoxSpecs {
                Key = QBoxKeyRoot.RtBtnNew,
                TitleKey = "question.Modal.Title.New",
                ImageSrc = $"{IMAGE_ROOT}/new.webp",
                Size = ContentBoxSize.BUTTON_WIDE,
                FooterDisplay = true, ClickId = 104,
                BuildBoxText = (lang, qn) => lang["question.Box.Footer.New"].FormatSafe(qn.FreePendingSlot),
                CheckEnable = (qn) => qn.FreePendingSlot > 0,
                LcdBackground = false,
                RenderContent = 0,
                BodyComp = null
            },
            new QBoxSpecs {
                Key = QBoxKeyContent.FactSlots,
                TitleKey = "question.Box.Title.FactorySlots",
                ImageSrc = string.Empty,
                Size = ContentBoxSize.CONTENT_HALF_LARGE,
                FooterDisplay = false, ClickId = 0,
                BuildBoxText = (lang, qn) => "",
                CheckEnable = (qn) => true,
                LcdBackground = true,
                RenderContent = 1,
                BodyComp = typeof(FactorySlotsBase)
            },
            new QBoxSpecs {
                Key = QBoxKeyContent.UsrSlots,
                TitleKey = string.Empty,
                BuildBoxText = (lang, qn) => lang["question.Box.Title.UsrSlots"].FormatSafe(qn.OccupiedUserSlot, qn.AvailableUserSlot),
                ImageSrc = string.Empty,
                Size = ContentBoxSize.CONTENT_LARGE,
                FooterDisplay = false, ClickId = 0,
                CheckEnable = (qn) => true,
                LcdBackground = true,
                RenderContent = 1,
                BodyComp = typeof(UserSlotManager)

            },
            new QBoxSpecs {
                Key = QBoxKeyContent.PendigSlots,
                TitleKey = string.Empty,
                BuildBoxText = (lang, qn) =>  lang["question.Box.Title.PendingSlots"].FormatSafe(qn.OccupiedPendingSlot, qn.AvailableUserSlot >> 1),
                ImageSrc = string.Empty,
                 Size = ContentBoxSize.CONTENT_LARGE,
                FooterDisplay = false, ClickId = 0,
                CheckEnable = (qn) => true,
                LcdBackground = true,
                RenderContent = 1,
                BodyComp = typeof(PendingSlotManager)
            },
            new QBoxSpecs {
                Key = QBoxKeyContent.NewSlot,
                TitleKey = "question.Modal.Title.New",
                ImageSrc = string.Empty,
                 Size = ContentBoxSize.CONTENT_LARGE,
                FooterDisplay = false, ClickId = 0,
                BuildBoxText = (lang, qn) => "",
                CheckEnable = (qn) => true,
                LcdBackground = false,
                RenderContent = 1,
                BodyComp = typeof(NewQuestionManager)
             }
        ];
    }

}
