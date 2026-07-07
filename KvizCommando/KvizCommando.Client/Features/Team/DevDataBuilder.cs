
/*
 *
 *
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
        public static BottomDevVm BuildTeam(TeamExtendedInfo info, int[] usedPoints, HelpDto help, string culture)
        {
            string headerType = "Help";
            int skilltype = 12;
            var vm = new BottomDevVm
            {
                UsedPoints = usedPoints,
                AvailableDevPoints = info.DevPts - usedPoints.Sum(),
                HeaderText = "team.Label.Attitude.Help",
                ListType = headerType,
                SaveButtonText = usedPoints.Sum() > 0 ? $" ({usedPoints.Sum()})" : ""
            };

            for (int i = 0; i < help.Skill.Length; i++)
            {
                vm.Rows.Add(
                    BuildDevRow(
                        help.Skill[i],
                        help.Category[i],
                        skilltype+i,
                        info.Level,
                        usedPoints[i],
                        vm.AvailableDevPoints,
                        culture
                    )
                );
            }

            return vm;
        }

        public static BottomDevVm BuildMember(int subPos, TeamMemberDto info, int[] usedPoints, string culture, ILanguageService lang)
        {
            (string headerType, int skilltype) = subPos == 1 ? ("Sec", 4) : ("3rd", 8);

            var vm = new BottomDevVm
            {
                UsedPoints = usedPoints,
                AvailableDevPoints = info.DevPts - usedPoints.Sum(),
                HeaderText = $"team.Label.Attitude.{headerType}",
                ListType = headerType,
                SaveButtonText = usedPoints.Sum() > 0 ? $" ({usedPoints.Sum()})" : ""
            };
            // 2) a megfelelő attitude DTO-t kiválasztjuk
            var attitude = subPos == 1 ? info.SecondAttitude : info.GenderAttitude;

            // 3) sorok létrehozása
            for (int i = 0; i < attitude.Skill.Length; i++)
            {
                vm.Rows.Add(
                    BuildDevRow(
                        attitude.Skill[i],
                        attitude.Category[i],
                        skilltype + i,
                        info.Level,
                        usedPoints[i],
                        vm.AvailableDevPoints,
                        culture
                    )
                );
            }

            return vm;
        }
       
        
    }

}
 *
  private static  (string, int) ResolveHeaderType(int tabPos, IGeneralInfo info)
        {
            if (info is TeamExtendedInfo) 
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
            if (info is TeamExtendedInfo)
                return help;

            if (info is TeamMemberDto m)
                return tabPos == 1 ? m.SecondAttitude : m.GenderAttitude;

            throw new Exception("Unknown type");
        }

 
 *
 *
  // 1) tab váltás logikája (korábban a komponensben volt)
            (string headerType, int skilltype) = ResolveHeaderType(tabPos, info);
 * 
 * 
   public DevViewModel BuildTeam(int tabPos, IGeneralInfo info, int[] usedPoints, HelpDto help, string culture)
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
 
 */