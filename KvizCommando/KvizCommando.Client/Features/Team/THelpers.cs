using KvizCommando.Client.Data;
using KvizCommando.Client.Helpers;
using KvizCommando.Client.Models.ViewModels;
using KvizCommando.Client.Pages.Team;
using KvizCommando.Client.Services.Visual.UiService.Language;
using KvizCommando.Shared.Models;
using KvizCommando.Shared.Models.Dtos;

namespace KvizCommando.Client.Features.Team
{
    internal static class THelpers
    {
        internal static int NormalizeCategory(int cat)
        {
            return cat > 8 ? cat - 8 : cat;
        }
        internal static string Right(string s, int count)
        {
            if (string.IsNullOrEmpty(s)) return s;
            if (s.Length <= count) return s;
            return "<" + s.Substring(s.Length - count) + ">";
        }
        internal static BottomDevRow BuildDevRow(
            SkillPartial skill,
            int category,
            int modifier,
            int actLevel,
            int developed,
            int availableDevPoints,            
            string culture,
            ILanguageService lang
        )
        {
            double actVal = ModifierTable.Data[skill.LvlCurrent].Modifier[modifier - 4] ?? 0.0;
            double devVal = ModifierTable.Data[Math.Min(skill.LvlCurrent + developed, skill.LvlCurMax)].Modifier[modifier - 4] ?? 0.0;

            string pre = devVal > 4 && modifier < 12 ? "+" : string.Empty;

            int startLevel = RankConstants.startLevels[modifier]; // sLevels refaktorálva

            return new BottomDevRow(
                CategoryNameLocalizer.GetCategory(category, culture),
                $"{skill.LvlCurrent + developed}/{skill.LvlCurMax}",
                category != 101 && category != 103 ? pre + TeamHelper.FormatOneDecimal(devVal, category > 100) : string.Empty,
                actVal != devVal,
                availableDevPoints > 0 && skill.LvlCurrent + developed < skill.LvlCurMax,
                (actLevel < startLevel
                ? RankNameTable.Data[startLevel].PublicLevel
                : skill.LvlCurrent == skill.LvlOvrMax
                ? lang["team.Label.Remark.AtMaximum"]
                : string.Empty) ?? ""
            );
        }
    }
}
