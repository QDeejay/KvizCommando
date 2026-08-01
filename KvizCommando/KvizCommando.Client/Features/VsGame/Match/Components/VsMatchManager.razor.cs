using KvizCommando.Client.Features.VsGame.Match.Builders;
using KvizCommando.Client.Features.VsGame.Match.Services;
using KvizCommando.Client.Features.VsGame.Match.ViewModels;
using KvizCommando.Client.Services.Audio;
using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Client.Services.Visual.UiService;
using KvizCommando.Client.Services.Visual.UiService.Language;
using KvizCommando.Shared.Contracts.VsGame.Match;
using KvizCommando.Shared.Models.Enums.VsGame;
using Microsoft.AspNetCore.Components;

namespace KvizCommando.Client.Features.VsGame.Match.Components;

public partial class VsMatchManager : IAsyncDisposable
{
    [Inject]
    private IVsMatchClientService MatchClient { get; set; } = default!;

    [Inject]
    private ILanguageService Lang { get; set; } = default!;

    [Inject]
    private AudioService Audio { get; set; } = default!;

    [Inject]
    private UiServices Ui { get; set; } = default!;

    [Inject]
    private ILogger<VsMatchManager> Logger { get; set; } = default!;

    [CascadingParameter]
    private AppState AppStates { get; set; } = default!;

    [Parameter, EditorRequired]
    public int ClassificationId { get; set; }

    [Parameter]
    public EventCallback<bool> OnQuitConfirmationChanged { get; set; }

    private VsMatchViewBuilder _builder = default!;
    private VsQueueViewData? _queue;
    private VsMatchViewData? _match;
    private string _errorText = string.Empty;
    private readonly CancellationTokenSource _lifetimeCts = new();
    private bool _battleMusicStarted;
    private bool _requiresQuitConfirmation;
    private bool _disposed;

    protected override async Task OnInitializedAsync()
    {
        _builder = new VsMatchViewBuilder(Lang);
        MatchClient.OnChanged += OnMatchClientChanged;

        try
        {
            var result =
                await MatchClient.StartAsync(
                    ClassificationId,
                    _lifetimeCts.Token);

            if (result.IsAccepted)
            {
                await BuildViewDataAsync();
            }
            else
            {
                _errorText = Lang[result.ErrorKey];

                if (result.ErrorKey ==
                    "vsgame.Match.Error.QueueValidation")
                {
                    await Ui.ReloadAsync(
                        ReqStates.Question,
                        ReqStates.Team,
                        ReqStates.VsGame);
                }
            }
        }
        catch (OperationCanceledException) when (_disposed)
        {
        }
        catch
        {
            _errorText =
                Lang["vsgame.Match.Error.Connection"];
        }
    }

    private void OnMatchClientChanged()
    {
        _ = InvokeAsync(RefreshFromSnapshotAsync);
    }

    private async Task RefreshFromSnapshotAsync()
    {
        if (_disposed)
            return;

        await BuildViewDataAsync();
        StateHasChanged();
    }

    private async Task BuildViewDataAsync()
    {
        _queue = MatchClient.QueueSnapshot is null
            ? null
            : _builder.BuildQueue(
                MatchClient.QueueSnapshot);

        _match = MatchClient.MatchSnapshot is null
            ? null
            : _builder.Build(
                MatchClient.MatchSnapshot,
                AppStates.Culture);

        _errorText =
            string.IsNullOrWhiteSpace(
                MatchClient.ErrorMessageKey)
                ? string.Empty
                : Lang[MatchClient.ErrorMessageKey];

        var phase = _match?.Phase;

        if (phase == VsMatchPhase.GameCompleted)
        {
            await Ui.ReloadAsync(
                ReqStates.Home,
                ReqStates.Question,
                ReqStates.Team,
                ReqStates.VsGame);
        }

        if (!_battleMusicStarted &&
            phase == VsMatchPhase.PreparationStarting)
        {
            _battleMusicStarted = true;
            await Audio.PlayMusicAsync("Battle01.webm");
        }

        var requiresQuitConfirmation =
            phase.HasValue &&
            phase != VsMatchPhase.GameCompleted;

        if (_requiresQuitConfirmation ==
            requiresQuitConfirmation)
        {
            return;
        }

        _requiresQuitConfirmation =
            requiresQuitConfirmation;

        await OnQuitConfirmationChanged.InvokeAsync(
            requiresQuitConfirmation);
    }

