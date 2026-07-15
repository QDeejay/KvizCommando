using KvizCommando.Client.Data;
using KvizCommando.Client.Helpers;
using KvizCommando.Client.Models.ViewModels.Ui;
using KvizCommando.Client.Pages.Team.ViewModels;
using KvizCommando.Client.Services.Visual.UiService.Language;
using KvizCommando.Shared.Models;
using KvizCommando.Shared.Models.Dtos;
using System.Globalization;

namespace KvizCommando.Client.Pages.Team
{
    internal sealed class THelpers
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
            ILanguageService lang)
        {
            double actVal = ModifierTable.Data[skill.LvlCurrent].Modifier[modifier - 4] ?? 0.0;
            double devVal = ModifierTable.Data[Math.Min(skill.LvlCurrent + developed, skill.LvlCurMax)].Modifier[modifier - 4] ?? 0.0;

            string pre = devVal > 4 && modifier < 12 ? "+" : string.Empty;

            int startLevel = RankConstants.startLevels[modifier]; // sLevels refaktorálva

            return new BottomDevRow(
                CategoryNameLocalizer.GetCategory(category, culture),
                $"{skill.LvlCurrent + developed}/{skill.LvlCurMax}",
                category != 101 && category != 103 ? pre + FormatOneDecimal(devVal, category > 100) : string.Empty,
                actVal != devVal,
                availableDevPoints > 0 && skill.LvlCurrent + developed < skill.LvlCurMax,
                (actLevel < startLevel
                ? RankNameTable.Data[startLevel].PublicLevel
                : skill.LvlCurrent == skill.LvlOvrMax
                ? lang["team.Label.Remark.AtMaximum"]
                : string.Empty) ?? ""
            );
        }

        internal static string FormatOneDecimal(double value, bool percent)
        {
            if (percent)
            {
                return value.ToString() + "%";
            }
            else
            {
                double truncated = Math.Truncate(value * 10) / 10.0;
                return truncated.ToString("0.0", CultureInfo.InvariantCulture) + "s";
            }

        }
        internal static (int, int, int[]) RecruitResolver(int member, int candidate)
        {
            string m = RecruitData.OrientKeys[member - 1];

            bool c = candidate == 1 || candidate == 3 || candidate == 5 || candidate == 7 ? true : false;
            candidate--;
            bool[] mask = RecruitData.RecruitMask[candidate / 2];
            int[] idx =
            {
                int.Parse(m[0].ToString()),
                int.Parse(m[1].ToString()),
                int.Parse(m[2].ToString()),
                int.Parse(m[3].ToString()),
                int.Parse(m[4].ToString()),
                int.Parse(m[5].ToString()),
                int.Parse(m[6].ToString()),
                int.Parse(m[7].ToString()),
            };

            return (idx[0], c ? idx[2] : idx[4],
                new int[]
                {
                    idx[0] + (mask[0] ? 8:0),
                    (c ? idx[2] : idx[4]) + (mask[0] ? 8:0),
                    idx[6] + (mask[4] ? 8:0),
                    idx[1] + (mask[1] ? 8:0),
                    (c ? idx[3] : idx[5]) + (mask[1] ? 8:0),
                    idx[6] + (mask[5] ? 8:0)
                });
        }
        internal static List<SubHeaderVm> SubHeaderResolver(bool[] masks, string cult)
        {
            var list = new List<SubHeaderVm>();
            int index = 0;
            foreach (var mask in masks)
            {
                index++;
                list.Add(new SubHeaderVm
                {
                    Text = OrientationLocalizer.GetOrientation(index, cult),
                    Enable = mask,
                    Visible = mask,
                    ClickId = index,
                });
            }
            return list;
        }

    }
}
