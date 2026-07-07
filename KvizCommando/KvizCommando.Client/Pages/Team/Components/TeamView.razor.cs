using KvizCommando.Client.Features.Team;
using KvizCommando.Client.Models.ViewModels;
using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Client.Services.Visual.UiService.Language;
using KvizCommando.Shared.Models.Dtos;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace KvizCommando.Client.Pages.Team.Components
{
    public partial class TeamView
    {
        [CascadingParameter]
        private AppState AppStates { get; set; } = default!;

        [Inject] private ILanguageService Lang { get; set; } = default!;
        [Parameter] public EventCallback<int> ActionButtonPushed { get; set; } = default!;
        private string Culture => AppStates.Culture;
        //private TeamExtendedInfo Info => AppStates.Team!.TeamInfo;
        private TeamDtos Team => AppStates.Team!;

        private UpperBlockViewModel _vmUp = new();
        private BottomBlockViewModel _vmBot = new();

        protected override void OnParametersSet()
        {
            var info = Team.TeamInfo;
            _vmUp = UpperBlockDataBuilder.BuildTeamHeader(info, 0, Culture, Lang);
            _vmBot = BottomBlockDataBuilder.BuildTeamView(Team, Culture);
        }

        private async Task OnActionButtonPushed(int rowId)
        {
            int delegateItem = vm.Rows[rowId].action > 400 ? vm.Rows[rowId].action - 100 : vm.Rows[rowId].action;
            if (ActionButtonPushed.HasDelegate)
                await ActionButtonPushed.InvokeAsync(delegateItem);
        }
        public void Dispose()
        {
            ActionButtonPushed = default;
            GC.SuppressFinalize(this);
        }
    }
}
