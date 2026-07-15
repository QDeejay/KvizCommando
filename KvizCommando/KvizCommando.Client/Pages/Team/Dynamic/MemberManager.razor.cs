using KvizCommando.Client.Pages.Team.Dynamic.Builders;
using KvizCommando.Client.Pages.Team.ViewModels;
using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Client.Services.Visual.UiService.Language;
using KvizCommando.Shared.Contracts.Team;
using KvizCommando.Shared.Models.Dtos;
using Microsoft.AspNetCore.Components;


namespace KvizCommando.Client.Pages.Team.Dynamic
{
    public partial class MemberManager : IDisposable
    {
        [Inject] private ILanguageService Lang { get; set; } = default!;

        [CascadingParameter]
        private AppState AppStates { get; set; } = default!;

        [CascadingParameter]
        public int SelectedPos { get; set; } = default!;
        //[Parameter] public EventCallback<ModifySkillRequest> OnModifySkillPushed { get; set; } = default!;
        [Parameter] public Func<ModifySkillRequest, Task>? OnModifySkillPushed { get; set; }
        private TBuilderMember? _builder;
        private UpperBlockVm _vmUp = new();
        private BottomBlockVm _vmBot = new();
        private BottomDevVm _vmDev = new();

        private bool _isReady = false;
        private int _currentSubPage = 0;
        private bool _proConSw = false;

        private int[] _usedPoints = new int[4];
        private TeamMemberDto _oldMember = new();
        private int _oldSelectedPos = 0;
        private string Culture => AppStates.Culture;
        private TeamMemberDto Member => SelectedPos > 0 ? AppStates.Team!.TeamMembers![SelectedPos] : new();
        private string PicCode => _currentSubPage == 0 ? Member.PictureCode ?? string.Empty : string.Empty;
        private void ResetUsedPoints() => _usedPoints = [0, 0, 0, 0];

        protected override void OnParametersSet()
        {
            if (!_isReady || SelectedPos == 0) return;
            if (_oldMember != Member)
            {
                _vmUp = _builder!.BuildMemberUpperVm(Member, Culture);
                ShowSubPage(_currentSubPage);
                _oldMember = Member;
            }
            if (_oldSelectedPos != SelectedPos)
            {
                ShowSubPage(0);
                _oldSelectedPos = SelectedPos;
            }

        }

        private void ShowSubPage(int page)
        {
            ResetUsedPoints();
            if (page == 0)
            {
                _vmBot = _builder!.BuildMemberBottomVm(Member, Culture);
                _proConSw = false;
            }
            else
                _vmDev = _builder!.BuildMemberBottomDevVm(page, Member, _usedPoints, Culture);
            _currentSubPage = page;
        }
        private void OnIncButtonPushed(int rowId)
        {
            int[] usdPnts = _usedPoints;
            if (usdPnts.Sum() >= Member.SkillPoints) return;
            usdPnts[rowId]++;
            _vmDev = _builder!.BuildMemberBottomDevVm(_currentSubPage, Member, usdPnts, Culture);
            _usedPoints = usdPnts;
            StateHasChanged();
        }
        private void OnDecButtonPushed(int rowId)
        {
            int[] usdPnts = _usedPoints;
            if (usdPnts.Sum() <= 0 || usdPnts[rowId] <= 0) return;
            usdPnts[rowId]--;
            _vmDev = _builder!.BuildMemberBottomDevVm(_currentSubPage, Member, usdPnts, Culture);
            _usedPoints = usdPnts;
            StateHasChanged();
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
        private async Task OnSaveButtonAsync()
        {
            if (_usedPoints.Sum() == 0) return;

            if (_currentSubPage != 1 && _currentSubPage != 2) return;

            ModifySkillRequest request = new()
            {
                SkillChanges = _usedPoints,
                SkillType = _currentSubPage,
                MemberId = SelectedPos
            };

            if (OnModifySkillPushed is not null)
                await OnModifySkillPushed.Invoke(request);

        }
        public void Dispose()
        {
            OnModifySkillPushed = default;
            GC.SuppressFinalize(this);
        }
    }
}
