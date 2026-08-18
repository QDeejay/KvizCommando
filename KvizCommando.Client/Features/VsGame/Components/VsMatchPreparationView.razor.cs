using KvizCommando.Client.Features.VsGame.Services;
using KvizCommando.Client.Features.VsGame.ViewModels;
using KvizCommando.Client.Services.Audio;
using KvizCommando.Client.Services.Visual.UiService.Language;
using KvizCommando.Shared.Contracts.VsGame.Match;
using KvizCommando.Shared.Models.Enums.VsGame;
using Microsoft.AspNetCore.Components;
using System.Globalization;

namespace KvizCommando.Client.Features.VsGame.Components;

public partial class VsMatchPreparationView : IDisposable
{
    [Inject] private ILanguageService Lang { get; set; } = default!;
    [Inject] private AudioService Audio { get; set; } = default!;
    [Inject]
    private IVsMatchClientService MatchClient { get; set; } = default!;

    [Parameter, EditorRequired]
    public VsMatchViewData Data { get; set; } = new();

    [Parameter] public EventCallback<int> OnCharacterSelected { get; set; }
    [Parameter] public EventCallback<VsLoadoutAssignmentRequest>  OnLoadoutAssigned   { get; set; }
    [Parameter] public EventCallback<VsHelpAssignmentRequest>  OnHelpAssigned { get; set; }
    [Parameter] public EventCallback OnReset { get; set; }
    [Parameter] public EventCallback OnFinish { get; set; }

    private System.Threading.Timer? _timer;

    private int? _selectedLoadoutPosition;
    private int? _selectedCategoryId;
    private VsHelpType _selectedHelp = VsHelpType.None;
    private VsMatchPhase? _lastPhase;

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
                (Data.DeadlineUtc.Value - MatchClient.ServerUtcNow)
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
        if (_lastPhase != Data.Phase)
        {
            _lastPhase = Data.Phase;
            _selectedLoadoutPosition = null;
            _selectedCategoryId = null;
            _selectedHelp = VsHelpType.None;
        }

