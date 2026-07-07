using KvizCommando.Client.Features.Team;
using KvizCommando.Client.Models.ViewModels;
using KvizCommando.Client.Pages.Question.Components;
using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Client.Services.Visual.UiService.Language;
using KvizCommando.Client.Utilities;
using KvizCommando.Shared.Models.Dtos;
using Microsoft.AspNetCore.Components;
using System.Globalization;

namespace KvizCommando.Client.Pages.Team.Components
{
    public partial class UpperBlockDisplay 
    {
        [CascadingParameter]
        private AppState AppStates { get; set; } = default!;

        [CascadingParameter]
        private int Selected { get; set; }
        [Inject] private  ILanguageService Lang { get; set; } = default!;
        //[Parameter] public int UsedSkillPoints { get; set; } = 0;

        private UpperBlockVm vm = new();
        private bool _shouldBeShow = false;
        private string _picCode = string.Empty;
        private string Culture => AppStates.Culture;
        private TeamExtendedInfo Info => AppStates.Team!.TeamInfo;
        private TeamMemberDto Member => AppStates.Team!.TeamMembers![Selected] ?? new TeamMemberDto();


        protected override void OnParametersSet()
        {
            _shouldBeShow = true;
            _picCode = string.Empty;
            if (Selected == 0)
                Console.WriteLine("");
            //vm = UpperBlockDataBuilder.BuildTeamHeader(Info, 0, Culture, Lang);
            else
            {
                if (AppStates.Team!.CharCatMask[Selected - 1])
                {
                    //  vm = UpperBlockDataBuilder.BuildMemberHeader(Member, 0, Culture, Lang);
                    _picCode = Member.PictureCode;
                }
                else
                    _shouldBeShow = false;
            }
               
        }
    }
}
