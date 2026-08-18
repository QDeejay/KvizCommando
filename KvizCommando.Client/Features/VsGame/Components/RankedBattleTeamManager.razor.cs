using KvizCommando.Client.Features.VsGame.Builders;
using KvizCommando.Client.Features.VsGame.Services;
using KvizCommando.Client.Features.VsGame.ViewModels;
using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Client.Services.Visual.UiService;
using KvizCommando.Client.Services.Visual.UiService.Language;
using KvizCommando.Shared.Contracts.VsGame;
using KvizCommando.Shared.Models.Dtos;
using Microsoft.AspNetCore.Components;

namespace KvizCommando.Client.Features.VsGame.Components;

public partial class RankedBattleTeamManager
{
    [Inject] private ILanguageService Lang { get; set; } = default!;
    [Inject] private IVsGameClientService GameService { get; set; } = default!;
    [Inject] private UiServices Ui { get; set; } = default!;

    [CascadingParameter]
    private AppState AppStates { get; set; } = default!;

    [Parameter]
    public Func<Task>? OnTeamSaved { get; set; }

    private readonly List<int> _selectedSlots = [];

    private VsBattleTeamBuilder _builder = default!;
    private VsBattleTeamVm _vm = new();
    private VsGameDtos? _previousSnapshot;
    private bool _isDirty;
    private bool _isReady;

    private string Culture => AppStates.Culture;
    private VsGameDtos VsData => AppStates.VsGame!;
    private int MaximumPartySize =>
        VsData.RankedBattlefields.Classifications
            .Max(rule => rule.RequiredPartySize);

    protected override void OnInitialized()
    {
        _builder = new VsBattleTeamBuilder(Lang);
    }

    protected override void OnParametersSet()
    {
        if (ReferenceEquals(_previousSnapshot, VsData))
            return;

        _selectedSlots.Clear();

        var savedSlots = VsData.RankedBattlefields
            .SavedSelection.SelectedSlotNumbers;

        var selectableSlots = VsData.RankedBattlefields
            .TeamMembers
            .Where(member => member.IsSelectable)
            .Select(member => member.SlotNumber)
            .ToHashSet();

        if (savedSlots.Length > 0 &&
            savedSlots.All(slot => slot > 0))
        {
            _selectedSlots.AddRange(
                savedSlots.Where(selectableSlots.Contains));
        }

        _isDirty = false;
        BuildViewModel();
        _previousSnapshot = VsData;
        _isReady = true;
    }

    private void ToggleMember(int slotNumber)
    {
        var member = _vm.Members.FirstOrDefault(
            item => item.SlotNumber == slotNumber);

        if (member?.IsSelectable != true)
            return;

        if (_selectedSlots.Contains(slotNumber))
        {
            _selectedSlots.Remove(slotNumber);
        }
        else
        {
            if (_selectedSlots.Count >= MaximumPartySize)
                return;

            _selectedSlots.Add(slotNumber);
        }

        _isDirty = HasSelectionChanged();
        BuildViewModel();
    }

    private async Task SaveAsync()
    {
        if (!_vm.CanSave)
            return;

        if (!await GameService.SaveBattleTeamAsync(
                new SaveBattleTeamRequest
                {
                    SelectedSlotNumbers = [.. _selectedSlots]
                }))
        {
            return;
        }

        await Ui.ReloadAsync(ReqStates.VsGame);

        if (OnTeamSaved is not null)
            await OnTeamSaved.Invoke();
    }

    private void BuildViewModel()
    {
        _vm = _builder.Build(
            VsData,
            _selectedSlots,
            _isDirty,
            Culture);
    }

    private bool HasSelectionChanged()
    {
        var savedSlots = VsData.RankedBattlefields
            .SavedSelection.SelectedSlotNumbers
            .Where(slot => slot > 0)
            .ToHashSet();

        return !savedSlots.SetEquals(_selectedSlots);
    }
}
