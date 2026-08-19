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
    /// <summary>
    /// A csapatkezeléshez tartozó modális nézetmodelleket állítja össze.
    /// </summary>
    public sealed partial class TBuilderModal
    {
        private readonly ILanguageService _lang;

        /// <summary>
        /// Létrehozza a csapatmodálok nézetmodell-builderét.
        /// </summary>
        /// <param name="lang">A feliratok feloldásához használt nyelvi szolgáltatás.</param>
        public TBuilderModal(ILanguageService lang)
        {
            _lang = lang;
        }

        private const string UNLOCK_SEP = " => ";

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
                if (newL >= slevel[i] && help.Skill[i].LvlCurMax + 1 <= maxLevel[i])
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
