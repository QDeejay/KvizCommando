using KvizCommando.Client.Data;
using KvizCommando.Client.Helpers;
using KvizCommando.Client.Models.ViewModels;
using KvizCommando.Client.Pages.Team;
using KvizCommando.Client.Services.Visual.UiService.Language;
using KvizCommando.Shared.Models;
using KvizCommando.Shared.Models.Dtos;

namespace KvizCommando.Client.Features.Team
{
    public class TDataBuilderTeam
    {
        private readonly ILanguageService _lang;

        public TDataBuilderTeam(ILanguageService lang)
        {
            _lang = lang;
        }
        public UpperBlockVm BuildTeamHeaderView(TeamExtendedInfo info, int usedSkillPoints, string culture)
        {
            var vm = new UpperBlockVm();

            // Közös adatok
            string name = info.Name;
            string rank = RankNameLocalizer.GetTeam(info.Level, culture);
            string publicLevel = RankNameTable.Data[info.Level].PublicLevel ?? "";
            string devPointsDisplay = (info.DevPts - usedSkillPoints).ToString();

            var t = info;
            string xp = info.Level < 22 ? $"{t.TeamXp}/{t.NextXpPts}" : _lang["team.Label.Remark.AtMaximum"];
            vm.Rows.Add(new(_lang["team.Label.Name"], name));
            vm.Rows.Add(new(_lang["team.Label.Org"], rank));
            vm.Rows.Add(new(_lang["team.Label.Level"], publicLevel));
            vm.Rows.Add(new(_lang["team.Label.TeamName"] + _lang[" Xp:"], xp));
            vm.Rows.Add(new(_lang["team.Label.Menbers"], $"{t.TotalMembers}/{t.MaxMembers}"));


            vm.Rows.Add(new(_lang["team.Label.Credit"], t.Credits.ToString()));
            vm.Rows.Add(new(_lang["team.label.TeamDevPointShort"], devPointsDisplay));
            vm.Rows.Add(new(_lang["team.Label.Bonus"], $"{t.TeamBonus}%"));
            vm.Rows.Add(new("", ""));
            vm.Rows.Add(new("", ""));

            return vm;
        }
        public BottomBlockVm BuildTeamView(TeamDtos team, string culture)
        {
            var vm = new BottomBlockVm();

            vm.Rows.Add(
                new BottomRow(
                    _lang["team.Label.TeamName"],
                    true.ToString(), "", "", 0
                )
            );

            BuildTeamRows(team, vm, culture);
            return vm;
        }
        public BottomDevVm BuildDevTeamView(TeamExtendedInfo info, int[] usedPoints, HelpDto help, string culture)
        {
            string headerType = "Help";
            int skilltype = 12;
            var vm = new BottomDevVm
            {
                UsedPoints = usedPoints,
                AvailableDevPoints = info.DevPts - usedPoints.Sum(),
                HeaderText = _lang["team.Label.Attitude.Help"],
                ListType = headerType,
                SaveButtonText = _lang["team.Button.Modify"] + (usedPoints.Sum() > 0 ? $" ({usedPoints.Sum()})" : "")
            };

            for (int i = 0; i < help.Skill.Length; i++)
            {
                vm.Rows.Add(
                    THelpers.BuildDevRow(
                        help.Skill[i],
                        help.Category[i],
                        skilltype + i,
                        info.Level,
                        usedPoints[i],
                        vm.AvailableDevPoints,
                        culture,
                        _lang
                    )
                );
            }

            return vm;
        }

        private void BuildTeamRows(TeamDtos input, BottomBlockVm vm, string culture)
        {
            foreach (int j in Enumerable.Range(1, 8))
            {
                if (input.CharCatMask[j])
                {
                    var mem = input.TeamMembers[j] ?? new TeamMemberDto();

                    vm.Rows.Add(new BottomRow(
                        mem.MemberName,
                        "<" + OrientationLocalizer.GetOrientShort(j, culture) + ">",
                        RankNameTable.Data[mem.MemberLvl].PublicLevel ?? "",
                        (int)mem.Remark == 0 ? string.Empty : _lang[$"team.modal.Button.{mem.Remark}"],
                        (int)mem.Remark > 10 ? (int)mem.Remark + j : j
                    ));
                }
            }
        }

    }

    

}