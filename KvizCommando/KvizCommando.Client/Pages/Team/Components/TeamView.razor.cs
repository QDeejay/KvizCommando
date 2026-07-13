using KvizCommando.Client.Features.Team;
using KvizCommando.Client.Models.ViewModels;
using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Client.Services.Visual.UiService.Language;
using KvizCommando.Shared.Contracts.Team;
using KvizCommando.Shared.Models.Dtos;
using Microsoft.AspNetCore.Components;


namespace KvizCommando.Client.Pages.Team.Components
{
    public partial class TeamView : IDisposable
    {
        [Inject] private ILanguageService Lang { get; set; } = default!;

        [CascadingParameter]
        private AppState AppStates { get; set; } = default!;


        [Parameter] public EventCallback<int> ActionButtonPushed { get; set; } = default!;
        [Parameter] public EventCallback<ModifySkillRequest> ModifySkill { get; set; } = default!;

        private TBuilderTeam? _builder;
        private UpperBlockVm _vmUp = new();
        private BottomBlockVm _vmBot = new();
        private BottomDevVm _vmDev = new();

        private bool _isReady = false;
        private int _currentSubPage = 0;

        private int[] _usedPoints = new int[4];

        private string Culture => AppStates.Culture;
        private TeamDtos Team => AppStates.Team!;
        private TeamExtendedInfo _oldInfo = new();
        private TeamMemberDto[] Memebers => Team.TeamMembers!;
        private TeamExtendedInfo Info => Team.TeamInfo;
        private HelpDto Help => Team.Help;
        private void ResetUsedPoints() => _usedPoints = [0, 0, 0, 0];

        protected override void OnParametersSet()
        {
            if (!_isReady) return;

            if (_oldInfo != Info)
            {
                _vmUp = _builder!.BuildTeamUpperVm(Info, Culture);
                ResetUsedPoints();
                ShowSubPage(_currentSubPage);
                _oldInfo = Info;
            }
        }

        private void ShowSubPage(int page)
        {
            if (page == 0)
                _vmBot = _builder!.BuildTeamBottomVm(Memebers, Culture);
            else
                _vmDev = _builder!.BuildTeamBottomDevVm(Info, _usedPoints, Help, Culture);
            
            if (page != _currentSubPage)
                ResetUsedPoints();

            _currentSubPage = page;
        }
        private void OnIncButtonPushed(int rowId)
        {
            int[] usdPnts = _usedPoints;
            if (usdPnts.Sum() >= Team.TeamInfo.DevPoints) return;
            usdPnts[rowId]++;
            _vmDev = _builder!.BuildTeamBottomDevVm(Info, usdPnts, Help, Culture);
            _usedPoints = usdPnts;
            StateHasChanged();
        }
        private void OnDecButtonPushed(int rowId)
        {
            int[] usdPnts = _usedPoints;
            if (usdPnts.Sum() <= 0 || usdPnts[rowId] <= 0) return;
            usdPnts[rowId]--;
            _vmDev = _builder!.BuildTeamBottomDevVm(Info, usdPnts, Help, Culture);
            _usedPoints = usdPnts;
            StateHasChanged();
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
            int delegateItem = _vmBot.Rows[rowId].Action;
            if (ActionButtonPushed.HasDelegate)
                await ActionButtonPushed.InvokeAsync(delegateItem);
        }

        protected override void OnInitialized()
        {
            _builder = new TBuilderTeam(Lang);
            _isReady = true;
        }
        private void OnResetButtonPushed()
        {
            ResetUsedPoints();
            _vmDev = _builder!.BuildTeamBottomDevVm(Info, _usedPoints, Help, Culture);
        }
        public void Dispose()
        {
            ActionButtonPushed = default;
            ModifySkill = default;
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