using KvizCommando.Client.Features.Team;
using KvizCommando.Client.Models.ViewModels;
using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Client.Services.Visual.UiService.Language;
using KvizCommando.Shared.Contracts.Team;
using KvizCommando.Shared.Models.Dtos;
using Microsoft.AspNetCore.Components;


namespace KvizCommando.Client.Pages.Team.Components
{
    public partial class MemberView : IDisposable
    {
        [Inject] private ILanguageService Lang { get; set; } = default!;

        [CascadingParameter]
        private AppState AppStates { get; set; } = default!;
        [CascadingParameter]
        public int SelectedPos { get; set; } = default!;
        [Parameter] public EventCallback<ModifySkillRequest> ModifySkill { get; set; } = default!;

        private TBuilderMember? _builder;
        private UpperBlockVm _vmUp = new();
        private BottomBlockVm _vmBot = new();
        private BottomDevVm _vmDev = new();

        private bool _isReady = false;
        private int _currentSubPage = 0;
        private bool _proConSw = false;

        private int[] _usedPoints = new int[4];
        private string Culture => AppStates.Culture;

        private TeamMemberDto Member => SelectedPos > 0 ? AppStates.Team.TeamMembers[SelectedPos] : new();
        private TeamMemberDto _oldMember = new();
        private void ResetUsedPoints() => _usedPoints = [0, 0, 0, 0];
        protected override void OnParametersSet()
        {
            if (!_isReady) return;
            if (_oldMember != Member)
            {
                _vmUp = _builder!.BuildMemberUpperVm(Member, Culture);
                ShowSubPage(_currentSubPage);
                _oldMember = Member;
            }
        }
        private void ShowSubPage(int page)
        {
            if (page == 0)
            {
                _vmBot = _builder!.BuildMemberBottomVm(Member, Culture);
                _proConSw = false;
            }
            else
                _vmDev = _builder!.BuildMemberBottomDevVm(page, Member, _usedPoints, Culture);

            if (page != _currentSubPage)
                ResetUsedPoints();

            _currentSubPage = page;

        }
        private void OnIncButtonPushed(int rowId)
        {
            int[] usdPnts = _usedPoints;
            if (usdPnts.Sum() >= Member.SkillPoints) return;
            usdPnts[rowId]++;
            _vmDev = _builder!.BuildMemberBottomDevVm(_currentSubPage, Member, _usedPoints, Culture);
            _usedPoints = usdPnts;
            StateHasChanged();
        }
        private void OnDecButtonPushed(int rowId)
        {
            int[] usdPnts = _usedPoints;
            if (usdPnts.Sum() <= 0 || usdPnts[rowId] <= 0) return;
            usdPnts[rowId]--;
            _vmDev = _builder!.BuildMemberBottomDevVm(_currentSubPage, Member, _usedPoints, Culture);
            _usedPoints = usdPnts;
            StateHasChanged();
        }

        private async Task OnSaveButtonPushed()
        {
            if (_usedPoints.Sum() == 0) return;

            if (_currentSubPage != 1 && _currentSubPage != 2) return;

            ModifySkillRequest request = new()
            {
                SkillChanges = _usedPoints,
                SkillType = _currentSubPage,
                MemberId = SelectedPos
            };

            if (ModifySkill.HasDelegate)
                await ModifySkill.InvokeAsync(request);
        }
        private void OnActionButtonPushed(int rowId)
        {
            if (rowId < 7)
                ShowSubPage(1);
            else
                ShowSubPage(2);
        }

        private void OnResetButtonPushed()
        {
            ResetUsedPoints();
            _vmDev = _builder!.BuildMemberBottomDevVm(_currentSubPage, Member, _usedPoints, Culture);
        }
        protected override void OnInitialized()
        {
            _builder = new TBuilderMember(Lang);
            _isReady = true;
        }

        public void Dispose()
        {
            ModifySkill = default;
            SelectedPos = default;
            GC.SuppressFinalize(this);
        }
    }
}
