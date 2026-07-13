using KvizCommando.Client.Data;
using KvizCommando.Client.Helpers;
using KvizCommando.Client.Models.ViewModels;
using KvizCommando.Client.Services.Visual.UiService.Language;
using KvizCommando.Shared.Models.Dtos;
using KvizCommando.Shared.Models.Enums;

namespace KvizCommando.Client.Features.Team
{
    public class TBuilderTeam
    {
        private readonly ILanguageService _lang;
        public TBuilderTeam(ILanguageService lang)
        {
            _lang = lang;
        }
        public UpperBlockVm BuildTeamUpperVm(TeamExtendedInfo info, string culture)
        {
            var vm = new UpperBlockVm();

            // Közös adatok
            string name = info.Name;
            string rank = RankNameLocalizer.GetTeam(info.Level, culture);
            string publicLevel = RankNameTable.Data[info.Level].PublicLevel ?? "";
            string devPointsDisplay = info.DevPoints.ToString();

            var t = info;
            string xp = info.Level < 22 ? $"{t.Xp}/{t.NextXp}" : _lang["team.Label.Remark.AtMaximum"];
            vm.Rows.Add(new(_lang["team.Label.Name"], name));
            vm.Rows.Add(new(_lang["team.Label.Org"], rank));
            vm.Rows.Add(new(_lang["team.Label.Level"], publicLevel));
            vm.Rows.Add(new(_lang["team.Label.TeamName"] + " Xp:", xp));
            vm.Rows.Add(new("", ""));


            vm.Rows.Add(new(_lang["team.Label.Credit"], t.Credits.ToString()));
            vm.Rows.Add(new(_lang["team.label.TeamDevPointShort"], devPointsDisplay));
            vm.Rows.Add(new(_lang["team.Label.Bonus"], $"{t.Bonus}%"));
            vm.Rows.Add(new(_lang["team.Label.Menbers"], $"{t.TotalMembers}/{t.MaxMembers}"));
            vm.Rows.Add(new("", ""));


            return vm;
        }
        public BottomBlockVm BuildTeamBottomVm(TeamMemberDto[] members, string culture)
        {
            var vm = new BottomBlockVm();

            vm.Rows.Add(
                new BottomRow(
                    _lang["team.Label.TeamName"],
                    true.ToString(), "", "", 0
                )
            );

            BuildTeamRows(members, vm, culture);

            return vm;
        }
        public BottomDevVm BuildTeamBottomDevVm(TeamExtendedInfo info, int[] usedPoints, HelpDto help, string culture)
        {
            //string headerType = "Help";
            int skilltype = 12;
            var vm = new BottomDevVm
            {
                UsedPoints = usedPoints,
                AvailableDevPoints = info.DevPoints - usedPoints.Sum(),
                HeaderText = _lang["team.Label.Attitude.Help"],
                //ListType = "Help",
                ResetButtonText = usedPoints.Sum() > 0 ? _lang["team.Button.Modify"].FormatSafe(usedPoints.Sum()) : ""
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
        private void BuildTeamRows(TeamMemberDto[] members, BottomBlockVm vm, string culture)
        {
            foreach (int j in Enumerable.Range(1, 8))
            {
                //if (input.CharCatMask[j])
                if (members[j] is not null)
                {
                    var mem = members[j];

                    vm.Rows.Add(new BottomRow(
                        mem.Name,
                        "<" + OrientationLocalizer.GetOrientShort(j, culture) + ">",
                        RankNameTable.Data[mem.Level].PublicLevel ?? "",
                        (int)mem.Remark < 100
                            ? (int)mem.Remark >= 50 ? _lang["team.Label.Remark.Develop"] : string.Empty
                            : _lang[$"team.modal.Button.{mem.Remark}"],
                        (int)mem.Remark >= (int)MembRemark.Develop ? (int)mem.Remark + j : j
                    ));
                }
            }
        }
    }
}