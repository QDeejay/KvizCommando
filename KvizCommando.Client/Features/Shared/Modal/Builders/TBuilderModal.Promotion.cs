using KvizCommando.Client.Data;
using KvizCommando.Client.Features.Team;
using KvizCommando.Client.Helpers;
using KvizCommando.Client.Models.ViewModels;
using KvizCommando.Client.Services.Visual.UiService.Language;
using KvizCommando.Shared.Models;
using KvizCommando.Shared.Models.Dtos;
using KvizCommando.Shared.Models.Rules;

namespace KvizCommando.Client.Features.Shared.Modal.Builders
{
    partial class TBuilderModal
    {
        /// <summary>
        /// Összeállítja a csapat előléptetéséhez tartozó modális nézetmodellt.
        /// </summary>
        /// <param name="info">A nézetmodellhez szükséges csapatadatok.</param>
        /// <param name="help">A nézetmodellhez szükséges segítségadatok.</param>
        /// <param name="culture">A kért kultúra neve, például <c>hu-HU</c>.</param>
        public ModalTeamPromoteVm BuildTeamPromoteVm(
            TeamExtendedInfo info,
            HelpDto help,
            string culture)
        {
            int newLevel = info.Level + 1;

            var vm = new ModalTeamPromoteVm();
            var bi = new BasicInfo()
            {
                IsMember = false,
                Name = info.Name,
                Devpoints = info.DevPoints.ToString(),
                Level = info.Level,
            };
            int addDevPoints = RankRewards.List[newLevel].DevPointToStore;

            int newBonus =
                RankRewards.List[newLevel].WinBonus > RankRewards.List[bi.Level].WinBonus
                    ? RankRewards.List[newLevel].WinBonus
                    : 0;

            int newTeamSize =
                RankRewards.List[newLevel].MaxCharacters > RankRewards.List[bi.Level].MaxCharacters
                    ? RankRewards.List[newLevel].MaxCharacters
                    : 0;
            int oldLoadOutSize = bi.Level > 0 ? RankRewards.List[bi.Level].MaxCharacters * 2 : 0;
            int newLoadOutSize = newTeamSize > 0 && newTeamSize <= 5 ? newTeamSize * 2 : 0;

            int newOwnSlotSize =
              RankRewards.List[newLevel].OwnQuestSlot > RankRewards.List[bi.Level].OwnQuestSlot
                  ? RankRewards.List[newLevel].OwnQuestSlot
                  : 0;
            int newExtra = (RankRewards.List[newLevel].HelpRewardNo >= 200 ? RankRewards.List[newLevel].HelpRewardNo : 0) ?? 0;

            vm.Info = BuildInfoRow(bi, addDevPoints, culture, _lang);

            vm.Unlocks = _lang["team.modal.Label.Unlocks"];
            vm.UnlocksLevel = (RankNameTable.Data[newLevel].PublicLevel ?? "") + ": ";
            vm.UnlocksOrg = RankNameLocalizer.GetTeam(newLevel, culture);
            vm.UnlockExtras = newExtra > 0 ? _lang["team.modal.Label.UnlocksExtras"] : string.Empty;
            vm.UnlockHelps = _lang["team.modal.Label.UnlocksHelps"];

            vm.Rows.Add(newExtra >= 200
                ? new ModalRow(
                    CategoryName: CategoryNameLocalizer.GetCategory(newExtra, culture),
                    ValueDisplay: string.Empty,
                    separator: string.Empty,
                    ValueChangeDisplay: string.Empty,
                    color: string.Empty)
                : new ModalRow(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty));

            if (newBonus > 0)
                vm.Rows.Add(new ModalRow(
                    CategoryName: _lang["team.modal.Label.Bonus"],
                    ValueDisplay: $"{RankRewards.List[bi.Level].WinBonus}%",
                    separator: UNLOCK_SEP,
                    ValueChangeDisplay: $"{newBonus}%",
                    color: "green"
                    ));

            if (addDevPoints > 0)
                vm.Rows.Add(new ModalRow(
                    CategoryName: _lang["team.label.TeamDevPoint"],
                    ValueDisplay: string.Empty,
                    separator: UNLOCK_SEP,
                    ValueChangeDisplay: "+" + addDevPoints,
                    color: "green"
                    ));

            if (newTeamSize > 0)
                vm.Rows.Add(new ModalRow(
                    CategoryName: _lang["team.modal.Label.TeamSize"],
                    ValueDisplay: $"{RankRewards.List[bi.Level].MaxCharacters}",
                    separator: UNLOCK_SEP,
                    ValueChangeDisplay: $"{newTeamSize}",
                    color: "green"
                    ));

            if (newLoadOutSize > 0)
                vm.Rows.Add(new ModalRow(
                    CategoryName: _lang["team.modal.Label.LoadoutSize"],
                    ValueDisplay: $"{oldLoadOutSize}",
                    separator: UNLOCK_SEP,
                    ValueChangeDisplay: $"{newLoadOutSize}",
                    color: "green"
                    ));

            if (newOwnSlotSize > 0)
                vm.Rows.Add(new ModalRow(
                    CategoryName: _lang["team.modal.Label.OwnSlotSize"],
                    ValueDisplay: $"{RankRewards.List[bi.Level].OwnQuestSlot}",
                    separator: UNLOCK_SEP,
                    ValueChangeDisplay: $"{newOwnSlotSize}",
                    color: "green"
                    ));

            vm.StartOfHelps = vm.Rows.Count;

            HelpLineResolver(help, vm, RankConstants.startLevels[12..16], RankConstants.maxLevels[12..16], newLevel, culture);

            return vm;
        }
        /// <summary>
        /// Összeállítja a karakter előléptetéséhez tartozó modális nézetmodellt.
        /// </summary>
        /// <param name="member">A megjelenítendő csapattag adatai.</param>
        /// <param name="culture">A kért kultúra neve, például <c>hu-HU</c>.</param>
        public ModalPromoteVm BuildPromoteVm(TeamMemberDto member, string culture)
        {
            var vm = new ModalPromoteVm();
            var bi = BasicInfoResolver(member);
            int newLevel = Math.Min(bi.Level + 1, 21);
            int newRc = (newLevel - 1) / 3 + 1;
            bool rankClassChanged =
                TeamRules.IsRankClassChangingPromotion(bi.Level);
            int addDevPoints = RankRewards.List[newLevel].DevPointRevard;
            vm.Info = BuildInfoRow(bi, addDevPoints, culture, _lang);

            vm.Unlocks = _lang["team.Label.Attitude.Mai"];
            vm.UnlocksLevel = (RankNameTable.Data[newLevel].PublicLevel ?? "") + ": ";
            vm.UnlocksRank = RankNameLocalizer.GetName(newLevel, culture);
            vm.RankClass = RankNameLocalizer.GetClass(newRc, culture);
            vm.RankClassChanged = rankClassChanged;
            vm.Infotext1 = rankClassChanged
                ? _lang["team.modal.Text.Promote2"]
                : _lang["team.modal.Text.Promote1"].FormatSafe(
                    TeamRules.PROMOTION_TEAM_DEV_POINT_COST);
            vm.UnlockMaxLevels1 = _lang["team.Label.Attitude.Sec"] + " (max)";
            vm.UnlockMaxLevels2 = _lang["team.Label.Attitude.3rd"] + " (max)";
            vm.Rows.Add(rankClassChanged ? new ModalRow(
                CategoryName: _lang["team.label.TeamDevPoint"],
                ValueDisplay: string.Empty,
                separator: UNLOCK_SEP,
                ValueChangeDisplay: "+" + RankRewards.List[newLevel].DevPointToStore.ToString(),
                color: "green"
                )
                : new ModalRow(
                    CategoryName: string.Empty,
                    ValueDisplay: string.Empty,
                    separator: string.Empty,
                    ValueChangeDisplay: string.Empty,
                    color: string.Empty
                    ));
            vm.Rows.Add(new ModalRow(
                CategoryName: _lang["team.Label.Vitality"][0..(_lang["team.Label.Vitality"].Length - 1)] + " maximum",
                ValueDisplay: $"{member.EnergyPoints}/{TeamRules.GetMemberMaxVitality(member.Level)}",
                separator: UNLOCK_SEP,
                ValueChangeDisplay: $"{TeamRules.GetMemberMaxVitality(newLevel)}/{TeamRules.GetMemberMaxVitality(newLevel)}",
                color: ""
                ));
            AttitudeLineResolver(member.MaintAttitude, vm, RankConstants.startLevels[0..4], newLevel, culture);
            AttitudeLineResolver(member.SecondAttitude, vm, RankConstants.startLevels[4..8], newLevel, culture, [0, 1, 0, 1]);
            AttitudeLineResolver(member.GenderAttitude, vm, RankConstants.startLevels[8..12], newLevel, culture, [0, 1, 0, 1]);
            return vm;
        }
    }
}
