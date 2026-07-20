using KvizCommando.Client.Helpers;
using KvizCommando.Client.Models.ViewModels;
using KvizCommando.Client.Pages.Question.Dynamic;
using KvizCommando.Client.Services.Visual.UiService.Language;
using KvizCommando.Shared.Models.Dtos;


namespace KvizCommando.Client.Pages.Question.Features
{


    public sealed class QBoxSpecs : VmSpecs
    {
        internal Enum Key { get; init; } = default!;
        internal Func<QuestionExtendedInfo, bool> CheckEnable { get; init; } = default!;
        internal Func<ILanguageService, QuestionExtendedInfo, string> BuildBoxText { get; init; } = default!;
        internal Func<QuestionCallbacks, Dictionary<string, object?>> BuildParams { get; init; } = default!;
    }

    public static class QuestionBoxSpecs
    {

        public static readonly IReadOnlyList<QBoxSpecs> Specs =
        [
            new QBoxSpecs {
                Key = QBoxKeyRoot.RtBtnFactory,
                TitleKey = "question.Box.Title.FactorySlots",
                ImageSrc = "images/solo/categories.webp",
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
                ImageSrc = "images/solo/orients.webp",
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
                ImageSrc = "images/solo/campaign.webp",
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
                ImageSrc = "images/solo/campaign.webp",
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
                BodyComp = typeof(FactorySlotsBase),
                BuildParams = (cb) => new Dictionary<string, object?> {
                    { QParamNames.SaveSlots.ToString(), cb.OnFactSave }
                }
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
                BodyComp = typeof(UserSlotManager),
                BuildParams = (cb) => new Dictionary<string, object?> {
                    { QParamNames.SelectedIdChanged.ToString(), cb.OnSelectId },
                    { QParamNames.OnWatchButtonPushed.ToString(), cb.OnWatch },
                    { QParamNames.OnHandleButtonPushed.ToString(), cb.OnDelete }
                }

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
                BodyComp = typeof(PendingSlotManager),
                BuildParams = (cb) => new Dictionary<string, object?> {
                    { QParamNames.SelectedIdChanged.ToString(), cb.OnSelectId },
                    { QParamNames.OnHandleButtonPushed.ToString(), cb.OnHandle }
                }
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
                BodyComp = typeof(NewQuestionManager),
                BuildParams = (cb) => new Dictionary<string, object?> {
                    { QParamNames.OnSendQuestion.ToString(), cb.OnSend }
                }
             }
        ];
    }

}