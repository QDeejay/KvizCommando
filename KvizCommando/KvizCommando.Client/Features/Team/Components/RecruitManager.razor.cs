using KvizCommando.Client.Features.Team.Builders;
using KvizCommando.Client.Features.Team.Services;
using KvizCommando.Client.Helpers;
using KvizCommando.Client.Models.ViewModels;
using KvizCommando.Client.Features.Shared.Modal.Dynamic;
using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Client.Services.Visual.UiService;
using KvizCommando.Client.Services.Visual.UiService.Language;
using KvizCommando.Shared.Contracts.Team;
using KvizCommando.Shared.Models.Dtos;
using Microsoft.AspNetCore.Components;
using KvizCommando.Client.Features.Shared.Modal.Builders;

namespace KvizCommando.Client.Features.Team.Components;

public partial class RecruitManager : IDisposable
{
    [Inject] private ILanguageService Lang { get; set; } = default!;
    [Inject] private ITeamClientService TeamService { get; set; } = default!;
    [Inject] private UiServices Ui { get; set; } = default!;

    [CascadingParameter]
    private AppState AppStates { get; set; } = default!;

    [Parameter]
    public Func<Task>? OnHireCompleted { get; set; }

    private readonly int[] _candidateOrder =
        [1, 2, 3, 4, 5, 6, 7, 8];

    private RecruitVm _vm = new();
    private TeamDtos? _previousTeam;
    private CandidateDto? _previousCandidate;
    private string _previousCulture = string.Empty;
    private int _selectedPosition;
    private int _selectedId;
    private bool _couldBeHire;
    private string _picCode = string.Empty;

    private string Culture => AppStates.Culture;
    private TeamDtos TeamData => AppStates.Team!;
    private bool[] CharacterMask => TeamData.CharCatMask[1..9];
    private bool[] CanHireMask =>
        TeamData.TeamInfo.AbleToHireMask[1..9];
    private CandidateDto[] Candidates =>
        TeamData.Candidates![1..9];
    private CandidateDto? Candidate =>
        _selectedPosition is >= 1 and <= 8
            ? TeamData.Candidates![_selectedPosition]
            : null;

    protected override void OnInitialized()
    {
        _candidateOrder.Shuffle();
        Ui.SubHeader.OnButtonClicked += HandleSubHeaderClicked;
    }

    protected override async Task OnInitializedAsync()
    {
        await RefreshExpiredCandidatesAsync();
    }

    protected override void OnParametersSet()
    {
        var snapshotChanged =  !ReferenceEquals(_previousTeam, TeamData);

        var cultureChanged = _previousCulture != Culture;

        var resolvedPosition = TeamHelpers.ResolvePosition(CanHireMask,_selectedPosition);

        var selectionChanged = resolvedPosition != _selectedPosition;

        _selectedPosition = resolvedPosition;

        if (snapshotChanged || cultureChanged)
            ShowSubHeader();

        if (snapshotChanged ||
            cultureChanged ||
            selectionChanged ||
            !ReferenceEquals(_previousCandidate, Candidate))
        {
            BuildSelectedCandidate();
        }

        _previousTeam = TeamData;
        _previousCulture = Culture;
    }

    private void ShowSubHeader()
    {
        var visibilities = CharacterMask
            .Select(value => !value)
            .ToArray();

        Ui.SubHeader.Show(
            TeamHelpers.SubHeaderResolver(
                visibilities,
                CanHireMask,
                TeamHelpers.RecruitToolTipResolver(
                    CharacterMask,
                    Candidates,
                    Lang),
                Culture),
            _selectedPosition);
    }

    private void BuildSelectedCandidate()
    {
        var candidate = Candidate;
        _couldBeHire = candidate?.CanBeHire == true;
        ResetSelection();

        if (!_couldBeHire || candidate is null)
        {
            _previousCandidate = candidate;
            return;
        }

        _vm = TBuilderRecruit.BuildRecruitVm(
            candidate,
            _candidateOrder,
            _selectedPosition,
            Culture,
            Lang);
        _previousCandidate = candidate;
    }

    private void HandleSubHeaderClicked(int index)
    {
        if (index is < 1 or > 8 ||
            !CanHireMask[index - 1] ||
            index == _selectedPosition)
        {
            return;
        }

        _selectedPosition = index;
        BuildSelectedCandidate();
        _ = InvokeAsync(StateHasChanged);
    }

    private async Task OnHireButtonAsync()
    {
        if (_selectedId == 0)
            return;

        var modal = MBoxBuilder.BuildParam(
            ModalTypes.THire,
            Ui.Lang);
        modal.BodyParameters.Add(
            nameof(TModalRender.SelectedMember),
            _selectedPosition);
        modal.BodyParameters.Add(
            nameof(TModalRender.CanDidateNo),
            _selectedId);

        if (await Ui.Modal.ShowAsync(modal) !=
            ModalResult.Button1)
        {
            return;
        }

        if (!await TeamService.ManageTeamAsync(
            new ManageTeamRequest
            {
                ReqType = ManageType.Hire,
                MemberNo = _selectedPosition,
                CandidateId = _selectedId
            }))
        {
            return;
        }

        await Ui.ReloadAsync(ReqStates.All);

        if (OnHireCompleted is not null)
            await OnHireCompleted.Invoke();
    }

    private void OnCandidateSelect(int id)
    {
        if (_selectedId == id)
            return;

        _selectedId = id;
        _picCode =
            Candidate?.PictureCode?[id - 1] ?? string.Empty;
    }

    private async Task RefreshExpiredCandidatesAsync()
    {
        if (Candidates.Any(candidate =>
                candidate is not null &&
                !candidate.CanBeHire &&
                candidate.ExpirationTime <= DateTime.UtcNow))
        {
            await Ui.ReloadAsync(ReqStates.Team);
        }
    }

    private void ResetSelection()
    {
        _selectedId = 0;
        _picCode = string.Empty;
    }

    public void Dispose()
    {
        Ui.SubHeader.OnButtonClicked -= HandleSubHeaderClicked;
        Ui.SubHeader.Hide();
        GC.SuppressFinalize(this);
    }
}
