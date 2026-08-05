using KvizCommando.Client.Data;
using KvizCommando.Client.Features.Team;
using KvizCommando.Client.Helpers;
using KvizCommando.Client.Models.ViewModels;
using KvizCommando.Client.Services.Visual.UiService.Language;
using KvizCommando.Shared.Models;
using KvizCommando.Shared.Models.Dtos;

namespace KvizCommando.Client.Features.Shared.Modal.Builders
{
    public sealed class TBuilderModal
    {
        private readonly ILanguageService _lang;
        public TBuilderModal(ILanguageService lang)
        {
            _lang = lang;
        }

        private const string UNLOCK_SEP = " => ";

        public ModalHireVm BuildHireVm(CandidateDto candidate, int hpos, int candno, string culture)
        {
            var vm = new ModalHireVm();
            var oriData = TeamHelpers.RecruitResolver(hpos, candno);
            //string orientkeys = RecruitData.OrientKeys[hpos - 1];
            var bi = new BasicInfo()
            {
                Name = candidate.Name[candno - 1] ?? string.Empty,
                Piccode = candidate.PictureCode[candno - 1] ?? string.Empty,
                Devpoints = "0",
                Orient1 = oriData.Item1,
                Orient2 = oriData.Item2,
                Level = 0
            };
            int[] orientcats = oriData.Item3;

            vm.Info = BuildInfoRow(bi, 0, culture, _lang);
            vm.Labelpros = _lang["team.modal.Label.Pros"];
            vm.Labelcons = _lang["team.modal.Label.Cons"];
            int index = -1;
            double val;
            string pref = string.Empty;
            foreach (int i in orientcats)
            {
                index++;
                if (index == 0)
                    val = -2 - 0.4 * (bi.Level - 1);
                else if (index == 3)
                    val = 10 - 0.4 * (bi.Level - 1);
                else
                    val = ModifierTable.Data[bi.Level].Modifier[ModalConstants.HireVal[index]] ?? 0.0;


                pref = val > 0 ? "+" : "";
                vm.Rows.Add(new ModalRow(
                        CategoryName: CategoryNameLocalizer.GetCategory(i, culture),
                        ValueDisplay: pref + TeamHelpers.FormatOneDecimal(val, false),
                        separator: string.Empty,
                        ValueChangeDisplay: string.Empty,
                        color: index < 3 ? "green" : "red"
                        ));
            }
            return vm;
        }

        public ModalTeamPromoteVm BuildTeamPromoteVm(
            TeamExtendedInfo info,
            HelpDto help,
            string culture,
            int achievedLevel = 0)
        {
            int newLevel = achievedLevel > 0
                ? achievedLevel
                : Math.Min(info.Level + 1, 30);
            int previousLevel = Math.Max(newLevel - 1, 0);

            var vm = new ModalTeamPromoteVm();
            var bi = new BasicInfo()
            {
                IsMember = false,
                Name = info.Name,
                Devpoints = info.DevPoints.ToString(),
                Level = previousLevel,
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
                    ValueDisplay: $"{RankRewards.List[bi.Level].MaxCharacters * 2}",
                    separator: UNLOCK_SEP,
                    ValueChangeDisplay: $"{newLoadOutSize}",
                    color: "green"
                    ));

            if (newOwnSlotSize > 0)
                vm.Rows.Add(new ModalRow(
                    CategoryName: _lang["team.modal.Label.OwnSlotSize"],
                    ValueDisplay: $"{RankRewards.List[bi.Level].OwnQuestSlot * 2}",
                    separator: UNLOCK_SEP,
                    ValueChangeDisplay: $"{newOwnSlotSize}",
                    color: "green"
                    ));

            vm.StartOfHelps = vm.Rows.Count;

            HelpLineResolver(help, vm, RankConstants.startLevels[12..16], RankConstants.maxLevels[12..16], newLevel, culture);

