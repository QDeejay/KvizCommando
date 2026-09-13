using KvizCommando.Client.Data;
using KvizCommando.Client.Features.Team;
using KvizCommando.Client.Helpers;
using KvizCommando.Client.Models.ViewModels;
using KvizCommando.Shared.Models;
using KvizCommando.Shared.Models.Dtos;
using KvizCommando.Shared.Models.Rules;

namespace KvizCommando.Client.Features.Shared.Modal.Builders
{
    partial class TBuilderModal
    {
        /// <summary>
        /// Összeállítja a karakter nyugdíjazásához tartozó modális nézetmodellt.
        /// </summary>
        /// <param name="member">A megjelenítendő csapattag adatai.</param>
        /// <param name="culture">A kért kultúra neve, például <c>hu-HU</c>.</param>
        public ModalRetireVm BuildRetireVm(TeamMemberDto member, string culture)
        {
            var vm = new ModalRetireVm();
            var bi = BasicInfoResolver(member);


            int newLevel = 31;
            int newRc = 11;
            vm.Info = BuildInfoRow(bi, 0, culture, _lang);
            vm.Infotext1 = _lang["modal.team.Retire.Description"];
            vm.Unlocks = _lang["modal.team.Reward.Title"];
            vm.UnlocksLevel = (RankNameTable.Data[newLevel].PublicLevel ?? "") + ": ";
            vm.UnlocksRank = RankNameLocalizer.GetName(newLevel, culture);
            vm.RankClass = RankNameLocalizer.GetClass(newRc, culture);
            vm.RankClassChanged = true;
            vm.Rows.Add(new ModalRow(
                CategoryName: _lang["modal.team.Label.Pension"],
                ValueDisplay: string.Empty,
                separator: UNLOCK_SEP,
                ValueChangeDisplay: "+" + member.Pension.ToString(),
                color: "color: green;"
                ));
            vm.Rows.Add(new ModalRow(
                CategoryName: _lang["modal.team.Label.TeamDevelopmentPoint"],
                ValueDisplay: string.Empty,
                separator: UNLOCK_SEP,
                ValueChangeDisplay: "+" + RankRewards.List[
                    TeamRules.RETIRE_REWARD_RANK].DevPointToStore.ToString(),
                color: "color: green;"
                ));

            return vm;
        }
        /// <summary>
        /// Összeállítja a karakter kezeléséhez tartozó modális nézetmodellt.
        /// </summary>
        /// <param name="member">A megjelenítendő csapattag adatai.</param>
        /// <param name="culture">A kért kultúra neve, például <c>hu-HU</c>.</param>
        public ModalHandleVm BuildHandleVm(TeamMemberDto member, string culture)
        {
            var vm = new ModalHandleVm();
            var bi = BasicInfoResolver(member);

            vm.Info = BuildInfoRow(bi, 0, culture, _lang);
            vm.Infotext1 = _lang["modal.team.Handle.Attention"];
            vm.Infotext2 = _lang["modal.team.Handle.NoVitality"];
            vm.Infotext3 = _lang["modal.team.Handle.HireCooldown"];
            if (member.SkillPoints == 0)
                vm.Infotext4 = _lang["modal.team.Handle.RestoreRequirement", vm.Info.Devpoints[0..7]];
            else
                vm.Infotext4 = string.Empty;
            return vm;
        }
    }
}