    private Task SelectCharacterAsync(int slotNumber) =>
        MatchClient.SelectCharacterAsync(slotNumber);

    private Task AssignLoadoutAsync(
        VsLoadoutAssignmentRequest request) =>
        MatchClient.AssignLoadoutAsync(request);

    private Task AssignHelpAsync(
        VsHelpAssignmentRequest request) =>
        MatchClient.AssignHelpAsync(request);

    private Task ResetPreparationAsync() =>
        MatchClient.ResetPreparationAsync();

    private Task FinishPreparationAsync() =>
        MatchClient.FinishPreparationAsync();

    private Task SubmitGuessAsync(
        VsGuessAnswerRequest request) =>
        MatchClient.SubmitGuessAsync(request);

    private Task SubmitChoiceAsync(
        VsChoiceAnswerRequest request) =>
        MatchClient.SubmitChoiceAsync(request);

    private Task UseHelpAsync(
        VsUseHelpRequest request) =>
        MatchClient.UseHelpAsync(request);

    private Task SelectCaptainQuestionAsync(
        VsCaptainQuestionRequest request) =>
        MatchClient.SelectCaptainQuestionAsync(request);

    private bool IsGamePhase =>
        _match?.Phase is
            VsMatchPhase.GameStarting or
            VsMatchPhase.NormalRoundGuess or
            VsMatchPhase.NormalRoundQuestion or
            VsMatchPhase.QuestionResult or
            VsMatchPhase.NormalRoundResult or
            VsMatchPhase.CaptainQuestionSelection or
            VsMatchPhase.CaptainQuestion or
            VsMatchPhase.CaptainRoundResult or
            VsMatchPhase.GameCompleted;

    private bool IsPreparationPhase =>
        _match?.Phase is
            VsMatchPhase.PreparationOrder or
            VsMatchPhase.PreparationCategories or
            VsMatchPhase.PreparationHelps or
            VsMatchPhase.PreparationCompleted;

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        _disposed = true;
        _lifetimeCts.Cancel();
        MatchClient.OnChanged -= OnMatchClientChanged;

        try
        {
            try
            {
                await MatchClient.LeaveQueueAsync();
            }
            finally
            {
                await MatchClient.StopAsync();
            }
        }
        finally
        {
            try
            {
                await Audio.PlayMusicAsync("Menu02.webm");
            }
            catch (Exception ex)
            {
                Logger.LogWarning(
                    ex,
                    "Failed to restore menu music.");
            }

            _lifetimeCts.Dispose();
        }

        GC.SuppressFinalize(this);
    }
}

/**
 * MÓDOSÍTÁS: a manager az egyetlen SignalR-kapcsolat tulajdonosa.
 * Közvetlenül feldolgozza a queue-belépési eredményt és helyben tartja
 * a lokalizált hibaállapotot; match lock- és hibacallbacket nem küld a
 * VS lap, builder vagy spec felé. Dispose során hivatalosan kilép a
 * queue-ból, majd minden esetben lezárja a kapcsolatot. A saját
 * életciklus-token megszakítja a még folyamatban lévő csatlakozást.
 * A minden játékost összegyűjtő visszaszámlálás kezdetén a Solo
 * játékkal azonos harci zenét indítja, kilépéskor pedig visszaállítja
 * a menüzenét. Queue-ban és befejezett meccsnél nem kér kilépési
 * megerősítést.
 * MÓDOSÍTÁS: ha egy korábban kilépett játékos elavult kliensadatai
 * miatt a szerver elutasítja a következő queue-belépést, a kérdés-,
 * csapat- és VS snapshotot egyszer frissíti. Így a meccs közben
 * bottá vált kliens a lezáráskori cache-változásokat is megkapja.
 *
 * MÓDOSÍTÁS: az explicit játékmeneti parancsokat, köztük a segítség
 * használatát is változtatás nélkül továbbítja, és kijelöli a roster
 * pontnézetének, valamint a természetes magasságú preparációnak a
 * fázisait.
 *
 * A VS ranked DynamicComponent életciklusát kezeli, snapshotból view
 * modelleket készít és továbbítja a preparációs/játékparancsokat.
 */