            return vm;
        }
        public ModalPromoteVm BuildPromoteVm(TeamMemberDto member, string culture)
        {
            var vm = new ModalPromoteVm();
            var bi = BasicInfoResolver(member);
            int rc = bi.Level == 0 ? 0 : (bi.Level - 1) / 3 + 1;
            int newLevel = Math.Min(bi.Level + 1, 21);
            int newRc = (newLevel - 1) / 3 + 1;
            int addDevPoints = RankRewards.List[newLevel].DevPointRevard;
            vm.Info = BuildInfoRow(bi, addDevPoints, culture, _lang);

            vm.Unlocks = _lang["team.Label.Attitude.Mai"];
            vm.UnlocksLevel = (RankNameTable.Data[newLevel].PublicLevel ?? "") + ": ";
            vm.UnlocksRank = RankNameLocalizer.GetName(newLevel, culture);
            vm.RankClass = RankNameLocalizer.GetClass(newRc, culture);
            vm.RankClassChanged = newRc > rc;
            vm.Infotext1 = newRc > rc ? _lang["team.modal.Text.Promote2"] : _lang["team.modal.Text.Promote1"];
            vm.UnlockMaxLevels1 = _lang["team.Label.Attitude.Sec"] + " (max)";
            vm.UnlockMaxLevels2 = _lang["team.Label.Attitude.3rd"] + " (max)";
            vm.Rows.Add(newRc > rc ? new ModalRow(
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
                ValueDisplay: $"{member.EnergyPoints}/{36 + member.Level * 3}",
                separator: UNLOCK_SEP,
                ValueChangeDisplay: $"{36 + newLevel * 3}/{36 + newLevel * 3}",
                color: ""
                ));
            AttitudeLineResolver(member.MaintAttitude, vm, RankConstants.startLevels[0..4], newLevel, culture);
            AttitudeLineResolver(member.SecondAttitude, vm, RankConstants.startLevels[4..8], newLevel, culture, [0, 1, 0, 1]);
            AttitudeLineResolver(member.GenderAttitude, vm, RankConstants.startLevels[8..12], newLevel, culture, [0, 1, 0, 1]);
            return vm;
        }
        public ModalRetireVm BuildRetireVm(TeamMemberDto member, string culture)
        {
            var vm = new ModalRetireVm();
            var bi = BasicInfoResolver(member);


            int newLevel = 31;
            int newRc = 11;
            vm.Info = BuildInfoRow(bi, 0, culture, _lang);
            vm.Infotext1 = _lang["team.modal.Text.Retire1"];
            vm.Unlocks = _lang["team.modal.Label.Unlocks"];
            vm.UnlocksLevel = (RankNameTable.Data[newLevel].PublicLevel ?? "") + ": ";
            vm.UnlocksRank = RankNameLocalizer.GetName(newLevel, culture);
            vm.RankClass = RankNameLocalizer.GetClass(newRc, culture);
            vm.RankClassChanged = true;
            vm.Rows.Add(new ModalRow(
                CategoryName: _lang["team.Label.Pension"],
                ValueDisplay: string.Empty,
                separator: UNLOCK_SEP,
                ValueChangeDisplay: "+" + member.Pension.ToString(),
                color: "color: green;"
                ));
            vm.Rows.Add(new ModalRow(
                CategoryName: _lang["team.label.TeamDevPoint"],
                ValueDisplay: string.Empty,
                separator: UNLOCK_SEP,
                ValueChangeDisplay: "+" + RankRewards.List[22].DevPointToStore.ToString(),
                color: "color: green;"
                ));

            return vm;
        }
        public ModalHandleVm BuildHandleVm(TeamMemberDto member, string culture)
        {
            var vm = new ModalHandleVm();
            var bi = BasicInfoResolver(member);

            vm.Info = BuildInfoRow(bi, 0, culture, _lang);
            vm.Infotext1 = _lang["team.modal.Text.Handle2"];
            vm.Infotext2 = _lang["team.modal.Text.Handle1"];
            vm.Infotext3 = _lang["team.modal.Text.Handle3"];
            if (member.SkillPoints == 0)
                vm.Infotext4 = _lang["team.modal.Text.Handle4"].FormatSafe(vm.Info.Devpoints[0..7]);
            else
                vm.Infotext4 = string.Empty;
            return vm;
        }

