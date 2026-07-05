using KvizCommando.Client.Data;
using KvizCommando.Client.Helpers;
using KvizCommando.Client.Models.ViewModels;
using KvizCommando.Client.Pages.Team;
using KvizCommando.Client.Services.Visual.UiService.Language;
using KvizCommando.Shared.Models;
using KvizCommando.Shared.Models.Dtos;
using System.Text.RegularExpressions;

namespace KvizCommando.Client.Features.Team
{
    public static class BottomBlockDataBuilder
    {
      
        public static BottomBlockViewModel BuildTeamView(TeamDtos team, string culture)
        {
            var vm = new BottomBlockViewModel();
            vm.Rows.Add(
                new BottomBlockRow(
                    "team.Label.TeamName",
                    true.ToString(), "", "", 0
                )
            );
            BuildTeamRows(team, vm, culture);
            return vm;
        }
        public static BottomBlockViewModel BuildMemberView(TeamMemberDto member, string culture)
        {
            var vm = new BottomBlockViewModel();
            vm.Rows.Add(
                new BottomBlockRow(
                    "team.Label.Modifiers",
                    false.ToString(), "", "", 0
                )
            );
            BuildMemberRows(member, vm, culture);
            return vm;
        }

        private static void BuildTeamRows(TeamDtos input, BottomBlockViewModel vm, string culture)
        {
            foreach (int j in Enumerable.Range(1, 8))
            {
                if (input.CharCatMask[j])
                {
                    var mem = input.TeamMembers[j] ?? new TeamMemberDto();

                    vm.Rows.Add(new BottomBlockRow(
                        mem.MemberName,
                        "<" + OrientationLocalizer.GetOrientShort(j, culture) + ">",
                        RankNameTable.Data[mem.MemberLvl].PublicLevel ?? "",
                        (int)mem.Remark == 0 ? string.Empty : $"team.modal.Button.{mem.Remark}",
                        (int)mem.Remark > 10 ? (int)mem.Remark + j : j 
                    ));
                }
            }
        }
        private static void BuildMemberRows(TeamMemberDto mem, BottomBlockViewModel vm, string culture)
        {
            int lvl = mem.MemberLvl;
            int skill = mem.SkillPoints;
            string levelShort = Right(RankNameTable.Data[lvl].PublicLevel ?? "", 2);
            var mainAt = mem.MaintAttitude;
            var secAt = mem.SecondAttitude;
            var genAt = mem.GenderAttitude;
            // - értékű sorok
            vm.Rows.Add(BuildMainSkillRow(0, mainAt.Category[0], mem.Level, levelShort, culture));
            vm.Rows.Add(BuildMainSkillRow(2, mainAt.Category[2], mem.Level, levelShort, culture));

            vm.Rows.Add(BuildSkillRow(secAt.Skill[0], secAt.Category[0], 0,  culture));
            vm.Rows.Add(BuildSkillRow(secAt.Skill[2], secAt.Category[2], 2,  culture));
            vm.Rows.Add(BuildSkillRow(genAt.Skill[0], genAt.Category[0], 4,  culture));
            vm.Rows.Add(BuildSkillRow(genAt.Skill[2], genAt.Category[2], 6,  culture));

            // + értékű sorok
            vm.Rows.Add(BuildMainSkillRow(1, mainAt.Category[1], mem.Level, levelShort, culture));
            vm.Rows.Add(BuildMainSkillRow(3, mainAt.Category[3], mem.Level, levelShort, culture));

            // Hátsó sorok
            vm.Rows.Add(BuildSkillRow(secAt.Skill[1], secAt.Category[1], 1, culture));
            vm.Rows.Add(BuildSkillRow(secAt.Skill[3], secAt.Category[3], 3, culture));
            vm.Rows.Add(BuildSkillRow(genAt.Skill[1], genAt.Category[1], 5, culture));
            vm.Rows.Add(BuildSkillRow(genAt.Skill[3], genAt.Category[3], 7, culture));
        }
        private static BottomBlockRow BuildSkillRow(SkillPartial skill, int category, int modifier, string culture)
        {
            double val = ModifierTable.Data[skill.LvlCurrent].Modifier[modifier] ?? 0.0;
            string prefix = val > 0 ? "+" : "";

            return new BottomBlockRow(
                CategoryNameLocalizer.GetCategory(category, culture),
                "",
                prefix + TeamHelper.FormatOneDecimal(val, false),
                skill.SkillCanDev ? "team.Label.Remark.Develop" : string.Empty,
                modifier+1
            );
        }
        private static BottomBlockRow BuildMainSkillRow(int skillno,int cat, int level, string levelshort, string culture)
        {
            double val = ModifierTable.DataMainSkill[skillno].StartValue + (level-1) * ModifierTable.DataMainSkill[skillno].StepValue;
            string prefix = val > 0 ? "+" : "";
            return new BottomBlockRow(
                CategoryNameLocalizer.GetCategory(cat, culture),
                levelshort,
                prefix + TeamHelper.FormatOneDecimal(val, false),
                "",
                0
                );
        }

