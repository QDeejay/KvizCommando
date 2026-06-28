namespace KvizCommando.Client.Features.Question
{
    public sealed record ModalPar
    {
        public string Title { get; init; } = string.Empty;
        public string ActionText1 { get; init; } = string.Empty;
        public string ActionText2 { get; init; } = string.Empty;
        public string ActionStyle1 { get; init; } = string.Empty;
        public string ActionStyle2 { get; init; } = string.Empty;
        public string CloseText { get; init; } = string.Empty;
        public string Size { get; init; } = string.Empty;
        public string CheckBoxText { get; init; } = string.Empty;
        public string CheckBoxKey { get; init; } = string.Empty;
        public bool CheckBottom { get; init; } = false;
        //public Func<Task> AsyncAction1 { get; init; } = () => Task.CompletedTask;
        //public Func<Task> AsyncAction2 { get; init; } = () => Task.CompletedTask;
        //public Action? AsyncCheckBoxAction { get; init; } = default!;

        public int Mode { get; init; } = 0;
        
        

       
    }
   
}
