using KvizCommando.Client.Data;
using KvizCommando.Client.Helpers;
using KvizCommando.Client.Models.ViewModels;
using KvizCommando.Client.Services.Visual.UiService.Language;
using KvizCommando.Shared.Models.Dtos;

namespace KvizCommando.Client.Features.Team
{
    public class UpperBlockDataBuilder
    {
        private readonly ILanguageService _lang;

        public UpperBlockDataBuilder(ILanguageService lang)
        {
            _lang = lang;
        }

        public UpperBlockViewModel Build(IGeneralInfo info, int usedSkillPoints, string culture)
        {
            var vm = new UpperBlockViewModel();

            // Közös adatok
            string name = info.Name;
            string publicLevel = RankNameTable.Data[info.Level].PublicLevel ?? "";
            string devPointsDisplay = (info.DevPts - usedSkillPoints).ToString();
         
            if (info is ExtendedInfo t)
            {
                string xp = info.Level < 22 ? $"{t.TeamXp}/{t.NextXpPts}" : _lang["team.Label.Remark.AtMaximum"];
                vm.Rows.Add(new(_lang["team.Label.TeamName"]+" "+_lang["team.Label.Name"], name));
                vm.Rows.Add(new(_lang["team.Label.Level"], publicLevel));
                vm.Rows.Add(new(_lang["team.Label.TeamName"] + " Xp:", xp));
                vm.Rows.Add(new(_lang["team.Label.Menbers"], $"{t.TotalMembers}/{t.MaxMembers}"));
                vm.Rows.Add(new("",""));
                
                vm.Rows.Add(new(_lang["team.Label.Credit"], t.Credits.ToString()));
                vm.Rows.Add(new(_lang["team.label.TeamDevPointShort"], devPointsDisplay));
                vm.Rows.Add(new(_lang["team.Label.Bonus"], $"{t.TeamBonus}%"));
                vm.Rows.Add(new("", ""));
                vm.Rows.Add(new("", ""));

            }

            if (info is TeamMemberDto m)
            {
                int c1 = NormalizeCategory(m.MaintAttitude.Category[0]);
                int c2 = NormalizeCategory(m.SecondAttitude.Category[0]);
                int vitMax = 36 + info.Level * 3;
                int vitAct = Math.Min(m.EnergyPoints, vitMax);
                int rankClass = info.Level == 0 ? 0 : (info.Level-1) / 3 + 1;
                string o1 = OrientationLocalizer.GetOrientation(c1, culture);
                string o2 = OrientationLocalizer.GetOrientation(c2, culture);
                string oShort = OrientationLocalizer.GetOrientShort(c1, culture);

                vm.Rows.Add(new(_lang["team.Label.Name"], name));
                vm.Rows.Add(new(_lang["team.Label.Rank"], RankNameLocalizer.GetName(info.Level, culture)));
                vm.Rows.Add(new(_lang["team.Label.Class"]+":", RankNameLocalizer.GetClass(rankClass, culture)));
                vm.Rows.Add(new(_lang["team.SubBtn.Main"] + ":", o1));
                vm.Rows.Add(new(_lang["team.SubBtn.Second"] + ":", o2));
                

                vm.Rows.Add(new(_lang["team.Label.Level"], publicLevel));
                vm.Rows.Add(new(_lang["team.Label.Vitality"], $"{vitAct}/{vitMax}"));
                vm.Rows.Add(new(_lang["team.Label.Next"], (info.NextXpPts - info.Xp).ToString()));
                vm.Rows.Add(new(_lang["team.Label.SkillPointShort"].FormatSafe(oShort), devPointsDisplay));
                vm.Rows.Add(new(_lang["team.Label.Pension"], m.Pension.ToString()));
            }

            return vm;
        }

        private int NormalizeCategory(int cat)
        {
            return cat > 8 ? cat - 8 : cat;
        }
    }
}