        /*
            readonly ILanguageService _lang;

        public BottomBlockDataBuilder(ILanguageService lang)
        {
            _lang = lang;
        }
        
       
        ///private static readonly int[] sLevels = { 2,  3,  8,  9, 14, 15, 20, 21 };
   
         public BottomBlockViewModel Build(TeamDtos input, int tabPos, string culture)
        {
            var vm = new BottomBlockViewModel();

            bool isTeamMode = tabPos == 0;

            // Header row
            vm.Rows.Add(
                new BottomBlockRow(
                    isTeamMode ? _lang["team.Label.TeamName"] + ":" : _lang["team.Label.Modifiers"],
                    isTeamMode.ToString(), "", "",0
                )
            );

            if (isTeamMode)
                BuildTeamRows(input, vm, culture);
            else
                BuildMemberRows(input.TeamMembers[tabPos], vm, culture);

            return vm;
        }
        
        
        vm.Rows.Add(BuildSkillRow(mem.SecondAttitude.Skill[0], mem.SecondAttitude.Category[0], 0, skill,mem.Level, culture));
            vm.Rows.Add(BuildSkillRow(mem.SecondAttitude.Skill[2], mem.SecondAttitude.Category[2], 2, skill, mem.Level, culture));
            vm.Rows.Add(BuildSkillRow(mem.GenderAttitude.Skill[0], mem.GenderAttitude.Category[0], 4, skill, mem.Level, culture));
            vm.Rows.Add(BuildSkillRow(mem.GenderAttitude.Skill[2], mem.GenderAttitude.Category[2], 6, skill, mem.Level, culture));
         * 
         * 
         *             string remark = (skill.LvlCurrent < skill.LvlCurMax && skillPoints > 0 && memberlevel >= sLevels[modifier])
                ? _lang["team.Label.Remark.Develop"]
                : "";
        private  (string, int) GetRemark(TeamMemberDto mem, int teampoints, int teamlevel)
        {
            if (mem.EnergyPoints <= 0)
                return (mem.SkillPoints>0 ? (_lang["team.modal.Button.Heal"],400) : (_lang["team.modal.Button.Fire"],300));

            if (mem.NextXp <= mem.MemberXp && mem.Level<teamlevel)
                if (mem.MemberLvl == 21)
                    return (_lang["team.modal.Button.Retire"],200);
                else if (teampoints > 0)
                    return (_lang["team.modal.Button.Promote"],100);

            if (mem.SkillPoints > 0)
            {
                bool canDev = false;
                int index = -1;
                foreach (var s in mem.SecondAttitude.Skill)
                {
                    index++;
                    if (s.LvlCurMax > s.LvlCurrent && mem.Level >= sLevels[index]) canDev = true;
                }

                foreach (var s in mem.GenderAttitude.Skill)
                {
                    index++;
                    if (s.LvlCurMax > s.LvlCurrent && mem.Level >= sLevels[index]) canDev = true;
                }
                 
                if (canDev)
                    return (_lang["team.Label.Remark.Develop"],0);
            }

            return ("",0);
        }
        */
        private static string Right(string s, int count)
        {
            if (string.IsNullOrEmpty(s)) return s;
            if (s.Length <= count) return s;
            return "<" + s.Substring(s.Length - count) + ">";
        }
    }
}
