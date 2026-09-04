using KvizCommando.Client.Features.Shared.Modal.Components;
using KvizCommando.Client.Features.Shared.Modal.ViewModels;

namespace KvizCommando.Client.Features.Shared.Modal.Builders
{
    public sealed class MboxSpecs : ModalVmSpecs
    {
    }

    public static class ModalBox
    {

        public static readonly IReadOnlyDictionary<ModalTypes, MboxSpecs> Specs =

            new Dictionary<ModalTypes, MboxSpecs>
            {
                [ModalTypes.Terms] = new MboxSpecs
                {
                    TitleKey = string.Empty,
                    TextKey1 = "checkin.modal.Button.Action",
                    Size = "modal-xl",
                    CheckBottom = true,
                    BodyComponent = null
                },

                [ModalTypes.LangConfirm] = new MboxSpecs
                {
                    TitleKey = string.Empty,
                    TextKey1 = string.Empty,
                    TextKey2 = string.Empty,
                    CloseTextKey = "common.Modal.Language.Keep",
                    Style1 = "#4b5320",
                    Style2 = string.Empty,
                    Size = string.Empty,
                    BodyComponent = typeof(DBoxModalRender)
                },
                [ModalTypes.DialogConfirm] = new MboxSpecs
                {
                    TitleKey = "common.Modal.Confirm.Title",
                    TextKey1 = "common.Modal.Confirm.Accept",
                    TextKey2 = string.Empty,
                    CloseTextKey = "common.Modal.Confirm.Cancel",
                    Style1 = "#a64b2a",
                    Style2 = string.Empty,
                    Size = string.Empty,
                    BodyComponent = typeof(DBoxModalRender)
                },
                [ModalTypes.QUsrDelet] = new MboxSpecs
                {
                    TitleKey = "question.Modal.Title.Confirm",
                    TextKey1 = "question.Button.Delete",
                    TextKey2 = string.Empty,
                    CloseTextKey = "question.Button.Close",
                    Style1 = "#a64b2a",
                    Style2 = string.Empty,
                    Size = string.Empty,
                    CheckBoxTextKey = "mainlayout.CheckBox.NotShow",
                    CheckBoxKey = ModalConst.LOCAL_NOT_SHOW_DEL,
                    CheckBottom = false,
                    BodyComponent = typeof(QModalRender)
                },

                [ModalTypes.QPendHandle] = new MboxSpecs
                {
                    TitleKey = "question.Modal.Title.Handling",
                    TextKey1 = "question.Button.Delete",
                    TextKey2 = "question.Button.Move",
                    CloseTextKey = "question.Button.Close",
                    Style1 = "#a64b2a",
                    Style2 = string.Empty,
                    Size = "modal-lg",
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
                    Size = "modal-lg",
                    CheckBoxTextKey = string.Empty,
                    CheckBoxKey = string.Empty,
                    CheckBottom = true,
                    BodyComponent = typeof(QModalRender)
                },

                [ModalTypes.QNewRules] = new MboxSpecs
                {
                    TitleKey = "question.Modal.Title.New",
                    TextKey1 = "question.Button.Send",
                    TextKey2 = string.Empty,
                    CloseTextKey = "question.Button.Close",
                    Style1 = "#4b5320",
                    Style2 = string.Empty,
                    Size = "modal-xl",
                    CheckBoxTextKey = "mainlayout.CheckBox.NotShow",
                    CheckBoxKey = ModalConst.LOCAL_NOT_SHOW_NEW,
                    CheckBottom = true,
                    BodyComponent = typeof(QModalRender)
                },

                [ModalTypes.THire] = new MboxSpecs
                {
                    TitleKey = "team.modal.Title.Hire",
                    TextKey1 = "team.modal.Button.Hire",
                    TextKey2 = string.Empty,
                    CloseTextKey = "team.modal.Button.Cancel",
                    Style1 = "#4b5320",
                    Style2 = string.Empty,
                    Size = "modal-lg",
                    SizeLock = true,
                    CheckBoxTextKey = string.Empty,
                    CheckBoxKey = string.Empty,
                    CheckBottom = false,
                    BodyComponent = typeof(TModalRender)
                },

                [ModalTypes.TPromoteMember] = new MboxSpecs
                {
                    TitleKey = "team.modal.Title.Promote",
                    TextKey1 = "team.modal.Button.Promote",
                    TextKey2 = string.Empty,
                    CloseTextKey = "team.modal.Button.Cancel",
                    Style1 = "#4b5320",
                    Style2 = string.Empty,
                    Size = "modal-lg",
                    SizeLock = false,
                    CheckBoxTextKey = string.Empty,
                    CheckBoxKey = string.Empty,
                    CheckBottom = true,
                    BodyComponent = typeof(TModalRender)
                },

                [ModalTypes.TRetire] = new MboxSpecs
                {
                    TitleKey = "team.modal.Title.Retire",
                    TextKey1 = "team.modal.Button.Retire",
                    TextKey2 = string.Empty,
                    CloseTextKey = "team.modal.Button.Cancel",
                    Style1 = "#4b5320",
                    Style2 = string.Empty,
                    Size = "modal-lg",
                    SizeLock = false,
                    CheckBoxTextKey = string.Empty,
                    CheckBoxKey = string.Empty,
                    CheckBottom = false,
                    BodyComponent = typeof(TModalRender)
                },

                [ModalTypes.THandle] = new MboxSpecs
                {
                    TitleKey = "team.modal.Title.Handle",
                    TextKey1 = "team.modal.Button.Fire",
                    TextKey2 = "team.modal.Button.Heal",
                    CloseTextKey = "team.modal.Button.Cancel",
                    Style1 = "#a64b2a",
                    Style2 = "#4b5320",
                    Size = "modal-lg",
                    SizeLock = false,
                    CheckBoxTextKey = string.Empty,
                    CheckBoxKey = string.Empty,
                    CheckBottom = false,
                    BodyComponent = typeof(TModalRender)
                },
                [ModalTypes.TPromoteTeam] = new MboxSpecs
                {
                    TitleKey = "team.modal.Title.PromoteTeam",
                    TextKey1 = string.Empty,
                    TextKey2 = string.Empty,
                    CloseTextKey = "team.modal.Button.Ack",
                    Style1 = string.Empty,
                    Style2 = string.Empty,
                    Size = "modal-lg",
                    SizeLock = false,
                    CheckBoxTextKey = string.Empty,
                    CheckBoxKey = string.Empty,
                    CheckBottom = true,
                    BodyComponent = typeof(TModalRender)
                },
            };
    }
}
