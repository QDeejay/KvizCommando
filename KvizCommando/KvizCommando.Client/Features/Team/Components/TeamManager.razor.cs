using KvizCommando.Client.Features.Team.Builders;
using KvizCommando.Client.Features.Team.Services;
using KvizCommando.Client.Features.Team.ViewModels;
using KvizCommando.Client.Features.Shared.Modal.Dynamic;
using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Client.Services.Visual.UiService;
using KvizCommando.Client.Services.Visual.UiService.Language;
using KvizCommando.Shared.Contracts.Team;
using KvizCommando.Shared.Models.Dtos;
using KvizCommando.Shared.Models.Enums;
using Microsoft.AspNetCore.Components;
using KvizCommando.Client.Features.Shared.Modal.Builders;

namespace KvizCommando.Client.Features.Team.Components;

public partial class TeamManager
{
    [Inject] private ILanguageService Lang { get; set; } = default!;
    [Inject] private ITeamClientService TeamService { get; set; } = default!;
    [Inject] private UiServices Ui { get; set; } = default!;

    [CascadingParameter]
    private AppState AppStates { get; set; } = default!;

    [Parameter]
    public Func<int, Task>? OnMemberSelected { get; set; }

    private const int NUMBER_OF_BOTTOM_ROWS = 4;

    private TBuilderTeam _builder = default!;
    private UpperBlockVm _vmUp = new();
    private BottomBlockVm _vmBot = new();
    private BottomDevVm _vmDev = new();
    private TeamDtos? _previousTeam;
    private string _previousCulture = string.Empty;
    private int[] _usedPoints = [0, 0, 0, 0];
    private int _currentSubPage;
    private bool _listHalfSw;
    private bool _isReady;

    private string Culture => AppStates.Culture;
    private TeamDtos TeamData => AppStates.Team!;
    private TeamMemberDto[] Members => TeamData.TeamMembers!;
    private TeamExtendedInfo Info => TeamData.TeamInfo;
    private HelpDto Help => TeamData.Help;

    protected override void OnInitialized()
    {
        _builder = new TBuilderTeam(Lang);
    }

    protected override void OnParametersSet()
    {
        if (ReferenceEquals(_previousTeam, TeamData) &&
            _previousCulture == Culture)
        {
            return;
        }

        _vmUp = _builder.BuildTeamUpperVm(Info, Culture);
        ShowSubPage(_currentSubPage);
        _previousTeam = TeamData;
        _previousCulture = Culture;
        _isReady = true;
    }

    private void ShowSubPage(int page)
    {
        ResetUsedPoints();

        if (page == 0)
        {
            _vmBot = _builder.BuildTeamBottomVm(
                Members,
                Culture);

            if (_vmBot.Rows.Count <= NUMBER_OF_BOTTOM_ROWS + 1)
                _listHalfSw = false;
        }
        else
        {
            _vmDev = _builder.BuildTeamBottomDevVm(
                Info,
                _usedPoints,
                Help,
                Culture);
            _listHalfSw = false;
        }

        _currentSubPage = page;
    }

    private void OnIncButtonPushed(int rowId)
    {
        if (_usedPoints.Sum() >= Info.DevPoints)
            return;

        _usedPoints[rowId]++;
        RebuildDevelopmentView();
    }

    private void OnDecButtonPushed(int rowId)
    {
        if (_usedPoints.Sum() == 0 ||
            _usedPoints[rowId] == 0)
        {
            return;
        }

        _usedPoints[rowId]--;
        RebuildDevelopmentView();
    }

    private void OnResetButtonPushed()
    {
        ResetUsedPoints();
        RebuildDevelopmentView();
    }

    private async Task OnSaveButtonAsync()
    {
        if (_usedPoints.Sum() == 0)
            return;

        var request = new ModifySkillRequest
        {
            SkillChanges = [.. _usedPoints],
            SkillType = 1,
            MemberId = 0
        };

        if (!await TeamService.ModifySkillsAsync(request))
            return;

        await Ui.ReloadAsync(ReqStates.Team);
    }

    private async Task OnManageButtonAsync(int rowId)
    {
        var action = _vmBot.Rows[rowId].Action;

        if (action is null)
            return;

        if (action.Remark == MembRemark.Develop)
        {
            if (OnMemberSelected is not null)
                await OnMemberSelected.Invoke(action.MemberNo);

            return;
        }

        var modalType = action.Remark switch
        {
            MembRemark.Promote => ModalTypes.TPromote,
            MembRemark.Retire => ModalTypes.TRetire,
            MembRemark.Fire or MembRemark.Heal =>
                ModalTypes.THandle,
            _ => ModalTypes.None
        };

        if (modalType == ModalTypes.None)
            return;

        var modal = MBoxBuilder.BuildParam(
            modalType,
            Ui.Lang);

        if (action.Remark == MembRemark.Fire)
            modal = modal with { ActionText2 = string.Empty };

        modal.BodyParameters.Add(
            nameof(TModalRender.SelectedMember),
            action.MemberNo);
        modal.BodyParameters.Add(
            nameof(TModalRender.CanDidateNo),
            0);

        var result = await Ui.Modal.ShowAsync(modal);
        var requestType = ResolveManageType(
            action.Remark,
            result);

        if (requestType is null)
            return;

        if (!await TeamService.ManageTeamAsync(
            new ManageTeamRequest
            {
                ReqType = requestType.Value,
                MemberNo = action.MemberNo,
                CandidateId = 0
            }))
        {
            return;
        }

        ReqStates[] refreshTypes = requestType is
            ManageType.Retire or ManageType.Fire
                ? [
                    ReqStates.Question,
                    ReqStates.Home,
                    ReqStates.Team
                ]
                : [
                    ReqStates.Team,
                    ReqStates.VsGame
                ];

        await Ui.ReloadAsync(refreshTypes);
    }

    private void RebuildDevelopmentView()
    {
        _vmDev = _builder.BuildTeamBottomDevVm(
            Info,
            _usedPoints,
            Help,
            Culture);
    }

    private void ResetUsedPoints()
    {
        _usedPoints = [0, 0, 0, 0];
    }

    private static string GetActionStyle(
        MembRemark remark) => remark switch
    {
        MembRemark.Heal =>
            "background-color: darkslateblue;",
        MembRemark.Fire =>
            "background-color: #a64b2a;",
        MembRemark.Retire =>
            "background-color: forestgreen;",
        MembRemark.Promote =>
            "background-color: darkolivegreen;",
        _ => string.Empty
    };

    private static ManageType? ResolveManageType(
        MembRemark remark,
        ModalResult result)
    {
        return remark switch
        {
            MembRemark.Promote
                when result == ModalResult.Button1 =>
                    ManageType.Promote,
            MembRemark.Retire
                when result == ModalResult.Button1 =>
                    ManageType.Retire,
            MembRemark.Fire
                when result == ModalResult.Button1 =>
                    ManageType.Fire,
            MembRemark.Heal
                when result == ModalResult.Button1 =>
                    ManageType.Fire,
            MembRemark.Heal
                when result == ModalResult.Button2 =>
                    ManageType.Heal,
            _ => null
        };
    }
}
