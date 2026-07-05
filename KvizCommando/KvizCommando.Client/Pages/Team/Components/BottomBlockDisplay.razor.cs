using KvizCommando.Client.Features.Team;
using KvizCommando.Client.Models.ViewModels;
using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Client.Utilities;
using KvizCommando.Shared.Models.Dtos;
using Microsoft.AspNetCore.Components;
using System.ComponentModel.Design;
using System.Globalization;
using System.Reflection.PortableExecutable;


namespace KvizCommando.Client.Pages.Team.Components
{
    public partial class BottomBlockDisplay: KcComponentBase
    {
        //[Inject] private BottomBlockDataBuilder Builder { get; set; } = default!;
        [CascadingParameter]
        private AppState AppStates { get; set; } = default!;

        [CascadingParameter]
        private int Selected { get; set; }

        //[Parameter] public TeamDtos Team { get; set; } = default!;
        //[Parameter] public int TabPos { get; set; }
        [Parameter] public EventCallback<int> ActionButtonPushed { get; set; } = default!;

        private TeamDtos Team => AppStates.Team!;
        private string Culture => AppStates.Culture;

        private string _colorColumn = string.Empty;
        
        private BottomBlockViewModel vm = new();
        protected override void OnParametersSet()
        {
            if (Selected == 0)
                vm = BottomBlockDataBuilder.BuildTeamView(Team, Culture);
           else if(Team.CharCatMask[Selected])
                vm = BottomBlockDataBuilder.BuildMemberView(Team.TeamMembers[Selected], Culture);
        }
        protected async Task OnActionButtonPushed(int rowId)
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