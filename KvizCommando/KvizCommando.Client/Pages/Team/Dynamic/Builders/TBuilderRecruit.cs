using KvizCommando.Client.Helpers;
using KvizCommando.Client.Models.ViewModels;
using KvizCommando.Client.Services.Visual.UiService.Language;
using KvizCommando.Shared.Models.Dtos;

namespace KvizCommando.Client.Pages.Team.Dynamic.Builders
{
    public class TBuilderRecruit
    {
        private static readonly int[] que = [1, 2, 3, 4, 5, 6, 7, 8];
        public static RecruitVm BuildRecruitVm(CandidateDto candidate, int[] order, int tabpos, string culture, ILanguageService lang)
        {
            var vm = new RecruitVm() { Info = lang["team.Label.NoMember"] };
            if (tabpos < 1 || tabpos > 8)
                return vm;
            if (!candidate.CanBeHire)
                return vm;
            if (candidate == null)
                return vm;

            var orderedQue = order ?? que;
            int index = 0;
            foreach (int i in orderedQue)
            {
                index++;
                if (index == 1 || index == 2)  // üres kártyák az első két sorba hogy legyen helye a képnek
                    vm.Cards.Add(new RecruitBlock(false, 0, new RecruitCardVm()));
                vm.Cards.Add(new RecruitBlock(
                    Show: true,
                    ClickId: i,
                    Card: RecCardResolver(candidate, i, tabpos, culture)
                     ));
                // ImageCode: candidate.PictureCode[i - 1] ?? string.Empty,


            }
            return vm;
        }
        private static RecruitCardVm RecCardResolver(CandidateDto cand, int cardNo, int pos, string cult)
        {
            var datas = THelpers.RecruitResolver(pos, cardNo);
            int ori2 = datas.Item2;
            int[] cats = datas.Item3;
            string oriShort = OrientationLocalizer.GetOrientShort(ori2, cult);
            return new RecruitCardVm
            {
                Name = cand.Name[cardNo - 1] ?? string.Empty,
                Sex = cardNo < 5,
                SecOrient = $"<{oriShort}>",
                MainCat = CategoryNameLocalizer.GetCategory(cats[0], cult),
                SubCat = CategoryNameLocalizer.GetCategory(cats[1], cult),
                ExtCat = CategoryNameLocalizer.GetCategory(cats[2], cult),
            };
        }
    }
}