using KvizCommando.Client.Features.Modal;

namespace KvizCommando.Client.Models.ViewModels.Ui
{
    public sealed record ModalBoxVm
    {
   
            public ModalTypes Mode { get; init; } = ModalTypes.None;
            public string Title { get; init; } = string.Empty;
            public string ActionText1 { get; init; } = string.Empty;
            public string ActionText2 { get; init; } = string.Empty;
            public string ActionStyle1 { get; init; } = string.Empty;
            public string ActionStyle2 { get; init; } = string.Empty;
            public string CloseText { get; init; } = string.Empty;
            public string Size { get; init; } = string.Empty;
            public string CheckBoxText { get; init; } = string.Empty;
            public string CheckBoxKey { get; init; } = string.Empty;
            public bool CheckBottom { get; init; } = new();
           
            public Type? BodyComponent { get; set; }
            public Dictionary<string, object?> BodyParameters { get; } = new();

        //public Func<Task> AsyncAction1 { get; init; } = () => Task.CompletedTask;
        //public Func<Task> AsyncAction2 { get; init; } = () => Task.CompletedTask;
        //public Action? AsyncCheckBoxAction { get; init; } = default!;

    }




    public abstract class ModalVmSpecs
    {
        public string TitleKey { get; init; } = string.Empty;
        public string TextKey1 { get; init; } = string.Empty;
        public string TextKey2 { get; init; } = string.Empty;
        public string Style1 { get; init; } = string.Empty;
        public string Style2 { get; init; } = string.Empty;
        public string CloseTextKey { get; init; } = string.Empty;
        public string Size { get; init; } = string.Empty;
        public string CheckBoxTextKey { get; init; } = string.Empty;
        public string CheckBoxKey { get; init; } = string.Empty;
        public bool CheckBottom { get; init; } = false;
        public Type? BodyComponent { get; set; }
    }
}
