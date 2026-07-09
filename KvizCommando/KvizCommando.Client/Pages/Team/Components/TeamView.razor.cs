using KvizCommando.Client.Features.Team;
using KvizCommando.Client.Models.ViewModels;
using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Client.Services.Visual.UiService.Language;
using KvizCommando.Shared.Contracts.Team;
using KvizCommando.Shared.Models.Dtos;
using Microsoft.AspNetCore.Components;
using System.Reflection.PortableExecutable;
using System.Text.RegularExpressions;


namespace KvizCommando.Client.Pages.Team.Components
{
    public partial class TeamView : IDisposable
    {
        [CascadingParameter]
        private AppState AppStates { get; set; } = default!;
        [Inject] private ILanguageService Lang { get; set; } = default!;
        [Parameter] public EventCallback<int> ActionButtonPushed { get; set; } = default!;
        [Parameter] public EventCallback<ModifySkillRequest> ModifySkill { get; set; } = default!;

        private TBuilderTeam? _builder;
        private UpperBlockVm _vmUp = new();
        private BottomBlockVm _vmBot = new();
        private BottomDevVm _vmDev = new();

        private bool _isReady = false;
        private int _currentSubPage = 0;
        
        private int[] _usedPoints;
        private string Culture => AppStates.Culture;
        private TeamDtos Team => AppStates.Team!;

        protected override void OnParametersSet()
        {
            if (!_isReady) return;
                _vmUp = _builder!.BuildTeamUpperVm(Team.TeamInfo, _usedPoints.Sum(), Culture);
            ShowSubPage(_currentSubPage);
        }
        private void ShowSubPage(int page)
        {
            ResetUsedPoints();
            if (page == 0)
                _vmBot = _builder.BuildTeamBottomVm(Team, Culture);
            else
                _vmDev = _builder.BuildTeamBottomDevVm(Team.TeamInfo, _usedPoints, Team.Help, Culture);
            _currentSubPage = page;
        }
        private void OnIncButtonPushed(int rowId)
        {
            int[] usdPnts = _usedPoints;
            if (usdPnts.Sum() >= Team.TeamInfo.DevPoints) return;
            _usedPoints[rowId]++;
            //usdPnts = _usedPoints;
            _vmDev = _builder.BuildTeamBottomDevVm(Team.TeamInfo, _usedPoints, Team.Help, Culture);
            _vmUp = _builder!.BuildTeamUpperVm(Team.TeamInfo, _usedPoints.Sum(), Culture);
        }
        private void OnDecButtonPushed(int rowId)
        {
            int[] usdPnts = _usedPoints;
            if (usdPnts.Sum() >= Team.TeamInfo.DevPoints) return;
            if (_usedPoints[rowId] > 0) _usedPoints[rowId]--;
            //usdPnts = _usedPoints;
            _vmDev = _builder.BuildTeamBottomDevVm(Team.TeamInfo, _usedPoints, Team.Help, Culture);
            _vmUp = _builder!.BuildTeamUpperVm(Team.TeamInfo, _usedPoints.Sum(), Culture);
        }
        private void ResetUsedPoints()
        {
            _usedPoints = [0, 0, 0, 0];
        }
        private async Task OnSaveButtonPushed()
        {
            if (_usedPoints.Sum() == 0) return;
            ModifySkillRequest request = new()
            {
                SkillChanges = _usedPoints,
                SkillType = 1,
                MemberId = 0
            };

            if (ModifySkill.HasDelegate)
                await ModifySkill.InvokeAsync(request);

        }
        private async Task OnActionButtonPushed(int rowId)
        {
            int delegateItem = _vmBot.Rows[rowId].Action > 400 ? _vmBot.Rows[rowId].Action - 100 : _vmBot.Rows[rowId].Action;
            if (ActionButtonPushed.HasDelegate)
                await ActionButtonPushed.InvokeAsync(delegateItem);
        }
        protected override void OnInitialized()
        {
            _builder = new TBuilderTeam(Lang);
            ResetUsedPoints();
            _isReady = true;
        }


        public void Dispose()
        {
            ActionButtonPushed = default;
            GC.SuppressFinalize(this);
        }
    }
}
/*
 
            <div></div><div></div><div></div><div></div>
            <div class="dev-button-section">
                <button class="military-button" disabled="@(_usedPoints.Sum() == 0)" @onclick="OnSaveButtonPushed">@_vmDev.SaveButtonText</button>
            </div>
 
 */