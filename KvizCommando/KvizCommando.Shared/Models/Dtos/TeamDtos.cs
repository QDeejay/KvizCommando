using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KvizCommando.Shared.Models.Dtos
{
    public sealed class TeamDtos
    {
        public ExtendedInfo TeamInfo { get; set; } = new ExtendedInfo();
        public TeamMemberDto[]? TeamMembers { get; set; } = new TeamMemberDto[9];

        public CandidateDto[]? Candidates { get; set; } = new CandidateDto[8];

        public bool[] CharCatMask { get; set; } = new bool[9];
        public HelpDto Help { get; set; } = new HelpDto();
       
    }
    public  interface IGeneralInfo 
    {
        string Name { get; }
        string PictureCode { get; }
        int Level { get; }
        int Xp { get; }
        int NextXpPts { get; }
        int DevPts { get; }
    }
    public class ExtendedInfo : IGeneralInfo
    {
        public string TeamName { get; set; } = string.Empty;
        public int TeamLevel { get; set; } = 0;
        public int TeamXp { get; set; } = 0;
        public int NextXp { get; set; } = 0;
        public int DevPoints { get; set; } = 0;
        public string TeamPictureCode { get; set; } = string.Empty;
        public int TeamBonus { get; set; } = 0;
        public int Credits { get; set; } = 0;
        public int TotalMembers { get; set; } = 0;
        public int MaxMembers { get; set; } = 0;

        public string Name => TeamName;
        public string PictureCode => TeamPictureCode;
        public int Level => TeamLevel;
        public int Xp => TeamXp;
        public int NextXpPts => NextXp;
        public int DevPts => DevPoints;



    }
    public sealed class TeamMemberDto : IGeneralInfo
    {
        public string MemberName { get; set; } = string.Empty;
        public string MemberPictureCode { get; set; } = string.Empty;
        public int MemberXp { get; set; } = 0;
        public int MemberLvl { get; set; } = 0;
        public int NextXp { get; set; } = 0;
        public int NextDevPoints { get; set; } = 0;
        public int? NextUnlock { get; set; }
        public int Pension { get; set; } = 0;
        public int EnergyPoints { get; set; } = 0;
        public int SkillPoints { get; set; } = 0;
        



        public AttidtudeDto MaintAttitude { get; set; } = new AttidtudeDto();
        public AttidtudeDto SecondAttitude { get; set; } = new AttidtudeDto();
        public AttidtudeDto GenderAttitude { get; set; } = new AttidtudeDto();

        public string Name => MemberName;
        public string PictureCode => MemberPictureCode;
        public int Level => MemberLvl;
        public int Xp => MemberXp;
        public int NextXpPts => NextXp;
        public int DevPts => SkillPoints;
    }
    public class AttidtudeDto
    {
        public byte[] Category { get; set; } = new byte[4];
        public SkillPartial[] Skill { get; set; } = new SkillPartial[4];
    }
    public sealed class HelpDto : AttidtudeDto
    {
        
        public int[] HelpVolumes = new int[4];
    }
    public class SkillPartial
    {
        public byte lvlCurrent { get; set; } = 0;
        public byte lvlCurMax { get; set; } = 0;
        public byte lvlOvrMax { get; set; } = 0;
    }
    public sealed class CandidateDto
    {
        public bool CanBeHire { get; set; } = false;
        public DateTime ExpirationTime { get; set; } = DateTime.UtcNow;
        public string[]? Name { get; set; } = new string[8];
        public string[]? PictureCode { get; set; } = new string[8];
    }
}