        private static InfoBlock BuildInfoRow(BasicInfo infoRowData, int adddevpoints, string culture, ILanguageService lang)
        {

            if (infoRowData.IsMember)
                return new InfoBlock(
                    Name: lang["team.Label.Name"],
                    Color: GenreColorResolver(infoRowData.Piccode),
                    NameValue: infoRowData.Name,
                    Rank: lang["team.Label.Rank"],
                    RankValue: RankNameLocalizer.GetName(infoRowData.Level, culture),
                    Level: lang["team.Label.Level"],
                    LevelValue: RankNameTable.Data[infoRowData.Level].PublicLevel ?? "",
                    Orient1: lang["team.modal.Label.Orient1"],
                    Orient2: lang["team.modal.Label.Orient2"],
                    Orient1Value: OrientationLocalizer.GetOrientation(infoRowData.Orient1, culture),
                    Orient2Value: OrientationLocalizer.GetOrientation(infoRowData.Orient2, culture),
                    Devpoints: lang["team.Label.SkillPointShort"].FormatSafe(OrientationLocalizer.GetOrientShort(infoRowData.Orient1, culture)),
                    DevPointsValue: infoRowData.Devpoints,
                    AddedDevPoints: adddevpoints > 0 ? "+" + adddevpoints.ToString() : ""
                );
            else
                return new InfoBlock(
                    Name: lang["team.Label.Name"],
                    Color: GenreColorResolver(infoRowData.Piccode),
                    NameValue: infoRowData.Name,
                    Rank: lang["team.Label.Org"],
                    RankValue: RankNameLocalizer.GetTeam(infoRowData.Level, culture),
                    Level: lang["team.Label.Level"],
                    LevelValue: RankNameTable.Data[infoRowData.Level].PublicLevel ?? "",
                    Orient1: string.Empty,
                    Orient2: string.Empty,
                    Orient1Value: string.Empty,
                    Orient2Value: string.Empty,
                    Devpoints: lang["team.label.TeamDevPointShort"],
                    DevPointsValue: infoRowData.Devpoints,
                    AddedDevPoints: adddevpoints > 0 ? "+" + adddevpoints.ToString() : ""
                );

        }
        private static BasicInfo BasicInfoResolver(TeamMemberDto member)
        {

            return new BasicInfo()
            {
                IsMember = true,
                Name = member.Name,
                Piccode = member.PictureCode,
                Devpoints = member.SkillPoints.ToString(),
                Level = member.Level,
                Orient1 = member.MaintAttitude.Category[0] > 8 ? member.MaintAttitude.Category[2] : member.MaintAttitude.Category[0],
                Orient2 = member.SecondAttitude.Category[0] > 8 ? member.SecondAttitude.Category[2] : member.SecondAttitude.Category[0]
            };
        }

        private static void HelpLineResolver(HelpDto help, ModalTeamPromoteVm vm, int[] slevel, int[] maxLevel, int level, string culture)
        {
            int actL = level - 1;
            int newL = level;
            for (int i = 0; i < 4; i++)
            {
                if (newL >= slevel[i] && help.Skill[i].LvlCurMax + 1<= maxLevel[i])
                {
                    vm.Rows.Add(new ModalRow(
                     CategoryName: CategoryNameLocalizer.GetCategory(help.Category[i], culture),
                     ValueDisplay: $"{help.Skill[i].LvlCurrent}/{help.Skill[i].LvlCurMax}",
                     separator: UNLOCK_SEP,
                     ValueChangeDisplay: $"{help.Skill[i].LvlCurrent}/{help.Skill[i].LvlCurMax + 1}",
                     color: string.Empty
                    ));
                }

            }
        }
        private static void AttitudeLineResolver(AttidtudeDto att, ModalPromoteVm vm, int[] slevel, int level, string culture, int[]? correctors = null)
        {
            correctors ??= [0, 0, 0, 0];
            int actL = level - 1;
            int newL = level;
            double valC = 0.0;
            double valN = 0.0;
            string pref = string.Empty;

            for (int i = 0; i < 4; i++)
            {
                if (slevel[i] == 0)
                {
                    valC = ModifierTable.DataMainSkill[i].StartValue + (actL - 1) * ModifierTable.DataMainSkill[i].StepValue;
                    valN = ModifierTable.DataMainSkill[i].StartValue + (newL - 1) * ModifierTable.DataMainSkill[i].StepValue;
                    pref = valC > 0 ? "+" : "";
                }
                if (newL >= slevel[i])
                {
                    int correct = att.Skill[i].LvlCurMax == 0 ? correctors[i] : 0;
                    vm.Rows.Add(new ModalRow(
                     CategoryName: CategoryNameLocalizer.GetCategory(att.Category[i], culture),
                     ValueDisplay: slevel[i] != 0 ? $"{att.Skill[i].LvlCurrent}/{att.Skill[i].LvlCurMax}" : pref + (TeamHelpers.FormatOneDecimal(valC, false)),
                     separator: UNLOCK_SEP,
                     ValueChangeDisplay: slevel[i] != 0 ? $"{att.Skill[i].LvlCurrent}/{att.Skill[i].LvlCurMax + 1 + correct}" : pref + (TeamHelpers.FormatOneDecimal(valN, false)),
                     color: slevel[i] == 0 ? pref == "+" ? "red" : "green" : ""
                    ));
                }

            }
        }
        private static string GenreColorResolver(string pictureCode)
        {
            if (string.IsNullOrEmpty(pictureCode))
                return "#cccccc";
            return pictureCode.StartsWith
                    ("M") ? "lightblue" : "pink";
        }
        private sealed class BasicInfo
        {
            public bool IsMember = true;
            public string Name = string.Empty;
            public string Piccode = string.Empty;
            public string Devpoints = string.Empty;
            public int Level = 0;
            public int Orient1 = 0;
            public int Orient2 = 0;
        }
    }
}
