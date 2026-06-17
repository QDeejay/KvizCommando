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
}
