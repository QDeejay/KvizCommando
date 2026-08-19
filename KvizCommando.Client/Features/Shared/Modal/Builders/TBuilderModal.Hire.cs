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
        /// Összeállítja a jelölt felvételéhez tartozó modális nézetmodellt.
        /// </summary>
        /// <param name="candidate">A megjelenítendő vagy felveendő jelölt adatai.</param>
        /// <param name="hpos">A toborzási hely sorszáma.</param>
        /// <param name="candno">A jelölt helyének sorszáma.</param>
        /// <param name="culture">A kért kultúra neve, például <c>hu-HU</c>.</param>
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
    }
}
