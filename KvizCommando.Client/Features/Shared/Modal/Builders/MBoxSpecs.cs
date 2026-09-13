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
                    TextKey1 = "modal.Button.CheckIn.Acknowledge",
                    Size = "modal-xl",
                    CheckBottom = true,
                    BodyComponent = null
                },

                [ModalTypes.LangConfirm] = new MboxSpecs
                {
                    TitleKey = string.Empty,
                    TextKey1 = string.Empty,
                    TextKey2 = string.Empty,
                    CloseTextKey = "modal.Button.Language.Keep",
                    Style1 = "#4b5320",
                    Style2 = string.Empty,
                    Size = string.Empty,
                    BodyComponent = typeof(DBoxModalRender)
                },
                [ModalTypes.DialogConfirm] = new MboxSpecs
                {
                    TitleKey = "modal.Title.Confirm",
                    TextKey1 = "modal.Button.Confirm.Yes",
                    TextKey2 = string.Empty,
                    CloseTextKey = "modal.Button.Confirm.No",
                    Style1 = "#a64b2a",
                    Style2 = string.Empty,
                    Size = string.Empty,
                    BodyComponent = typeof(DBoxModalRender)
                },
                [ModalTypes.QUsrDelet] = new MboxSpecs
                {
                    TitleKey = "modal.Title.Question.UserDelete",
                    TextKey1 = "modal.Button.Question.Delete",
                    TextKey2 = string.Empty,
                    CloseTextKey = "modal.Button.Question.Close",
                    Style1 = "#a64b2a",
                    Style2 = string.Empty,
                    Size = string.Empty,
                    CheckBoxTextKey = "modal.CheckBox.NotShowAgain",
                    CheckBoxKey = ModalConst.LOCAL_NOT_SHOW_DEL,
                    CheckBottom = false,
                    BodyComponent = typeof(QModalRender)
                },

                [ModalTypes.QPendHandle] = new MboxSpecs
                {
                    TitleKey = "modal.Title.Question.PendingHandling",
                    TextKey1 = "modal.Button.Question.Delete",
                    TextKey2 = "modal.Button.Question.Move",
                    CloseTextKey = "modal.Button.Question.Close",
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
                    TitleKey = "modal.Title.Question.View",
                    TextKey1 = string.Empty,
                    TextKey2 = string.Empty,
                    CloseTextKey = "modal.Button.Question.Close",
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
                    TitleKey = "modal.Title.Question.New",
                    TextKey1 = "modal.Button.Question.Send",
                    TextKey2 = string.Empty,
                    CloseTextKey = "modal.Button.Question.Close",
                    Style1 = "#4b5320",
                    Style2 = string.Empty,
                    Size = "modal-xl",
                    CheckBoxTextKey = "modal.CheckBox.NotShowAgain",
                    CheckBoxKey = ModalConst.LOCAL_NOT_SHOW_NEW,
                    CheckBottom = true,
                    BodyComponent = typeof(QModalRender)
                },

                [ModalTypes.THire] = new MboxSpecs
                {
                    TitleKey = "modal.Title.Team.Hire",
                    TextKey1 = "modal.Button.Team.Hire",
                    TextKey2 = string.Empty,
                    CloseTextKey = "modal.Button.Team.Cancel",
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
                    TitleKey = "modal.Title.Team.PromoteMember",
                    TextKey1 = "modal.Button.Team.Promote",
                    TextKey2 = string.Empty,
                    CloseTextKey = "modal.Button.Team.Cancel",
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
                    TitleKey = "modal.Title.Team.Retire",
                    TextKey1 = "modal.Button.Team.Retire",
                    TextKey2 = string.Empty,
                    CloseTextKey = "modal.Button.Team.Cancel",
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
                    TitleKey = "modal.Title.Team.Handle",
                    TextKey1 = "modal.Button.Team.Fire",
                    TextKey2 = "modal.Button.Team.Heal",
                    CloseTextKey = "modal.Button.Team.Cancel",
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
                    TitleKey = "modal.Title.Team.PromoteTeam",
                    TextKey1 = string.Empty,
                    TextKey2 = string.Empty,
                    CloseTextKey = "modal.Button.Team.Acknowledge",
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
