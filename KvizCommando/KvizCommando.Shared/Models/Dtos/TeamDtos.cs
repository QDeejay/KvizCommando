using KvizCommando.Shared.Models.Enums;

namespace KvizCommando.Shared.Models.Dtos
{
    public sealed class TeamDtos
    {
        public bool AccessDenied { get; set; } = false;
        public TeamExtendedInfo TeamInfo { get; set; } = new();
        public TeamMemberDto[]? TeamMembers { get; set; } = new TeamMemberDto[9];
        public CandidateDto[]? Candidates { get; set; } = new CandidateDto[8];
        public TeamRootBoxInfo RootBoxInfo { get; set; } = new();
        public bool[] CharCatMask { get; set; } = new bool[9];

        public HelpDto Help { get; set; } = new HelpDto();

    }

    public class TeamExtendedInfo
    {
        public string Name { get; set; } = string.Empty;
        public int Level { get; set; } = 0;
        public int Xp { get; set; } = 0;
        public int NextXp { get; set; } = 0;
        public int DevPoints { get; set; } = 0;
        public string PictureCode { get; set; } = string.Empty;
        public int Bonus { get; set; } = 0;
        public int Credits { get; set; } = 0;
        public int TotalMembers { get; set; } = 0;
        public int AbleToHire { get; set; } = 0;
        public int MaxMembers { get; set; } = 0;
        public bool[] AbleToHireMask = new bool[9];
        public MembRemark[] MembRemarks { get; set; } = new MembRemark[9];
    }
    public sealed class TeamMemberDto
    {
        public string Name { get; set; } = string.Empty;
        public string PictureCode { get; set; } = string.Empty;
        public int Xp { get; set; } = 0;
        public int Level { get; set; } = 0;
        public int NextXp { get; set; } = 0;
        public int NextDevPoints { get; set; } = 0;
        public int? NextUnlock { get; set; }
        public int Pension { get; set; } = 0;
        public int EnergyPoints { get; set; } = 0;
        public int SkillPoints { get; set; } = 0;
        public MembRemark Remark { get; set; } = MembRemark.None;

        public AttidtudeDto MaintAttitude { get; set; } = new AttidtudeDto();
        public AttidtudeDto SecondAttitude { get; set; } = new AttidtudeDto();
        public AttidtudeDto GenderAttitude { get; set; } = new AttidtudeDto();

    }
    public class AttidtudeDto
    {
        public bool CanDev { get; set; } = false;
        public byte[] Category { get; set; } = new byte[4];
        public SkillPartial[] Skill { get; set; } = new SkillPartial[4];
    }
    public sealed class HelpDto : AttidtudeDto
    {

        public int[] HelpVolumes = new int[4];
    }
    public class SkillPartial
    {
        public byte LvlCurrent { get; set; } = 0;
        public byte LvlCurMax { get; set; } = 0;
        public byte LvlOvrMax { get; set; } = 0;
        public bool SkillCanDev = false;
    }
    public sealed class CandidateDto
    {
        public bool CanBeHire { get; set; } = false;
        public DateTime ExpirationTime { get; set; } = DateTime.UtcNow;
        public string[]? Name { get; set; } = new string[8];
        public string[]? PictureCode { get; set; } = new string[8];
    }
    public sealed class TeamRootBoxInfo
    {
        public int TeamOpRequired { get; set; } = 0;
        public int MemberOpRequired { get; set; } = 0;
        public int FreePositions { get; set; } = 0;
        public int AbleToHire { get; set; } = 0;
        public bool IsTeamEnable { get; set; } = false;
        public bool IsMemberEnable { get; set; } = false;
        public bool IsRecruitEnable { get; set; } = false;
    }




}
/*
 * 
        public string Name => MemberName;
        public string PictureCode => MemberPictureCode;
        public int Level => MemberLvl;
        public int Xp => MemberXp;
        public int NextXpPts => NextXp;
        public int DevPts => SkillPoints;
 *         public string Name => TeamName;
        public string PictureCode => TeamPictureCode;
        public int Level => TeamLevel;
        public int Xp => TeamXp;
        public int NextXpPts => NextXp;
        public int DevPts => DevPoints;
 * 
  public interface IGeneralInfo 
    {
        string Name { get; }
        string PictureCode { get; }
        int Level { get; }
        int Xp { get; }
        int NextXpPts { get; }
        int DevPts { get; }
    }
 */