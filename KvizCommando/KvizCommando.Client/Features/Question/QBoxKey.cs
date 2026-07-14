using KvizCommando.Shared.Contracts.Question;
using KvizCommando.Shared.Contracts.Team;
using System.Security.Cryptography.X509Certificates;

namespace KvizCommando.Client.Features.Question

{
    internal enum QBoxKeyRoot
    {
        // -------------- Root ----------------- //
        RtBtnFactory,
        //------------------  
        RtBtnUsr,
        // ------------------
        RtBtnPendig,
        // ------------------ 
        RtBtnNew
    }
    internal enum QBoxKeyContent { 
        // --------- Content Boxes ---------- //
        FactSlots,
        // ------------------ 
        UsrSlots,
        // ----------------- 
        PendigSlots,
        // ------------------
        NewSlot
        // ------------------
    }
    internal enum QParamNames
    {
       SaveSlots,
       SelectedIdChanged,
       OnWatchButtonPushed, 
       OnHandleButtonPushed,
       OnSendQuestion
    }

    public sealed record QuestionCallbacks
    {
        public required Func<Task> OnDelete { get; init; }
        public required Func<Task> OnHandle { get; init; }
        public required Func<Task> OnWatch { get; init; }
        public required Func<int[], Task> OnFactSave { get; init; }
        public required Func<NewQuestionRequest, Task> OnSend { get; init; }
        public required Action<int> OnSelectId { get; init; }
    }

}