        if (_selectedLoadoutPosition.HasValue &&
            Data.Preparation.Loadout.All(item =>
                item.LoadoutPosition !=
                _selectedLoadoutPosition.Value))
        {
            _selectedLoadoutPosition = null;
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

    private string PreparationStepText => Data.Phase switch
    {
        VsMatchPhase.PreparationOrder => "1 / 3",
        VsMatchPhase.PreparationCategories => "2 / 3",
        VsMatchPhase.PreparationHelps => "3 / 3",
        _ => "3 / 3"
    };

    private string PreparationHeaderText =>
        Data.Phase == VsMatchPhase.PreparationCompleted
            ? Lang["vsgame.Match.Title.Preparation"]
            : Data.InfoText;

    private string InventoryTitle => Data.Phase switch
    {
        VsMatchPhase.PreparationOrder =>
            Lang["vsgame.Match.Inventory.Characters"],
        VsMatchPhase.PreparationCategories =>
            Lang["vsgame.Match.Inventory.Loadout"],
        VsMatchPhase.PreparationHelps =>
            Lang["vsgame.Match.Inventory.Helps"],
        _ => string.Empty
    };

    private string InventoryCssClass => Data.Phase switch
    {
        VsMatchPhase.PreparationOrder => "characters",
        VsMatchPhase.PreparationCategories => "categories",
        VsMatchPhase.PreparationHelps => "helps",
        _ => string.Empty
    };

    private string ResolveRoundName(VsPreparationRoundVm round)
    {
        if (round.IsCaptainRound)
        {
            return Data.Players
                       .FirstOrDefault(player => player.IsMe)?.DisplayName ??
                   Lang["vsgame.Match.Round.Captain"];
        }

        return round.Character?.Name ??
               Lang["vsgame.Match.Round.Empty"];
    }

    private async Task SelectCharacterAsync(int slotNumber)
    {
        if (Data.Preparation.IsFinished)
            return;

        await PlayClickAsync();
        await OnCharacterSelected.InvokeAsync(slotNumber);
    }

    private async Task SelectLoadoutAsync(VsLoadoutCardVm loadout)
    {
        if (!loadout.IsSelectable ||
            Data.Preparation.IsFinished)
        {
            return;
        }

        await PlayClickAsync();
        _selectedLoadoutPosition = loadout.LoadoutPosition;
        _selectedCategoryId = loadout.CategoryId;
    }

    private bool CanAssignCategory(
        VsPreparationRoundVm round) =>
        Data.Phase == VsMatchPhase.PreparationCategories &&
        !Data.Preparation.IsFinished &&
        !round.IsCaptainRound &&
        round.Loadout is null &&
        _selectedLoadoutPosition.HasValue;

    private async Task AssignCategoryAsync(
        VsPreparationRoundVm round)
    {
        if (!CanAssignCategory(round))
            return;

        await PlayClickAsync();
        await OnLoadoutAssigned.InvokeAsync(
            new VsLoadoutAssignmentRequest
            {
                LoadoutPosition =
                    _selectedLoadoutPosition!.Value,
                RoundNumber = round.RoundNumber
            });

        _selectedLoadoutPosition = null;
        _selectedCategoryId = null;
    }

    private string GetModifierText(VsPreparationRoundVm round)
    {
        var categoryId =
            round.Loadout?.CategoryId ??
            _selectedCategoryId;

        if (!categoryId.HasValue ||
            categoryId.Value is
                VsLoadoutCategoryIds.OwnQuestion or
                VsLoadoutCategoryIds.AllCategories)
        {
            return "—";
        }

        var seconds = Data.Preparation.CategoryModifiers
            .FirstOrDefault(item =>
                item.RoundNumber == round.RoundNumber &&
                item.CategoryId ==
                categoryId.Value)?.Seconds ?? 0;

        var prefix = seconds > 0 ? "+" : string.Empty;

        return prefix +
               seconds.ToString(
                   "0.0",
                   CultureInfo.InvariantCulture) +
               "s";
    }

    private string GetModifierClass(VsPreparationRoundVm round)
    {
        var text = GetModifierText(round);

        return text.StartsWith('+')
            ? "positive"
            : text.StartsWith('-')
                ? "negative"
                : string.Empty;
    }

    private async Task SelectHelpAsync(VsHelpCardVm help)
    {
        if (help.Count <= 0 ||
            Data.Preparation.IsFinished)
        {
            return;
        }

        await PlayClickAsync();
        _selectedHelp = help.HelpType;
    }

    private bool CanAssignHelp(
        VsPreparationRoundVm round) =>
        Data.Phase == VsMatchPhase.PreparationHelps &&
        !Data.Preparation.IsFinished &&
        _selectedHelp != VsHelpType.None &&
        round.Help is null &&
        (_selectedHelp is
            VsHelpType.FiftyFifty or
            VsHelpType.AiSuggestion ||
         !round.IsCaptainRound);

    private async Task AssignHelpAsync(
        VsPreparationRoundVm round)
    {
        if (!CanAssignHelp(round))
            return;

        await PlayClickAsync();
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
        if (!Data.Preparation.CanReset)
            return;

        await PlayClickAsync();
        _selectedLoadoutPosition = null;
        _selectedCategoryId = null;
        _selectedHelp = VsHelpType.None;
        await OnReset.InvokeAsync();
    }

    private async Task FinishAsync()
    {
        if (!Data.Preparation.CanFinish ||
            Data.Preparation.IsFinished)
        {
            return;
        }

        await PlayClickAsync();
        await OnFinish.InvokeAsync();
    }

    private Task PlayClickAsync() =>
        Audio.PlaySfxAsync(AudioService.SFX_CLICK);

    /// <inheritdoc />
    public void Dispose()
    {
        _timer?.Dispose();
        GC.SuppressFinalize(this);
    }
}
