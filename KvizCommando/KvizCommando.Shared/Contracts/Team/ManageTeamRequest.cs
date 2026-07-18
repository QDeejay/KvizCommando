namespace KvizCommando.Shared.Contracts.Team
{
    public sealed class ManageTeamRequest
    {
        public string SessionId { get; set; } = string.Empty;
        public int MemberNo { get; set; } = 0;
        public int CandidateId { get; set; } = 0;
        public ManageType ReqType { get; set; }

    }
    public enum ManageType
    {
        Hire = 0,
        Promote = 1,
        Retire = 2,
        Fire = 3,
        Heal = 4,
        UpdateCan = 10
    }
}



