using KvizCommando.Client.Components.Dynamic;
using KvizCommando.Client.Models.ViewModels;
using KvizCommando.Client.Pages.Question.Components;
using KvizCommando.Client.Services.Visual.UiService.Language;
using KvizCommando.Shared.Models.Dtos;

namespace KvizCommando.Client.Features.Modal
{
    public sealed class MboxSpecs : ModalVm
    {
        //internal Func<ILanguageService, string> BuidText { get; init; } = default!;
    }


    public static class ModalBox
    {
        
        public static readonly IReadOnlyDictionary<ModalTypes, MboxSpecs> Specs =

            new Dictionary<ModalTypes, MboxSpecs>
            { 
                [ModalTypes.Terms] = new MboxSpecs {
                    TitleKey = "PLACEHOLDER",
                    TextKey1 = "checkin.modal.Button.Action",
                    TextKey2 = string.Empty,
                    CloseTextKey = string.Empty,
                    Style1 = string.Empty,
                    Style2 = string.Empty,
                    Size = "lg",
                    CheckBoxTextKey = string.Empty,
                    CheckBoxKey = string.Empty,
                    CheckBottom = true,
                    BodyComponent = null
                },

                [ModalTypes.LangConfirm] = new MboxSpecs {
                },

                [ModalTypes.QUsrDelet] = new MboxSpecs {
                    TitleKey = "question.Modal.Title.Confirm",
                    TextKey1 = "question.Button.Delete",
                    TextKey2 = string.Empty,
                    CloseTextKey = "question.Button.Close",
                    Style1 = "#a64b2a",
                    Style2 = string.Empty,
                    Size = "lg",
                    CheckBoxTextKey = "mainlayout.CheckBox.NotShow",
                    CheckBoxKey = ModalConst.LOCAL_NOT_SHOW_DEL,
                    CheckBottom = false,
                    BodyComponent = typeof(QModalRender)
                },

                [ModalTypes.QPendHandle] = new MboxSpecs {
                    TitleKey = "question.Modal.Title.Handling",
                    TextKey1 = "question.Button.Delete",
                    TextKey2 = "question.Button.Move",
                    CloseTextKey = "question.Button.Close",
                    Style1 = "#a64b2a",
                    Style2 = string.Empty,
                    Size = "lg",
                    CheckBoxTextKey = string.Empty,
                    CheckBoxKey = string.Empty,
                    CheckBottom = true,
                    BodyComponent = typeof(QModalRender)
                },
                [ModalTypes.QCheckQuestion] = new MboxSpecs
                {
                    TitleKey = "question.Modal.Title.CheckQuestion",
                    TextKey1 = string.Empty,
                    TextKey2 = string.Empty,
                    CloseTextKey = "question.Button.Close",
                    Style1 = string.Empty,
                    Style2 = string.Empty,
                    Size = "lg",
                    CheckBoxTextKey = string.Empty,
                    CheckBoxKey = string.Empty,
                    CheckBottom = true,
                    BodyComponent = typeof(QModalRender)
                },

                [ModalTypes.QNewRules] = new MboxSpecs {
                    TitleKey = "question.Modal.Title.New",
                    TextKey1 = "question.Button.Send",
                    TextKey2 = string.Empty,
                    CloseTextKey = "question.Button.Close",
                    Style1 = "#4b5320",
                    Style2 = string.Empty,
                    Size = "lg",
                    CheckBoxTextKey = "mainlayout.CheckBox.NotShow",
                    CheckBoxKey = ModalConst.LOCAL_NOT_SHOW_NEW,
                    CheckBottom = true,
                    BodyComponent = typeof(QModalRender)
                },

                [ModalTypes.THire] = new MboxSpecs {
                    TitleKey = "team.modal.Title.Hire",
                    TextKey1 = "team.modal.Button.Hire",
                    TextKey2 = string.Empty,
                    CloseTextKey = "team.modal.Button.Cancel",
                    Style1 = "#4b5320",
                    Style2 = string.Empty,
                    Size = "lg",
                    CheckBoxTextKey = string.Empty,
                    CheckBoxKey = string.Empty,
                    CheckBottom = false,
                    BodyComponent = typeof(TModalRender)
                },

                [ModalTypes.TPromote] = new MboxSpecs {
                    TitleKey = "team.modal.Title.Promote",
                    TextKey1 = "team.modal.Button.Promote",
                    TextKey2 = string.Empty,
                    CloseTextKey = "team.modal.Button.Cancel",
                    Style1 = "#4b5320",
                    Style2 = string.Empty,
                    Size = "lg",
                    CheckBoxTextKey = string.Empty,
                    CheckBoxKey = string.Empty,
                    CheckBottom = true,
                    BodyComponent = typeof(TModalRender)
                },

                [ModalTypes.TRetire] = new MboxSpecs {
                    TitleKey = "team.modal.Title.Retire",
                    TextKey1 = "team.modal.Button.Retire",
                    TextKey2 = string.Empty,
                    CloseTextKey = "team.modal.Button.Cancel",
                    Style1 = "#4b5320",
                    Style2 = string.Empty,
                    Size = "lg",
                    CheckBoxTextKey = string.Empty,
                    CheckBoxKey = string.Empty,
                    CheckBottom = false,
                    BodyComponent = typeof(TModalRender)
                },

                [ModalTypes.THandle] = new MboxSpecs {
                    TitleKey = "team.modal.Title.Handle",
                    TextKey1 = "team.modal.Button.Fire",
                    TextKey2 = "team.modal.Button.Heal",
                    CloseTextKey = "team.modal.Button.Cancel",
                    Style1 = "#a64b2a",
                    Style2 = "#4b5320",
                    Size = "lg",
                    CheckBoxTextKey = string.Empty,
                    CheckBoxKey = string.Empty,
                    CheckBottom = false,
                    BodyComponent = typeof(TModalRender)
                }
            };
    }
}
