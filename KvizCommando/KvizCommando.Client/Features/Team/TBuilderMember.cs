using KvizCommando.Client.Data;
using KvizCommando.Client.Helpers;
using KvizCommando.Client.Models.ViewModels;
using KvizCommando.Client.Pages.Team;
using KvizCommando.Client.Services.Visual.UiService.Language;
using KvizCommando.Shared.Models;
using KvizCommando.Shared.Models.Dtos;

namespace KvizCommando.Client.Features.Team
{
    public class TBuilderMember
    {
        private readonly ILanguageService _lang;

        public TBuilderMember(ILanguageService lang)
        {
            _lang = lang;
        }
        public UpperBlockVm BuildMemberUpperVm(TeamMemberDto member, string culture)
        {
            var vm = new UpperBlockVm();

            // Közös adatok
            string name = member.Name;
            string publicLevel = RankNameTable.Data[member.Level].PublicLevel ?? "";
            string devPointsDisplay = member.SkillPoints.ToString();

            var m = member;
            int c1 = THelpers.NormalizeCategory(m.MaintAttitude.Category[0]);
            int c2 = THelpers.NormalizeCategory(m.SecondAttitude.Category[0]);
            int vitMax = 36 + m.Level * 3;
            int vitAct = Math.Min(m.EnergyPoints, vitMax);
            int rankClass = m.Level == 0 ? 0 : (m.Level - 1) / 3 + 1;
            string o1 = OrientationLocalizer.GetOrientation(c1, culture);
            string o2 = OrientationLocalizer.GetOrientation(c2, culture);
            string oShort = OrientationLocalizer.GetOrientShort(c1, culture);

            vm.Rows.Add(new(_lang["team.Label.Name"], name));
            vm.Rows.Add(new(_lang["team.Label.Rank"], RankNameLocalizer.GetName(m.Level, culture)));
            vm.Rows.Add(new(_lang["team.Label.Class"], RankNameLocalizer.GetClass(rankClass, culture)));
            vm.Rows.Add(new(_lang["team.SubBtn.Main"] + ":", o1));
            vm.Rows.Add(new(_lang["team.SubBtn.Second"] + ":", o2));


            vm.Rows.Add(new(_lang["team.Label.Level"], publicLevel));
            vm.Rows.Add(new(_lang["team.Label.Vitality"], $"{vitAct}/{vitMax}"));
            vm.Rows.Add(new(_lang["team.Label.Next"], (m.NextXp - m.Xp).ToString()));
            vm.Rows.Add(new(_lang["team.Label.SkillPointShort"].FormatSafe(oShort), devPointsDisplay));
            vm.Rows.Add(new(_lang["team.Label.Pension"], m.Pension.ToString()));


            return vm;
        }
        public BottomBlockVm BuildMemberBottomVm(TeamMemberDto member, string culture)
        {
            var vm = new BottomBlockVm();

            // Header row
            vm.Rows.Add(
                new BottomRow(
                    _lang["team.Label.Modifiers"],
                    false.ToString(), "", "", 0
                )
            );

            BuildMemberRows(member, vm, culture);
            return vm;
        }
        public BottomDevVm BuildMemberBottomDevVm(int subPos, TeamMemberDto member, int[] usedPoints, string culture)
        {
            (string headerType, int skilltype) = subPos == 1 ? ("Sec", 4) : ("3rd", 8);

            var vm = new BottomDevVm
            {
                UsedPoints = usedPoints,
                AvailableDevPoints = member.SkillPoints - usedPoints.Sum(),
                HeaderText = _lang[$"team.Label.Attitude.{headerType}"],
                //ListType = headerType,
                ResetButtonText = usedPoints.Sum() > 0 ? _lang["team.Button.Modify"].FormatSafe(usedPoints.Sum()) : ""
            };
            // 2) a megfelelő attitude DTO-t kiválasztjuk
            var attitude = subPos == 1 ? member.SecondAttitude : member.GenderAttitude;

            // 3) sorok létrehozása
            for (int i = 0; i < attitude.Skill.Length; i++)
            {
                vm.Rows.Add(
                    THelpers.BuildDevRow(
                        attitude.Skill[i],
                        attitude.Category[i],
                        skilltype + i,
                        member.Level,
                        usedPoints[i],
                        vm.AvailableDevPoints,
                        culture,
                        _lang
                    )
                );
            }

            return vm;
        }

        private static void BuildMemberRows(TeamMemberDto mem, BottomBlockVm vm, string culture)
        {
            int lvl = mem.Level;
            //int skill = mem.SkillPoints;
            string levelShort = THelpers.Right(RankNameTable.Data[lvl].PublicLevel ?? "", 2);
            var mainAt = mem.MaintAttitude;
            var secAt = mem.SecondAttitude;
            var genAt = mem.GenderAttitude;
            // - értékű sorok
            vm.Rows.Add(BuildMainSkillRow(0, mainAt.Category[0], mem.Level, levelShort, culture));
            vm.Rows.Add(BuildMainSkillRow(2, mainAt.Category[2], mem.Level, levelShort, culture));

            vm.Rows.Add(BuildSkillRow(secAt.Skill[0], secAt.Category[0], 0, culture));
            vm.Rows.Add(BuildSkillRow(secAt.Skill[2], secAt.Category[2], 2, culture));
            vm.Rows.Add(BuildSkillRow(genAt.Skill[0], genAt.Category[0], 4, culture));
            vm.Rows.Add(BuildSkillRow(genAt.Skill[2], genAt.Category[2], 6, culture));

            // + értékű sorok
            vm.Rows.Add(BuildMainSkillRow(1, mainAt.Category[1], mem.Level, levelShort, culture));
            vm.Rows.Add(BuildMainSkillRow(3, mainAt.Category[3], mem.Level, levelShort, culture));

            // Hátsó sorok
            vm.Rows.Add(BuildSkillRow(secAt.Skill[1], secAt.Category[1], 1, culture));
            vm.Rows.Add(BuildSkillRow(secAt.Skill[3], secAt.Category[3], 3, culture));
            vm.Rows.Add(BuildSkillRow(genAt.Skill[1], genAt.Category[1], 5, culture));
            vm.Rows.Add(BuildSkillRow(genAt.Skill[3], genAt.Category[3], 7, culture));
        }
        private static BottomRow BuildSkillRow(SkillPartial skill, int category, int modifier, string culture)
        {
            double val = ModifierTable.Data[skill.LvlCurrent].Modifier[modifier] ?? 0.0;
            string prefix = val > 0 ? "+" : "";

            return new BottomRow(
                CategoryNameLocalizer.GetCategory(category, culture),
                "",
                prefix + TeamHelper.FormatOneDecimal(val, false),
                skill.SkillCanDev ? "team.Label.Remark.Develop" : string.Empty,
                modifier + 1
            );
        }
        private static BottomRow BuildMainSkillRow(int skillno, int cat, int level, string levelshort, string culture)
        {
            double val = ModifierTable.DataMainSkill[skillno].StartValue + (level - 1) * ModifierTable.DataMainSkill[skillno].StepValue;
            string prefix = val > 0 ? "+" : "";
            return new BottomRow(
                CategoryNameLocalizer.GetCategory(cat, culture),
                levelshort,
                prefix + TeamHelper.FormatOneDecimal(val, false),
                "",
                0
                );
        }
    }
}
