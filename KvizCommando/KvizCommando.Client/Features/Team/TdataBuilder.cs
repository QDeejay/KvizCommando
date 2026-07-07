using KvizCommando.Client.Data;
using KvizCommando.Client.Helpers;
using KvizCommando.Client.Models.ViewModels;
using KvizCommando.Client.Services.Visual.UiService.Language;
using KvizCommando.Shared.Models.Dtos;

namespace KvizCommando.Client.Features.Team
{
    public class TdataBuilder
    {
        public static UpperBlockViewModel BuildTeamHeader(TeamExtendedInfo info, int usedSkillPoints, string culture)
        {
            var vm = new UpperBlockViewModel();

            // Közös adatok
            string name = info.Name;
            string rank = RankNameLocalizer.GetTeam(info.Level, culture);
            string publicLevel = RankNameTable.Data[info.Level].PublicLevel ?? "";
            string devPointsDisplay = (info.DevPts - usedSkillPoints).ToString();

            var t = info;
            string xp = info.Level < 22 ? $"{t.TeamXp}/{t.NextXpPts}" : "team.Label.Remark.AtMaximum";
            vm.Rows.Add(new("team.Label.Name", name));
            vm.Rows.Add(new("team.Label.Org", rank));
            vm.Rows.Add(new("team.Label.Level", publicLevel));
            vm.Rows.Add(new("team.Label.TeamName" + " Xp:", xp));
            vm.Rows.Add(new("team.Label.Menbers", $"{t.TotalMembers}/{t.MaxMembers}"));


            vm.Rows.Add(new("team.Label.Credit", t.Credits.ToString()));
            vm.Rows.Add(new("team.label.TeamDevPointShort", devPointsDisplay));
            vm.Rows.Add(new("team.Label.Bonus", $"{t.TeamBonus}%"));
            vm.Rows.Add(new("", ""));
            vm.Rows.Add(new("", ""));

            return vm;
        }
        public static UpperBlockViewModel BuildMemberHeader(TeamMemberDto info, int usedSkillPoints, string culture, ILanguageService lang)
        {
            var vm = new UpperBlockViewModel();

            // Közös adatok
            string name = info.Name;
            string publicLevel = RankNameTable.Data[info.Level].PublicLevel ?? "";
            string devPointsDisplay = (info.DevPts - usedSkillPoints).ToString();

            var m = info;
            int c1 = NormalizeCategory(m.MaintAttitude.Category[0]);
            int c2 = NormalizeCategory(m.SecondAttitude.Category[0]);
            int vitMax = 36 + info.Level * 3;
            int vitAct = Math.Min(m.EnergyPoints, vitMax);
            int rankClass = info.Level == 0 ? 0 : (info.Level - 1) / 3 + 1;
            string o1 = OrientationLocalizer.GetOrientation(c1, culture);
            string o2 = OrientationLocalizer.GetOrientation(c2, culture);
            string oShort = OrientationLocalizer.GetOrientShort(c1, culture);

            vm.Rows.Add(new(lang["team.Label.Name"], name));
            vm.Rows.Add(new(lang["team.Label.Rank"], RankNameLocalizer.GetName(info.Level, culture)));
            vm.Rows.Add(new(lang["team.Label.Class"] + ":", RankNameLocalizer.GetClass(rankClass, culture)));
            vm.Rows.Add(new(lang["team.SubBtn.Main"] + ":", o1));
            vm.Rows.Add(new(lang["team.SubBtn.Second"] + ":", o2));


            vm.Rows.Add(new(lang["team.Label.Level"], publicLevel));
            vm.Rows.Add(new(lang["team.Label.Vitality"], $"{vitAct}/{vitMax}"));
            vm.Rows.Add(new(lang["team.Label.Next"], (info.NextXpPts - info.Xp).ToString()));
            vm.Rows.Add(new(lang["team.Label.SkillPointShort"].FormatSafe(oShort), devPointsDisplay));
            vm.Rows.Add(new(lang["team.Label.Pension"], m.Pension.ToString()));


            return vm;
        }
        
        
        
        private static int NormalizeCategory(int cat)
        {
            return cat > 8 ? cat - 8 : cat;
        }
    }
}
