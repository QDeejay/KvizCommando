using KvizCommando.Client.Features.Team.Builders;
using KvizCommando.Client.Features.Team.Services;
using KvizCommando.Client.Features.Team.ViewModels;
using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Client.Services.Audio;
using KvizCommando.Client.Services.Visual.UiService;
using KvizCommando.Client.Services.Visual.UiService.Language;
using KvizCommando.Shared.Contracts.Team;
using KvizCommando.Shared.Models.Dtos;
using Microsoft.AspNetCore.Components;

namespace KvizCommando.Client.Features.Team.Components;

public partial class MemberManager : IDisposable
{
    [Inject] private ILanguageService Lang { get; set; } = default!;
    [Inject] private ITeamClientService TeamService { get; set; } = default!;
    [Inject] private UiServices Ui { get; set; } = default!;
    [Inject] private AudioService Audio { get; set; } = default!;

    [CascadingParameter]
    private AppState AppStates { get; set; } = default!;

    [Parameter]
    public int InitialPosition { get; set; }

    private TBuilderMember _builder = default!;
    private UpperBlockVm _vmUp = new();
    private BottomBlockVm _vmBot = new();
    private BottomDevVm _vmDev = new();
    private TeamDtos? _previousTeam;
    private TeamMemberDto? _previousMember;
    private string _previousCulture = string.Empty;
    private int[] _usedPoints = [0, 0, 0, 0];
    private int _previousInitialPosition = -1;
    private int _selectedPosition;
    private int _currentSubPage;
    private bool _proConSw;
    private bool _isReady;

    private string Culture => AppStates.Culture;
    private TeamDtos TeamData => AppStates.Team!;
    private bool[] CharacterMask => TeamData.CharCatMask[1..9];
    private TeamMemberDto? Member =>
        _selectedPosition is >= 1 and <= 8
            ? TeamData.TeamMembers![_selectedPosition]
            : null;
    private string PicCode => _currentSubPage == 0
        ? Member?.PictureCode ?? string.Empty
        : string.Empty;
    protected override void OnInitialized()
    {
        _builder = new TBuilderMember(Lang);
        Ui.SubHeader.OnButtonClicked += HandleSubHeaderClicked;
    }

    protected override void OnParametersSet()
    {
        var snapshotChanged =
            !ReferenceEquals(_previousTeam, TeamData);
        var cultureChanged = _previousCulture != Culture;
        var initialPositionChanged =
            _previousInitialPosition != InitialPosition;
        var requestedPosition =
            initialPositionChanged && InitialPosition > 0
                ? InitialPosition
                : _selectedPosition;
        var resolvedPosition = TeamHelpers.ResolvePosition(
            CharacterMask,
            requestedPosition);
        var selectionChanged =
            resolvedPosition != _selectedPosition;

        _selectedPosition = resolvedPosition;

        if (snapshotChanged ||
            cultureChanged ||
            initialPositionChanged)
        {
            ShowSubHeader();
        }

        if (selectionChanged)
            _currentSubPage = 0;

        if (snapshotChanged ||
            cultureChanged ||
            selectionChanged ||
            !ReferenceEquals(_previousMember, Member))
        {
            BuildSelectedMember();
        }

        _previousTeam = TeamData;
        _previousCulture = Culture;
        _previousInitialPosition = InitialPosition;
    }

    private void ShowSubHeader()
    {
        Ui.SubHeader.Show(
            TeamHelpers.SubHeaderResolver(
                CharacterMask,
                CharacterMask,
                new string[8],
                Culture),
            _selectedPosition);
    }

    private void BuildSelectedMember()
    {
        var member = Member;

        if (member is null)
        {
            _isReady = false;
            return;
        }

        _vmUp = _builder.BuildMemberUpperVm(
            member,
            Culture);
        ShowSubPage(_currentSubPage);
        _previousMember = member;
        _isReady = true;
    }

    private void HandleSubHeaderClicked(int index)
    {
        if (index is < 1 or > 8 ||
            !CharacterMask[index - 1] ||
            index == _selectedPosition)
        {
            return;
        }

        _selectedPosition = index;
        _currentSubPage = 0;
        BuildSelectedMember();
        _ = InvokeAsync(StateHasChanged);
    }

    private void ShowSubPage(int page)
    {
        var member = Member;

        if (member is null)
            return;

        ResetUsedPoints();

        if (page == 0)
        {
            _vmBot = _builder.BuildMemberBottomVm(
                member,
                Culture);
            _proConSw = false;
        }
        else
        {
            _vmDev = _builder.BuildMemberBottomDevVm(
                page,
                member,
                _usedPoints,
                Culture);
        }

        _currentSubPage = page;
    }

    private async Task ShowSubPageAsync(int page)
    {
        await Audio.PlaySfxAsync(AudioService.SFX_UI_TOUCH);
        ShowSubPage(page);
    }

    private async Task OnIncButtonPushed(int rowId)
    {
        var member = Member;

        if (member is null ||
            _usedPoints.Sum() >= member.SkillPoints)
        {
            return;
        }

        await Audio.PlaySfxAsync(AudioService.SFX_UI_TOUCH);
        _usedPoints[rowId]++;
        RebuildDevelopmentView(member);
    }

    private async Task OnDecButtonPushed(int rowId)
    {
        var member = Member;

        if (member is null ||
            _usedPoints.Sum() == 0 ||
            _usedPoints[rowId] == 0)
        {
            return;
        }

        await Audio.PlaySfxAsync(AudioService.SFX_UI_TOUCH);
        _usedPoints[rowId]--;
        RebuildDevelopmentView(member);
    }

    private async Task OnActionButtonPushed(int rowId)
    {
        if (_vmBot.Rows[rowId].Remark == string.Empty)
            return;

        await Audio.PlaySfxAsync(AudioService.SFX_UI_TOUCH);
        ShowSubPage(rowId < 7 ? 1 : 2);
    }

    private async Task OnResetButtonPushed()
    {
        var member = Member;

        if (member is null)
            return;

        await Audio.PlaySfxAsync(AudioService.SFX_UI_TOUCH);
        ResetUsedPoints();
        RebuildDevelopmentView(member);
    }

    private async Task OnSaveButtonAsync()
    {
        if (_usedPoints.Sum() == 0 ||
            _currentSubPage is not 1 and not 2)
        {
            return;
        }

        await Audio.PlaySfxAsync(AudioService.SFX_UI_TOUCH);
        var request = new ModifySkillRequest
        {
            SkillChanges = [.. _usedPoints],
            SkillType = _currentSubPage,
            MemberId = _selectedPosition
        };

        if (!await TeamService.ModifySkillsAsync(request))
            return;

        await Ui.ReloadAsync(ReqStates.Team);
    }

    private void RebuildDevelopmentView(
        TeamMemberDto member)
    {
        _vmDev = _builder.BuildMemberBottomDevVm(
            _currentSubPage,
            member,
            _usedPoints,
            Culture);
    }

    private void ResetUsedPoints()
    {
        _usedPoints = [0, 0, 0, 0];
    }

    private async Task SetProConAsync(bool value)
    {
        await Audio.PlaySfxAsync(AudioService.SFX_UI_TOUCH);
        _proConSw = value;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Ui.SubHeader.OnButtonClicked -= HandleSubHeaderClicked;
        Ui.SubHeader.Hide();
        GC.SuppressFinalize(this);
    }
}
