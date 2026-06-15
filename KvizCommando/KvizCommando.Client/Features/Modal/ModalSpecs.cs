namespace KvizCommando.Client.Features.Question
{
    public class ModalSpecs
    {
        public static readonly ModalPar[] SpecsQ = new[]
        {
            new ModalPar
            {
                
                Mode = 1,
                Title = "question.Modal.Title.Confirm",
                ActionText1 = "question.Button.Delete",
                ActionText2 = "",
                CloseText = "question.Button.Close",
                ActionStyle1 = "background-color: #a64b2a",
                ActionStyle2 = string.Empty,
                Size = "modal-lg",
                CheckBoxText = "mainlayout.CheckBox.NotShow",
                CheckBoxKey = "",
                CheckBottom = false
            },
            new ModalPar
            {

                Mode = 2,
                Title = "question.Modal.Title.Handling",
                ActionText1 = "question.Button.Delete",
                ActionText2 = "question.Button.Move",
                CloseText = "question.Button.Close",
                ActionStyle1 = "background-color: #a64b2a",
                ActionStyle2 = "",
                Size = "modal-lg",
                CheckBoxText = "",
                CheckBoxKey = "",
                CheckBottom = false
            },
            new ModalPar
            {
                // manage user question, kérdés törlés 
                Mode = 3,
                Title = "question.Modal.Title.New",
                ActionText1 = "question.Button.Send",
                ActionText2 = "",
                CloseText = "question.Button.Close",
                ActionStyle1 = "background-color: #4b5320",
                ActionStyle2 = string.Empty,
                Size = "modal-lg",
                CheckBoxText = "mainlayout.CheckBox.NotShow",
                CheckBoxKey = "",
                CheckBottom = true
            }
        };
        public static readonly ModalPar[] SpecsT = new[]
        {
            new ModalPar
            {
                Mode = 1,
                Title ="team.modal.Title.Hire",
                ActionText1 = "team.modal.Button.Hire",
                ActionText2 = "",
                CloseText = "team.modal.Button.Cancel",
                ActionStyle1 ="background-color: #4b5320",
                ActionStyle2 = "",
                Size = "Modal-lg",
                CheckBoxText = "",
                CheckBoxKey = "",
                CheckBottom = false
            },
            new ModalPar
            {
                Mode = 2,
                Title ="team.modal.Title.Promote",
                ActionText1 = "team.modal.Button.Promote",
                ActionText2 = "",
                CloseText = "team.modal.Button.Cancel",
                ActionStyle1 ="background-color: #4b5320",
                ActionStyle2 = "",
                Size = "Modal-lg",
                CheckBoxText = "",
                CheckBoxKey = "",
                CheckBottom = true
            },
            new ModalPar
            {
                Mode = 3,
                Title ="team.modal.Title.Retire",
                ActionText1 = "team.modal.Button.Retire",
                ActionText2 = "",
                CloseText = "team.modal.Button.Cancel",
                ActionStyle1 ="background-color: #4b5320",
                ActionStyle2 = "",
                Size = "Modal-lg",
                CheckBoxText = "",
                CheckBoxKey = "",
                CheckBottom = false
            },
            new ModalPar
            {
                Mode = 4,
                Title ="team.modal.Title.Handle",
                ActionText1 = "team.modal.Button.Fire",
                ActionText2 = "team.modal.Button.Heal",
                CloseText = "team.modal.Button.Cancel",
                ActionStyle1 ="background-color: #a64b2a",
                ActionStyle2 = "background-color: #4b5320",
                Size = "Modal-lg",
                CheckBoxText = "",
                CheckBoxKey = "",
                CheckBottom = false
            }
        };
    }
}
