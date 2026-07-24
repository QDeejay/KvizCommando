using KvizCommando.Client.Features.Team.Builders;
using KvizCommando.Client.Models.ViewModels;
using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Client.Utilities;
using KvizCommando.Shared.Models.Dtos;
using Microsoft.AspNetCore.Components;

namespace KvizCommando.Client.Features.Team;

public partial class Team : KcComponentBase, IDisposable
{
    [CascadingParameter]
    private AppState AppStates { get; set; } = default!;

    private readonly Dictionary<string, ContentBoxVm> _boxes = [];

    private string[] _boxOrder = [];
    private int _selectedMember;
    private bool _isReady;

    private string Culture => AppStates.Culture;
    private TeamDtos TeamData => AppStates.Team!;

    protected override void OnInitialized()
    {
        Ui.Header.OnBackBtnClicked += HandleBack;
        Ui.Header.SetTitle(Ui.Lang["mainlayout.Header.Team"], 2);
        _boxOrder = TBoxBuilder.Root;
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
                     Ui.Lang))
        {
            _boxes[box.Key] = box.Value;
        }

        _isReady = true;
    }

    private void OnBoxClick(int boxId)
    {
        _boxOrder = TBoxBuilder.Root;
        var headerTitle = Ui.Lang["mainlayout.Header.Team"];

        switch (boxId)
        {
            case 201:
                _selectedMember = 0;
                _boxOrder = TBoxBuilder.SubTeam;
                headerTitle = _boxes[
                    TBoxKeyRoot.RtBtnTeam.ToString()].Header;
                break;

            case 202:
                _boxOrder = TBoxBuilder.SubMember;
                headerTitle = _boxes[
                    TBoxKeyRoot.RtBtnMembers.ToString()].Header;
                break;

            case 203:
                _selectedMember = 0;
                _boxOrder = TBoxBuilder.SubRecruit;
                headerTitle = _boxes[
                    TBoxKeyRoot.RtBtnRecruit.ToString()].Header;
                break;

            default:
                _selectedMember = 0;
                break;
        }

        Ui.Header.SetTitle(headerTitle, boxId);
        Ui.Header.SetBackBtnEna(boxId != 2);
        StateHasChanged();
    }

    private Task OpenMemberAsync(int memberNo)
    {
        _selectedMember = memberNo;
        BuildBoxes();
        OnBoxClick(202);
        return Task.CompletedTask;
    }

    private Task ShowTeamOverviewAsync()
    {
        _selectedMember = 0;
        BuildBoxes();
        OnBoxClick(201);
        return Task.CompletedTask;
    }

    private void HandleBack()
    {
        if (Ui.Header.PageIndex == 2)
        {
            Ui.Nav.NavigateTo("/home");
            return;
        }

        _selectedMember = 0;
        BuildBoxes();
        OnBoxClick(2);
    }

    public void Dispose()
    {
        Ui.Header.OnBackBtnClicked -= HandleBack;
        GC.SuppressFinalize(this);
    }
}
