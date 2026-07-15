using KvizCommando.Shared.Contracts.Team;

namespace KvizCommando.Client.Pages.Team.Features
{
    internal enum TBoxKeyRoot
    {
        // -------------- Root ----------------- //
        RtBtnTeam,
        //------------------  
        RtBtnMembers,
        // ------------------
        RtBtnRecruit,
        // ------------------ 

    }
    internal enum TBoxKeyContent
    {
        // --------- Content Boxes ---------- //
        Team,
        // ------------------ 
        Member,
        // ----------------- 
        Recruit,
        // ------------------
    }

    internal enum TParamNames
    {
        OnManagePushed,
        OnModifySkillPushed,
        OnCandidateHired,
        CandidateOrder
    }
    public sealed record TeamCallbacks
    {
        public required Func<ModifySkillRequest, Task> OnModify { get; init; }
        public required Func<int, Task> OnHire { get; init; }
        public required Func<int, Task> OnManage { get; init; }

        public required int[] OnShuffledIds { get; init; }
        // public required Action<int[]> OnShuffledIds { get; init; }
    }
}
