using KvizCommando.Client.Helpers;
using KvizCommando.Client.Models.ViewModels;
using KvizCommando.Client.Pages.Team;
using KvizCommando.Client.Services.Dto;
using KvizCommando.Client.Services.Visual.UiService.Language;
using KvizCommando.Shared.Models;
using KvizCommando.Shared.Models.Dtos;
using System.Reflection.Emit;

namespace KvizCommando.Client.Features.Team
{
    public class TBuilderRecruit
    {
        
        private static readonly int[] que = [1, 2, 3, 4, 5, 6, 7, 8];
        public static RecruitVm BuildRecruitVm(CandidateDto candidate, int[] order, int tabpos, string culture, ILanguageService lang)
        {
            var vm = new RecruitVm() { Info= lang["team.Label.NoMember"] };
            if (tabpos < 1 || tabpos > 8)
                return vm;
            if (!candidate.CanBeHire)
                return vm;
            if (candidate==null)
                return vm;
            ;
            var orderedQue = order ?? que;     
            foreach (int i in orderedQue)
            {
                var datas = TeamHelper.RecruitResolver(tabpos,i);
                int ori2 = datas.Item2;
                int[] cats = datas.Item3;
                string oriShort = OrientationLocalizer.GetOrientShort(ori2, culture);
                vm.Cards.Add(new RecruitBlock(
                    Name: candidate.Name[i-1] ?? string.Empty,
                    Sex: i<5,
                    ImageCode: candidate.PictureCode[i-1] ?? string.Empty,
                    SubOrientSh: $"<{oriShort}>",
                    MainCat: CategoryNameLocalizer.GetCategory(cats[0], culture),
                    SubCat: CategoryNameLocalizer.GetCategory(cats[1], culture),
                    ExtCat: CategoryNameLocalizer.GetCategory(cats[2], culture),
                    ClickId: i
                    )); 
            }
            return vm;
        }
    }
}
/*
 private readonly ILanguageService _lang;
        public RecruitBlockBuilder(ILanguageService lang)
        {
            _lang = lang;            
        }
 
 */