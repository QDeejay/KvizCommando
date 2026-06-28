using KvizCommando.Client.Data;
using KvizCommando.Client.Helpers;
using KvizCommando.Client.Models.ViewModels;
using KvizCommando.Client.Pages.Team;
using KvizCommando.Shared.Models.Dtos;
using KvizCommando.Shared.Models;
using KvizCommando.Client.Services.Visual.UiService.Language;

namespace KvizCommando.Client.Features.Team
{
    public class BottomBlockDataBuilder
    {
        readonly ILanguageService _lang;

        public BottomBlockDataBuilder(ILanguageService lang)
        {
            _lang = lang;
        }
        private static readonly int[] sLevels = { 2,  3,  8,  9, 14, 15, 20, 21 };
   
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

        private void BuildTeamRows(TeamDtos input, BottomBlockViewModel vm, string culture)
        {
            foreach (int j in Enumerable.Range(1, 8))
            {
                if (input.CharCatMask[j])
                {
                    var mem = input.TeamMembers[j] ?? new TeamMemberDto();

                    var remark = GetRemark(mem,  input.TeamInfo.DevPts, input.TeamInfo.TeamLevel);

                    vm.Rows.Add(new BottomBlockRow(
                        mem.MemberName,
                        "<" + OrientationLocalizer.GetOrientShort(j, culture) + ">",
                        RankNameTable.Data[mem.MemberLvl].PublicLevel ?? "",
                        remark.Item1,
                        remark.Item2>0 ? remark.Item2+j : j 
                    ));
                }
            }
        }

        private void BuildMemberRows(TeamMemberDto mem, BottomBlockViewModel vm, string culture)
        {
            int lvl = mem.MemberLvl;
            int skill = mem.SkillPoints;
            string levelShort = Right(RankNameTable.Data[lvl].PublicLevel ?? "", 2);
            // - értékű sorok
            vm.Rows.Add(BuildMainSkillRow(0, mem.MaintAttitude.Category[0], mem.Level, levelShort, culture));
            vm.Rows.Add(BuildMainSkillRow(2, mem.MaintAttitude.Category[2], mem.Level, levelShort, culture));

            vm.Rows.Add(BuildSkillRow(mem.SecondAttitude.Skill[0], mem.SecondAttitude.Category[0], 0, skill,mem.Level, culture));
            vm.Rows.Add(BuildSkillRow(mem.SecondAttitude.Skill[2], mem.SecondAttitude.Category[2], 2, skill, mem.Level, culture));
            vm.Rows.Add(BuildSkillRow(mem.GenderAttitude.Skill[0], mem.GenderAttitude.Category[0], 4, skill, mem.Level, culture));
            vm.Rows.Add(BuildSkillRow(mem.GenderAttitude.Skill[2], mem.GenderAttitude.Category[2], 6, skill, mem.Level, culture));

            // + értékű sorok
            vm.Rows.Add(BuildMainSkillRow(1, mem.MaintAttitude.Category[1], mem.Level, levelShort, culture));
            vm.Rows.Add(BuildMainSkillRow(3, mem.MaintAttitude.Category[3], mem.Level, levelShort, culture));

            // Hátsó sorok
            vm.Rows.Add(BuildSkillRow(mem.SecondAttitude.Skill[1], mem.SecondAttitude.Category[1], 1, skill, mem.Level, culture));
            vm.Rows.Add(BuildSkillRow(mem.SecondAttitude.Skill[3], mem.SecondAttitude.Category[3], 3, skill, mem.Level, culture));
            vm.Rows.Add(BuildSkillRow(mem.GenderAttitude.Skill[1], mem.GenderAttitude.Category[1], 5, skill, mem.Level, culture));
            vm.Rows.Add(BuildSkillRow(mem.GenderAttitude.Skill[3], mem.GenderAttitude.Category[3], 7, skill, mem.Level, culture));
        }

        private BottomBlockRow BuildSkillRow(
            SkillPartial skill, int category, int modifier, int skillPoints, int memberlevel, string culture)
        {
            double val = ModifierTable.Data[skill.lvlCurrent].Modifier[modifier] ?? 0.0;
            string prefix = val > 0 ? "+" : "";
            string remark = (skill.lvlCurrent < skill.lvlCurMax && skillPoints > 0 && memberlevel >= sLevels[modifier])
                ? _lang["team.Label.Remark.Develop"]
                : "";

            return new BottomBlockRow(
                CategoryNameLocalizer.GetCategory(category, culture),
                "",
                prefix + TeamHelper.FormatOneDecimal(val, false),
                remark,
                modifier+1
            );
        }
        private BottomBlockRow BuildMainSkillRow(int skillno,int cat, int level, string levelshort, string culture)
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
        private (string, int) GetRemark(TeamMemberDto mem, int teampoints, int teamlevel)
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
                    if (s.lvlCurMax > s.lvlCurrent && mem.Level >= sLevels[index]) canDev = true;
                }

                foreach (var s in mem.GenderAttitude.Skill)
                {
                    index++;
                    if (s.lvlCurMax > s.lvlCurrent && mem.Level >= sLevels[index]) canDev = true;
                }
                 
                if (canDev)
                    return (_lang["team.Label.Remark.Develop"],0);
            }

            return ("",0);
        }

        private static string Right(string s, int count)
        {
            if (string.IsNullOrEmpty(s)) return s;
            if (s.Length <= count) return s;
            return "<" + s.Substring(s.Length - count) + ">";
        }
    }
}
