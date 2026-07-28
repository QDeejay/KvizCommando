using KvizCommando.Client.Features.VsGame.Match.ViewModels;
using KvizCommando.Client.Services.Visual.UiService.Language;
using KvizCommando.Shared.Contracts.VsGame.Match;
using KvizCommando.Shared.Models.Enums.VsGame;
using Microsoft.AspNetCore.Components;
using System.Globalization;

namespace KvizCommando.Client.Features.VsGame.Match.Components;

public partial class VsMatchPreparationView : IDisposable
{
    [Inject] private ILanguageService Lang { get; set; } = default!;

    [Parameter, EditorRequired]
    public VsMatchViewData Data { get; set; } = new();

    [Parameter]
    public EventCallback<int> OnCharacterSelected { get; set; }

    [Parameter]
    public EventCallback<VsLoadoutAssignmentRequest>
        OnLoadoutAssigned { get; set; }

    [Parameter]
    public EventCallback<VsHelpAssignmentRequest>
        OnHelpAssigned { get; set; }

    [Parameter] public EventCallback OnReset { get; set; }
    [Parameter] public EventCallback OnFinish { get; set; }

    private System.Threading.Timer? _timer;

    private Guid? _selectedLoadoutToken;
    private int? _selectedCategoryId;
    private VsHelpType _selectedHelp = VsHelpType.None;

    protected override void OnInitialized()
    {
        _timer = new System.Threading.Timer(
            _ => _ = InvokeAsync(StateHasChanged),
            null,
            TimeSpan.FromMilliseconds(250),
            TimeSpan.FromMilliseconds(250));
    }

    private int RemainingSeconds => !Data.DeadlineUtc.HasValue
        ? 0
        : Math.Max(
            0,
            (int)Math.Ceiling(
                (Data.DeadlineUtc.Value - DateTime.UtcNow)
                .TotalSeconds));

    private double TimerPercent => Data.PhaseDurationSeconds <= 0
        ? 0
        : Math.Clamp(
            (double)RemainingSeconds /
            Data.PhaseDurationSeconds * 100,
            0,
            100);

    private string TimerStyle =>
        $"width: {TimerPercent.ToString(
            "0.##",
            CultureInfo.InvariantCulture)}%";

    protected override void OnParametersSet()
    {
        if (_selectedLoadoutToken.HasValue &&
            Data.Preparation.Loadout.All(item =>
                item.LoadoutToken != _selectedLoadoutToken.Value))
        {
            _selectedLoadoutToken = null;
            _selectedCategoryId = null;
        }

        var selectedHelp = Data.Preparation.Helps
            .FirstOrDefault(item =>
                item.HelpType == _selectedHelp);

        if (_selectedHelp != VsHelpType.None &&
            (selectedHelp is null ||
             selectedHelp.Count <= 0))
        {
            _selectedHelp = VsHelpType.None;
        }
    }

    private void SelectLoadout(VsLoadoutCardVm loadout)
    {
        if (!loadout.IsSelectable ||
            Data.Preparation.IsFinished)
        {
            return;
        }

        _selectedLoadoutToken = loadout.LoadoutToken;
        _selectedCategoryId = loadout.CategoryId;
    }

    private bool CanAssignCategory(
        VsPreparationRoundVm round) =>
        Data.Phase == VsMatchPhase.PreparationCategories &&
        !Data.Preparation.IsFinished &&
        !round.IsCaptainRound &&
        _selectedLoadoutToken.HasValue;

    private async Task AssignCategoryAsync(
        VsPreparationRoundVm round)
    {
        if (!CanAssignCategory(round))
            return;

        await OnLoadoutAssigned.InvokeAsync(
            new VsLoadoutAssignmentRequest
            {
                LoadoutToken =
                    _selectedLoadoutToken!.Value,
                RoundNumber = round.RoundNumber
            });

        _selectedLoadoutToken = null;
        _selectedCategoryId = null;
    }

    private string GetModifierText(int roundNumber)
    {
        if (!_selectedCategoryId.HasValue ||
            _selectedCategoryId.Value is
                VsLoadoutCategoryIds.OwnQuestion or
                VsLoadoutCategoryIds.AllCategories)
        {
            return "—";
        }

        var seconds = Data.Preparation.CategoryModifiers
            .FirstOrDefault(item =>
                item.RoundNumber == roundNumber &&
                item.CategoryId ==
                _selectedCategoryId.Value)?.Seconds ?? 0;

        var prefix = seconds > 0 ? "+" : string.Empty;

        return prefix +
               seconds.ToString(
                   "0.0",
                   CultureInfo.InvariantCulture) +
               "s";
    }

    private void SelectHelp(VsHelpCardVm help)
    {
        if (help.Count <= 0 ||
            Data.Preparation.IsFinished)
        {
            return;
        }

        _selectedHelp = help.HelpType;
    }

    private bool CanAssignHelp(
        VsPreparationRoundVm round) =>
        Data.Phase == VsMatchPhase.PreparationHelps &&
        !Data.Preparation.IsFinished &&
        _selectedHelp != VsHelpType.None &&
        (_selectedHelp is
            VsHelpType.FiftyFifty or
            VsHelpType.AiSuggestion ||
         !round.IsCaptainRound);

    private async Task AssignHelpAsync(
        VsPreparationRoundVm round)
    {
        if (!CanAssignHelp(round))
            return;

        await OnHelpAssigned.InvokeAsync(
            new VsHelpAssignmentRequest
            {
                HelpType = _selectedHelp,
                RoundNumber = round.RoundNumber
            });

        _selectedHelp = VsHelpType.None;
    }

    private async Task ResetAsync()
    {
        _selectedLoadoutToken = null;
        _selectedCategoryId = null;
        _selectedHelp = VsHelpType.None;
        await OnReset.InvokeAsync();
    }

    public void Dispose()
    {
        _timer?.Dispose();
        GC.SuppressFinalize(this);
    }
}

/**
 * A preparációs nézet lokális kijelöléseit, visszaszámlálását és
 * EventCallback-alapú parancstovábbítását kezeli; domainállapotot
 * nem módosít.
 */
