using KvizCommando.Client.Helpers;
using KvizCommando.Client.Models.ViewModels;
using KvizCommando.Client.Pages.Team.Dynamic;
using KvizCommando.Client.Services.Visual.UiService.Language;
using KvizCommando.Shared.Models.Dtos;

namespace KvizCommando.Client.Pages.Team.Features
{
    public class TBoxSpecs : VmSpecs
    {
        internal Enum Key { get; init; } = default!;
        internal Func<TeamRootBoxInfo, string> SizeBuilder { get; set; } = default!;
        internal Func<TeamRootBoxInfo, bool> CheckEnable { get; init; } = default!;
        internal Func<ILanguageService, TeamRootBoxInfo, string> BuildBoxText { get; init; } = default!;
        internal Func<TeamCallbacks, Dictionary<string, object?>> BuildParams { get; init; } = default!;
    }

    public static class TeamBoxSpecs
    {
        public static readonly IReadOnlyList<TBoxSpecs> Specs =
        [
            new TBoxSpecs {
                Key = TBoxKeyRoot.RtBtnTeam,
                TitleKey = "team.Box.Title.TeamOverview",
                ImageSrc = "images/solo/categories.webp",
                Size = ContentBoxSize.BUTTON_WIDE,
                FooterDisplay = true, ClickId = 201,
                BuildBoxText = (lang, rb) => lang["team.Box.Footer.Team"].FormatSafe(rb.TeamOpRequired),
                CheckEnable = (rb) => rb.IsTeamEnable,
                LcdBackground = false,
                RenderContent = 0,
                BodyComp = null
            },
            new TBoxSpecs {
                Key = TBoxKeyRoot.RtBtnMembers,
                TitleKey = "team.Box.Title.Members",
                ImageSrc = "images/solo/categories.webp",
                Size = ContentBoxSize.BUTTON_WIDE,
                FooterDisplay = true, ClickId = 202,
                BuildBoxText = (lang, rb) => lang["team.Box.Footer.Member"].FormatSafe(rb.MemberOpRequired),
                CheckEnable = (rb) => rb.IsMemberEnable,
                LcdBackground = false,
                RenderContent = 0,
                BodyComp = null
            },
            new TBoxSpecs {
                Key = TBoxKeyRoot.RtBtnRecruit,
                TitleKey = "team.Box.Title.Recruit",
                ImageSrc = "images/solo/categories.webp",
                Size = ContentBoxSize.BUTTON_WIDE,
                FooterDisplay = true, ClickId = 203,
                BuildBoxText = (lang, rb) => lang["team.Box.Footer.Recruit"].FormatSafe(rb.FreePositions),
                CheckEnable = (rb) => rb.IsRecruitEnable,
                LcdBackground = false,
                RenderContent = 0,
                BodyComp = null
            },

            new TBoxSpecs {
                Key = TBoxKeyContent.Team,
                TitleKey = "team.Box.Title.TeamOverview",
                ImageSrc = string.Empty,
                 Size = ContentBoxSize.CONTENT_LARGE,
                FooterDisplay = false, ClickId = 0,
                BuildBoxText = (lang, rb) => "",
                CheckEnable = (rb) => true,
                LcdBackground = true,
                RenderContent = 1,
                BodyComp= typeof(TeamManager),
                BuildParams = (cb) => new Dictionary<string, object?> {
                    { TParamNames.OnManagePushed.ToString(), cb.OnManage },
                    { TParamNames.OnModifySkillPushed.ToString(), cb.OnModify }
                }
             },
            new TBoxSpecs {
                Key = TBoxKeyContent.Member,
                TitleKey = "team.Box.Title.Members",
                ImageSrc = string.Empty,
                Size = ContentBoxSize.CONTENT_LARGE,
                FooterDisplay = false, ClickId = 0,
                BuildBoxText = (lang, rb) => "",
                CheckEnable = (rb) => true,
                LcdBackground = true,
                RenderContent = 1,
                BodyComp= typeof(MemberManager),
                BuildParams = (cb) => new Dictionary<string, object?> {
                    { TParamNames.OnModifySkillPushed.ToString(), cb.OnModify }
                 }
                },
            new TBoxSpecs {
                Key = TBoxKeyContent.Recruit,
                TitleKey = "team.Box.Title.Recruit",
                ImageSrc = string.Empty, Size = string.Empty, FooterDisplay = false, ClickId = 0,
                BuildBoxText = (lang, rb) => "",
                CheckEnable = (rb) => true,
                LcdBackground = false,
                RenderContent = 1,
                SizeBuilder = (rb) => rb.AbleToHire==0 ? ContentBoxSize.CONTENT_CLOSED_LARGE : ContentBoxSize.CONTENT_EXTRA_LARGE,
                BodyComp = typeof(RecruitManager),
                BuildParams = (cb) => new Dictionary<string, object?> {
                    { TParamNames.OnCandidateHired.ToString(), cb.OnHire },
                    { TParamNames.CandidateOrder.ToString(), cb.OnShuffledIds}
                }
            }
        ];
    }
}







