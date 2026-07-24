namespace KvizCommando.Client.Features.Solo.Builders

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
        BtnCat = 16,
        // --------- Ori boxes * 8 ---------- //
        BtnOri = 8,
        // ------------------ 
        GameBoxCat,
        GameBoxOri
    }

}
