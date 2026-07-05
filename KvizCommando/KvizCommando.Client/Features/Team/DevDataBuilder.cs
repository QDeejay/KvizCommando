using KvizCommando.Client.Data;
using KvizCommando.Client.Helpers;
using KvizCommando.Client.Pages.Team;
using KvizCommando.Shared.Models.Dtos;
using KvizCommando.Client.Models.ViewModels;
using KvizCommando.Shared.Models;
using KvizCommando.Client.Services.Visual.UiService.Language;

namespace KvizCommando.Client.Features.Team
{
    public class DevDataBuilder
    {
        private readonly ILanguageService _lang;

        public DevDataBuilder(ILanguageService lang)
        {
            _lang = lang;
        }
  
        public DevViewModel Build(int tabPos, IGeneralInfo info, int[] usedPoints, HelpDto help, string culture)
        {
            var model = new DevViewModel();
            
            model.UsedPoints = usedPoints;
            model.AvailableDevPoints = info.DevPts - usedPoints.Sum();

            // 1) tab váltás logikája (korábban a komponensben volt)
            (string headerType, int skilltype) = ResolveHeaderType(tabPos, info);

            model.HeaderText = _lang[$"team.Label.Attitude.{headerType}"];
            model.ListType = headerType;
            model.SaveButtonText = _lang["team.Button.Modify"] + (usedPoints.Sum() > 0 ? $" ({usedPoints.Sum()})" : "");
            // 2) a megfelelő attitude DTO-t kiválasztjuk
            var attitude = ResolveAttitude(info, tabPos, help);

            // 3) sorok létrehozása
            for (int i = 0; i < attitude.Skill.Length; i++)
            {
                model.Rows.Add(
                    BuildDevRow(
                        attitude.Skill[i],
                        attitude.Category[i],
                        skilltype+i,
                        info.Level,
                        usedPoints[i],
                        model.AvailableDevPoints,
                        culture
                    )
                );
            }

            return model;
        }

        private static  (string, int) ResolveHeaderType(int tabPos, IGeneralInfo info)
        {
            if (info is ExtendedInfo) 
            {
                return ("Help", 12);
            }


            if (info is TeamMemberDto)
            {
                return tabPos == 1 ? ("Sec",4) : ("3rd",8);
            }
                

            return ("Help", 12);
        }

        private static AttidtudeDto ResolveAttitude(IGeneralInfo info, int tabPos, HelpDto help)
        {
            if (info is ExtendedInfo)
                return help;

            if (info is TeamMemberDto m)
                return tabPos == 1 ? m.SecondAttitude : m.GenderAttitude;

            throw new Exception("Unknown type");
        }

        private DevRow BuildDevRow(
            SkillPartial skill,
            int category,
            int modifier,
            int actLevel,
            int developed,
            int availableDevPoints,
            string culture
        )
        {
            double actVal = ModifierTable.Data[skill.lvlCurrent].Modifier[modifier-4] ?? 0.0;
            double devVal = ModifierTable.Data[Math.Min(skill.lvlCurrent + developed, skill.lvlCurMax)].Modifier[modifier-4] ?? 0.0;

            string pre = devVal > 4 && modifier<12 ? "+" : string.Empty;

            int startLevel = RankConstants.startLevels[modifier]; // sLevels refaktorálva

            return new DevRow(
                CategoryNameLocalizer.GetCategory(category, culture),
                $"{skill.lvlCurrent + developed}/{skill.lvlCurMax}",
                category != 101 && category != 103 ? pre + TeamHelper.FormatOneDecimal(devVal, category > 100) : string.Empty,
                actVal != devVal,
                availableDevPoints > 0 && skill.lvlCurrent + developed < skill.lvlCurMax,
                (actLevel < startLevel 
                ? RankNameTable.Data[startLevel].PublicLevel 
                : skill.lvlCurrent == skill.lvlOvrMax 
                ? _lang["team.Label.Remark.AtMaximum"] 
                :string.Empty) ?? ""
            );
        }
    }

}