/*
            new QBoxSpecs {
                Key = QBoxKeyRoot.RtBtnUsr,
                TitleKey = "question.Box.Title.UsrSlots.NoData",
                ImageSrc = "images/solo/orients.webp", Size = "wide", FooterDisplay = true, ClickId = 102,
                BuildBoxText =(lang, qn) => lang["question.Box.Footer.UsrSlots"].FormatSafe(qn.OccupiedUserSlot,qn.AvailableUserSlot),
                CheckEnable = (qn) => qn.AvailableUserSlot>0,
                LcdBackground = false,
                RenderContent = 0,
                BodyComp = null
            },
            new QBoxSpecs {
                Key = QBoxKeyRoot.RtBtnPendig,
                TitleKey = "question.Box.Title.PendingSlots.NoData",
                ImageSrc = "images/solo/campaign.webp", Size = "wide", FooterDisplay = true, ClickId = 103,
                BuildBoxText = (lang, qn) => lang["question.Box.Footer.PendingSlots"].FormatSafe(qn.HandlePendingSlot),
                CheckEnable = (qn) => qn.AvailablePendingSlot>0,
                LcdBackground = false,
                RenderContent = 0,
                BodyComp = null
            },
            new QBoxSpecs {
                Key = QBoxKeyContent.FactSlots,
                TitleKey = "question.Box.Title.FactorySlots",
                ImageSrc = string.Empty, Size = "halflarge", FooterDisplay = false, ClickId = 0,
                BuildBoxText = (lang, qn) => "",
                CheckEnable = (qn) => true,
                LcdBackground = true,
                RenderContent = 1,
                BodyComp = typeof(FactorySlotsBase),
                BuildParams = (cb) => new Dictionary<string, object?> {
                    { ParamNames.SaveSlots.ToString(), cb.OnFactSave }
                }
            },
            new QBoxSpecs {
                Key = QBoxKeyContent.UsrSlots,
                TitleKey = string.Empty,
                BuildBoxText = (lang, qn) => lang["question.Box.Title.UsrSlots"].FormatSafe(qn.OccupiedUserSlot, qn.AvailableUserSlot),
                ImageSrc = string.Empty, Size = "large", FooterDisplay = false, ClickId = 0,
                CheckEnable = (qn) => true,
                LcdBackground = true,
                RenderContent = 1,
                BodyComp = typeof(UserSlotManager),
                BuildParams = (cb) => new Dictionary<string, object?> {
                    { ParamNames.SelectedIdChanged.ToString(), cb.OnSelectId },
                    { ParamNames.OnWatchButtonPushed.ToString(), cb.OnWatch },
                    { ParamNames.OnHandleButtonPushed.ToString(), cb.OnDelete }
                }

            },
            new QBoxSpecs {
                Key = QBoxKeyContent.PendigSlots,
                TitleKey = string.Empty,
                BuildBoxText = (lang, qn) =>  lang["question.Box.Title.PendingSlots"].FormatSafe(qn.OccupiedPendingSlot, qn.AvailableUserSlot >> 1),
                ImageSrc = string.Empty, Size = "large", FooterDisplay = false, ClickId = 0,
                CheckEnable = (qn) => true,
                LcdBackground = true,
                RenderContent = 1,
                BodyComp = typeof(PendingSlotManager),
                BuildParams = (cb) => new Dictionary<string, object?> {
                    { ParamNames.SelectedIdChanged.ToString(), cb.OnSelectId },
                    { ParamNames.OnHandleButtonPushed.ToString(), cb.OnHandle }
                }
            }*/