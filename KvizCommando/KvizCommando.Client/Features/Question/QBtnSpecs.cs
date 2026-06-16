
using KvizCommando.Client.Features.Home;
using KvizCommando.Client.Helpers;
using KvizCommando.Client.Models.ViewModels;
using KvizCommando.Client.Services.Language;
using KvizCommando.Shared.Models.Dtos;
using System.Globalization;
using System.Xml;


namespace KvizCommando.Client.Features.Question
{ 

   
    public sealed class QBtnSpec : ButtonVm
    {
       
        public Func <QuestionExtendedInfo,bool> CheckEnable { get; init; } = default!;
        public Func<ILanguageService, QuestionExtendedInfo, string> BuildFooter { get; init; } = default!;
    }

    public static class QuestionButtonSpecs
    {

        public static readonly IReadOnlyList<QBtnSpec> Specs = new[]
        {
            new QBtnSpec {
                TitleKey = "question.Box.Title.FactorySlots",
                ImageSrc = "images/solo/categories.webp", Size = "wide", FooterDisplay = true, ClickId = 11,
                BuildFooter = (lang, qn) => lang["question.Box.Footer.FactorySlots"].FormatSafe(qn.NoFownQuestion),
                CheckEnable = (qn) => true
            },
            new QBtnSpec {
                TitleKey = "question.Box.Title.UsrSlots.NoData",
                ImageSrc = "images/solo/orients.webp", Size = "wide", FooterDisplay = true, ClickId = 12,
                BuildFooter =(lang, qn) => lang["question.Box.Footer.UsrSlots"].FormatSafe(qn.OccupiedUserSlot,qn.AvailableUserSlot),
                CheckEnable = (qn) => qn.AvailableUserSlot>0
            },
            new QBtnSpec {
                TitleKey = "question.Box.Title.PendingSlots.NoData",
                ImageSrc = "images/solo/campaign.webp", Size = "wide", FooterDisplay = true, ClickId = 13,
                BuildFooter = (lang, qn) => lang["question.Box.Footer.PendingSlots"].FormatSafe(qn.HandlePendingSlot),
                CheckEnable = (qn) => qn.AvailablePendingSlot>0
            },
            new QBtnSpec {
                TitleKey = "question.Modal.Title.New",
                ImageSrc = "images/solo/campaign.webp", Size = "wide", FooterDisplay = true, ClickId = 14,
                BuildFooter = (lang, qn) => lang["question.Box.Footer.New"].FormatSafe(qn.FreePendingSlot),
                CheckEnable = (qn) => qn.FreePendingSlot > 0
            }


        };
    }

}