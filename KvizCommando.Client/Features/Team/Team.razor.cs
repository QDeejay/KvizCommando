using KvizCommando.Client.Features.Home.Builders;
using KvizCommando.Client.Features.Team.Builders;
using KvizCommando.Client.Models.ViewModels;
using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Client.Utilities;
using KvizCommando.Localization.Team;
using KvizCommando.Shared.Models.Dtos;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace KvizCommando.Client.Features.Team;

public partial class Team : KcComponentBase, IDisposable
{
    [Inject] private IStringLocalizer<TeamResource> Lang { get; set; } = default!;

    [CascadingParameter]
    private AppState AppStates { get; set; } = default!;

    private readonly Dictionary<string, ContentBoxVm> _boxes = [];

    private string[] _boxOrder = [];
    private int _selectedMember;
    private bool _isReady;

    private TeamDtos TeamData => AppStates.Team!;

    protected override void OnInitialized()
    {
        Ui.Header.OnBackBtnClicked += HandleBack;
        Ui.Header.SetTitle(
            AppStates.BoxTitles["Root.Team"],
            (int)HomeBoxKey.Team);
        Ui.Header.SetBackBtnEna(false);
        _boxOrder = TBoxBuilder.Root;
        BuildBoxes();
        _isReady = true;
    }

    private ContentBoxVm Box(string key) => _boxes[key];

    private void BuildBoxes()
    {
        var parameters = new TeamComponentParameters
        {
            OnMemberSelected = OpenMemberAsync,
            OnHireCompleted = ShowTeamOverviewAsync,
            SelectedMember = _selectedMember
        };

        foreach (var box in TBoxBuilder.BuildBoxes(
                     TeamData.RootBoxInfo,
                     parameters,
                     AppStates.BoxTitles,
                     Lang))
        {
            _boxes[box.Key] = box.Value;
        }
    }

    private void OnBoxClick(int boxId)
    {
        _boxOrder = TBoxBuilder.Root;
        var headerTitle = AppStates.BoxTitles["Root.Team"];

        switch (boxId)
        {
            case (int)TBoxKeyRoot.TeamOverview:
                _selectedMember = 0;
                _boxOrder = TBoxBuilder.SubTeam;
                headerTitle = _boxes[
                    TBoxKeyRoot.TeamOverview.ToString()].Header;
                break;

            case (int)TBoxKeyRoot.Members:
                _boxOrder = TBoxBuilder.SubMember;
                headerTitle = _boxes[
                    TBoxKeyRoot.Members.ToString()].Header;
                break;

            case (int)TBoxKeyRoot.Recruit:
                _selectedMember = 0;
                _boxOrder = TBoxBuilder.SubRecruit;
                headerTitle = _boxes[
                    TBoxKeyRoot.Recruit.ToString()].Header;
                break;

            default:
                _selectedMember = 0;
                break;
        }

        Ui.Header.SetTitle(headerTitle, boxId);
        Ui.Header.SetBackBtnEna(boxId != (int)HomeBoxKey.Team);
        StateHasChanged();
    }

    private Task OpenMemberAsync(int memberNo)
    {
        _selectedMember = memberNo;

        BuildBoxes();

        OnBoxClick((int)TBoxKeyRoot.Members);

        return Task.CompletedTask;
    }

    private Task ShowTeamOverviewAsync()
    {
        _selectedMember = 0;
        BuildBoxes();

        if (TeamData.RootBoxInfo.IsRecruitEnable)
            OnBoxClick((int)TBoxKeyRoot.Recruit);
        else
            OnBoxClick((int)TBoxKeyRoot.TeamOverview);

        return Task.CompletedTask;
    }

    private void HandleBack()
    {
        if (Ui.Header.PageIndex == (int)HomeBoxKey.Team)
        {
            Ui.Nav.NavigateTo("/home");
            return;
        }

        _selectedMember = 0;
        BuildBoxes();
        OnBoxClick((int)HomeBoxKey.Team);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Ui.Header.OnBackBtnClicked -= HandleBack;
        GC.SuppressFinalize(this);
    }
}
