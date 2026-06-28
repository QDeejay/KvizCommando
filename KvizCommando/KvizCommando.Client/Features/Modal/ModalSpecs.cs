namespace KvizCommando.Client.Features.Question
{
    public class ModalSpecs
    {
        public static readonly ModalPar[] Specs = new[]
        {
            new ModalPar
            {

                Mode = 1,
                Title = "PLACEHOLDER",
                ActionText1 = "checkin.modal.Button.Action",
                ActionText2 = "",
                CloseText = "",
                ActionStyle1 = string.Empty,
                ActionStyle2 = string.Empty,
                Size = "modal-lg",
                CheckBoxText = string.Empty,
                CheckBoxKey = string.Empty,
                CheckBottom = true
            },
            new ModalPar
            {
                
                Mode = 102,
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
                Mode = 103,
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
                Mode = 104,
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
            },
            new ModalPar
            {
                Mode = 201,
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
                Mode = 202,
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
                Mode = 203,
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
                Mode = 204,
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
