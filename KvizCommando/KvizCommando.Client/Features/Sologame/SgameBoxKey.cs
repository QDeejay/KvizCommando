using System.Security.Cryptography.X509Certificates;

namespace KvizCommando.Client.Features.Question

{
    internal enum SgameBoxKeyRoot
    {
        // -------------- Root ----------------- //
        RtBtnCategory,
        //------------------  
        RtBtnOrient,
        // ------------------
        RtBtnCampaign 
    };
    internal enum SgameBoxKeySub 
    {
        // ---------- Cat boxes * 16 -------- 
        BtnCat=16,
        // --------- Ori boxes * 8 ---------- //
        BtnOri=8
        // ------------------ 
    }

}
