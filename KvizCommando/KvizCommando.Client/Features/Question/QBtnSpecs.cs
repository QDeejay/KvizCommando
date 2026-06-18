
using KvizCommando.Client.Features.Home;
using KvizCommando.Client.Helpers;
using KvizCommando.Client.Models.ViewModels;
using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Client.Services.Language;
using KvizCommando.Shared.Models.Dtos;
using System.Drawing;
using System.Globalization;
using System.Xml;


namespace KvizCommando.Client.Features.Question
{ 

   
    public sealed class QBoxSpecs : ButtonVm
    {
        internal Enum Key { get; init; } = default!;
        internal Func <QuestionExtendedInfo,bool> CheckEnable { get; init; } = default!;
        internal Func<ILanguageService, QuestionExtendedInfo, string> BuildBoxText { get; init; } = default!;
    }

    public static class QuestionButtonSpecs
    {

        public static readonly IReadOnlyList<QBoxSpecs> Specs = new[]
        {
            new QBoxSpecs {
                Key = QBoxKeyRoot.RtBtnFactory,
                TitleKey = "question.Box.Title.FactorySlots",
                ImageSrc = "images/solo/categories.webp", Size = "wide", FooterDisplay = true, ClickId = 101,
                BuildBoxText = (lang, qn) => lang["question.Box.Footer.FactorySlots"].FormatSafe(qn.NoFownQuestion),
                CheckEnable = (qn) => true,
                LcdBackground = false
                
            },
            new QBoxSpecs {
                Key = QBoxKeyRoot.RtBtnUsr,
                TitleKey = "question.Box.Title.UsrSlots.NoData",
                ImageSrc = "images/solo/orients.webp", Size = "wide", FooterDisplay = true, ClickId = 102,
                BuildBoxText =(lang, qn) => lang["question.Box.Footer.UsrSlots"].FormatSafe(qn.OccupiedUserSlot,qn.AvailableUserSlot),
                CheckEnable = (qn) => qn.AvailableUserSlot>0,
                LcdBackground = false
            },
            new QBoxSpecs {
                Key = QBoxKeyRoot.RtBtnPendig,
                TitleKey = "question.Box.Title.PendingSlots.NoData",
                ImageSrc = "images/solo/campaign.webp", Size = "wide", FooterDisplay = true, ClickId = 103,
                BuildBoxText = (lang, qn) => lang["question.Box.Footer.PendingSlots"].FormatSafe(qn.HandlePendingSlot),
                CheckEnable = (qn) => qn.AvailablePendingSlot>0,
                LcdBackground = false
            },
            new QBoxSpecs {
                Key = QBoxKeyRoot.RtBtnNew,
                TitleKey = "question.Modal.Title.New",
                ImageSrc = "images/solo/campaign.webp", Size = "wide", FooterDisplay = true, ClickId = 104,
                BuildBoxText = (lang, qn) => lang["question.Box.Footer.New"].FormatSafe(qn.FreePendingSlot),
                CheckEnable = (qn) => qn.FreePendingSlot > 0,
                LcdBackground = false
            },
            new QBoxSpecs {
                Key = QBoxKeyContent.FactSlots,
                TitleKey = "question.Box.Title.FactorySlots",
                ImageSrc = string.Empty, Size = "halflarge", FooterDisplay = false, ClickId = 1001,
                BuildBoxText = (lang, qn) => "",
                CheckEnable = (qn) => false,
                LcdBackground = true
            },
            new QBoxSpecs {
                Key = QBoxKeyContent.UsrSlots,
                TitleKey = string.Empty,
                BuildBoxText = (lang, qn) => lang["question.Box.Title.UsrSlots"].FormatSafe(qn.OccupiedUserSlot, qn.AvailableUserSlot),
                ImageSrc = string.Empty, Size = "large", FooterDisplay = false, ClickId = 1002,
                CheckEnable = (qn) => false,
                LcdBackground = true

            },
            new QBoxSpecs {
                Key = QBoxKeyContent.PendigSlots,
                TitleKey = string.Empty,
                BuildBoxText = (lang, qn) =>  lang["question.Box.Title.PendingSlots"].FormatSafe(qn.OccupiedPendingSlot, qn.AvailableUserSlot >> 1),
                ImageSrc = string.Empty, Size = "large", FooterDisplay = false, ClickId = 1003,
                CheckEnable = (qn) => false,
                LcdBackground = true
            },
            new QBoxSpecs {
                Key = QBoxKeyContent.NewSlot,
                TitleKey = "question.Modal.Title.New",
                ImageSrc = string.Empty, Size = "large", FooterDisplay = false, ClickId = 1004,
                BuildBoxText = (lang, qn) => "",
                CheckEnable = (qn) => false,
                LcdBackground = true
             }
        };
    }

}