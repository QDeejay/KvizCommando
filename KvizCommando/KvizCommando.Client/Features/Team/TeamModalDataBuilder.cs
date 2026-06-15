using KvizCommando.Client.Data;
using KvizCommando.Client.Helpers;
using KvizCommando.Client.Models.DataModels;
using KvizCommando.Client.Models.ViewModels;
using KvizCommando.Client.Pages.Team;
using KvizCommando.Client.Services.Language;
using KvizCommando.Shared.Models;
using KvizCommando.Shared.Models.Dtos;
using Microsoft.Extensions.Primitives;
using System.ComponentModel;
using System.Reflection.Emit;
using System.Runtime.InteropServices.JavaScript;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace KvizCommando.Client.Features.Team
{
    public interface ITeamModalDataBuilder
    {
        HireModalViewModel BuildHire(CandidateDto candidate,int hpos, int candno,string culture);
        PromoteModalView BuildPromote(TeamMemberDto member, string culture);
        RetireModalView BuildRetire(TeamMemberDto member,string culture);
        HandleModalView BuildHandle(TeamMemberDto member,string culture);
    }
    public class TeamModalDataBuilder : ITeamModalDataBuilder
    {
        private readonly ILanguageService _lang;
        public TeamModalDataBuilder(ILanguageService lang)
        {
            _lang = lang;
        }
        private const string UNLOCK_SEP = " => ";
        public HireModalViewModel BuildHire(CandidateDto candidate, int hpos, int candno, string culture)

        {
            var vm = new HireModalViewModel();
            var oriData = TeamHelper.RecruitResolver(hpos, candno);
            string orientkeys = RecruitData.OrientKeys[hpos - 1];
            var bi = new BasicInfo()
            {
                _name = candidate.Name[candno - 1] ?? string.Empty,
                _piccode = candidate.PictureCode[candno - 1] ?? string.Empty,
                _devpoints = "0",
                _orient1 = oriData.Item1,
                _orient2 = oriData.Item2,
                _level = 0
            };
            int[] orientcats = oriData.Item3;
            
            vm.Info = BuildInfoRow(bi,0, culture);
            vm.labelpros = _lang["team.modal.Label.Pros"];
            vm.labelcons = _lang["team.modal.Label.Cons"];
            int index = -1;
            double val;
            string pref = string.Empty;
            foreach (int i in orientcats)
            {
                index++;
                if (index == 0)
                    val = -2 - 0.4 * (bi._level - 1);
                else if (index == 3)
                    val = 10 - 0.4 * (bi._level - 1);
                else 
                    val = ModifierTable.Data[bi._level].Modifier[ModalConstants.HireVal[index]] ?? 0.0;
              
                    
                pref = val > 0 ? "+" : "";
                vm.Rows.Add(new ModalRow(
                        CategoryName: CategoryNameLocalizer.GetCategory(i,culture),
                        ValueDisplay: pref + TeamHelper.FormatOneDecimal(val, false),
                        separator:string.Empty,
                        ValueChangeDisplay:string.Empty,
                        color: index<3 ? "green" : "red"
                        ));
            }
            return vm;
        }
        public PromoteModalView BuildPromote(TeamMemberDto member, string culture)
        { 
            var vm = new PromoteModalView();
            var bi = BasicInfoResolver(member);
            int rc = bi._level==0 ? 0 : (bi._level - 1) / 3 + 1;
            int newLevel = Math.Min(bi._level+1, 21);
            int newRc = (newLevel - 1) / 3 + 1;
            int addDevPoints = RankRewards.List[newLevel].DevPointRevard;
            vm.Info = BuildInfoRow(bi, addDevPoints,culture);
           
            vm.Unlocks = _lang["team.Label.Attitude.Mai"] ;
            vm.UnlocksLevel = (RankNameTable.Data[newLevel].PublicLevel ?? "") + ": ";
            vm.UnlocksRank = RankNameLocalizer.GetName(newLevel, culture);
            vm.RankClass = RankNameLocalizer.GetClass(newRc, culture);
            vm.RankClassChanged = newRc > rc;
            vm.infotext1 = newRc > rc ? _lang["team.modal.Text.Promote2"] : _lang["team.modal.Text.Promote1"];
            vm.UnlockMaxLevels1 = _lang["team.Label.Attitude.Sec"] + " (max)";
            vm.UnlockMaxLevels2 = _lang["team.Label.Attitude.3rd"] + " (max)";
            vm.Rows.Add(newRc > rc ? new ModalRow(
                CategoryName: _lang["team.label.TeamDevPoint"],
                ValueDisplay: string.Empty,
                separator: UNLOCK_SEP,
                ValueChangeDisplay: "+"+RankRewards.List[newLevel].DevPointToStore.ToString(),
                color: "green"
                ) 
                : new ModalRow(
                    CategoryName: string.Empty,
                    ValueDisplay: string.Empty,
                    separator: string.Empty,
                    ValueChangeDisplay: string.Empty,
                    color: string.Empty
                    ));
            vm.Rows.Add(new ModalRow(
                CategoryName: _lang["team.Label.Vitality"][0..(_lang["team.Label.Vitality"].Length - 1)] + " maximum",
                ValueDisplay: $"{member.EnergyPoints}/{36 + member.MemberLvl * 3}",
                separator: UNLOCK_SEP,
                ValueChangeDisplay: $"{36 + newLevel * 3}/{36 + newLevel * 3}",
                color: ""
                ));
            AttitudeLineResolver(member.MaintAttitude, vm, RankConstants.startLevels[0..4], newLevel, culture);
            AttitudeLineResolver(member.SecondAttitude, vm, RankConstants.startLevels[4..8], newLevel, culture);
            AttitudeLineResolver(member.GenderAttitude, vm, RankConstants.startLevels[8..12], newLevel, culture);
            return vm; 
        }
        public RetireModalView BuildRetire(TeamMemberDto member,string culture)
        {
            var vm = new RetireModalView();
            var bi = BasicInfoResolver(member);
            
            
            int newLevel = 31;
            int newRc = 11;
            vm.Info = BuildInfoRow(bi,0, culture);
            vm.infotext1 = _lang["team.modal.Text.Retire1"];
            vm.Unlocks = _lang["team.modal.Label.Unlocks"];
            vm.UnlocksLevel = (RankNameTable.Data[newLevel].PublicLevel ?? "") + ": ";
            vm.UnlocksRank = RankNameLocalizer.GetName(newLevel, culture);
            vm.RankClass = RankNameLocalizer.GetClass(newRc, culture);
            vm.RankClassChanged = true;
            vm.Rows.Add(new ModalRow(
                CategoryName: _lang["team.Label.Pension"],
                ValueDisplay: string.Empty,
                separator: UNLOCK_SEP,
                ValueChangeDisplay: "+"+member.Pension.ToString(),
                color: "color: green;"
                ));
            vm.Rows.Add(new ModalRow(
                CategoryName: _lang["team.label.TeamDevPoint"],
                ValueDisplay: string.Empty,
                separator: UNLOCK_SEP,
                ValueChangeDisplay: "+"+RankRewards.List[22].DevPointToStore.ToString(),
                color: "color: green;"
                ));

            return vm; 
        }
        public HandleModalView BuildHandle(TeamMemberDto member, string culture)
        { 
            var vm = new HandleModalView();
            var bi = BasicInfoResolver(member);
            
            vm.Info = BuildInfoRow(bi,0, culture);
            vm.infotext1 = _lang["team.modal.Text.Handle2"];
            vm.infotext2 = _lang["team.modal.Text.Handle1"];
            vm.infotext3 = _lang["team.modal.Text.Handle3"];
            if (member.SkillPoints == 0)
                vm.infotext4 = _lang["team.modal.Text.Handle4"].FormatSafe(vm.Info.Devpoints[0..7]);
            else 
                vm.infotext4=string.Empty;
            return vm; 
        }
        private InfoBlock BuildInfoRow(BasicInfo inforowdata, int adddevpoints,  string culture)
        {

            return new InfoBlock(
                Name: _lang["team.Label.Name"],
                Color: GenreColorResolver(inforowdata._piccode),
                NameValue: inforowdata._name,
                Rank: _lang["team.Label.Rank"],
                RankValue: RankNameLocalizer.GetName(inforowdata._level, culture),
                Level: _lang["team.Label.Level"],
                LevelValue: RankNameTable.Data[inforowdata._level].PublicLevel ?? "",
                Orient1: _lang["team.modal.Label.Orient1"],
                Orient2: _lang["team.modal.Label.Orient2"],
                Orient1Value: OrientationLocalizer.GetOrientation(inforowdata._orient1, culture),
                Orient2Value: OrientationLocalizer.GetOrientation(inforowdata._orient2, culture),
                Devpoints: _lang["team.Label.SkillPointShort"].FormatSafe(OrientationLocalizer.GetOrientShort(inforowdata._orient1, culture)),
                DevPointsValue: inforowdata._devpoints,
                AddedDevPoints: adddevpoints > 0 ? "+" + adddevpoints.ToString() : ""
                );
        }
        private BasicInfo BasicInfoResolver(TeamMemberDto member)
        { 
           
            return new BasicInfo() 
            { 
                _name = member.Name,
                _piccode = member.PictureCode,
                _devpoints = member.DevPts.ToString(),
                _level = member.Level,
                _orient1 = member.MaintAttitude.Category[0] > 8 ? member.MaintAttitude.Category[2] : member.MaintAttitude.Category[0],
                _orient2 = member.SecondAttitude.Category[0] > 8 ? member.SecondAttitude.Category[2] : member.SecondAttitude.Category[0]
            }; 
        }
        private void AttitudeLineResolver(AttidtudeDto att, PromoteModalView vm, int[] slevel, int level, string culture)
        {
            int actL = level-1;
            int newL = level;
            double valC=0.0;
            double valN=0.0;
            string pref=string.Empty;

            for (int i=0; i<4; i++) 
            {
                if (slevel[i] == 0)
                {
                    valC = ModifierTable.DataMainSkill[i].StartValue + (actL - 1) * ModifierTable.DataMainSkill[i].StepValue;
                    valN = ModifierTable.DataMainSkill[i].StartValue + (newL - 1) * ModifierTable.DataMainSkill[i].StepValue;
                    pref = valC > 0 ? "+" : "";
                }
                if (newL >= slevel[i])
                {
                    vm.Rows.Add(new ModalRow(
                     CategoryName: CategoryNameLocalizer.GetCategory(att.Category[i], culture),
                     ValueDisplay: slevel[i] != 0 ? $"{att.Skill[i].lvlCurrent}/{att.Skill[i].lvlCurMax}" :pref + (TeamHelper.FormatOneDecimal(valC, false)),
                     separator: UNLOCK_SEP,
                     ValueChangeDisplay: slevel[i] != 0 ? $"{att.Skill[i].lvlCurrent}/{att.Skill[i].lvlCurMax+1}" : pref + (TeamHelper.FormatOneDecimal(valN, false)),
                     color: slevel[i] == 0 ? pref == "+" ? "red" : "green" : ""
                    ));
                }
                
            }
        }
        private string GenreColorResolver(string pictureCode)
        {
            if (string.IsNullOrEmpty(pictureCode))
                return "unknown";
            return pictureCode.StartsWith
                    ("M") ? "lightblue" : "pink";
        }
        private class BasicInfo
        { 
            public string _name = string.Empty;
            public string _piccode = string.Empty;
            public string _devpoints = string.Empty;
            public int _level = 0;
            public int _orient1 =0;
            public int _orient2 =0;
        }

    }
}
