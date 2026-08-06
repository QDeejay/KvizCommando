using KvizCommando.Client.Features.VsGame.Builders;
using KvizCommando.Client.Features.VsGame.Services;
using KvizCommando.Client.Features.VsGame.ViewModels;
using KvizCommando.Client.Helpers;
using KvizCommando.Client.Services.Audio;
using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Client.Services.Visual.UiService;
using KvizCommando.Client.Services.Visual.UiService.Language;
using KvizCommando.Shared.Contracts.VsGame.Match;
using KvizCommando.Shared.Models.Enums.VsGame;
using Microsoft.AspNetCore.Components;

namespace KvizCommando.Client.Features.VsGame.Components;

public partial class VsMatchManager : IAsyncDisposable
{
    [Inject] private IVsMatchClientService MatchClient { get; set; } = default!;

    [Inject] private ILanguageService Lang { get; set; } = default!;

    [Inject] private AudioService Audio { get; set; } = default!;

    [Inject] private UiServices Ui { get; set; } = default!;

    [Inject] private ILogger<VsMatchManager> Logger { get; set; } = default!;

    [CascadingParameter]
    private AppState AppStates { get; set; } = default!;

    [Parameter, EditorRequired] public int ClassificationId { get; set; }
    [Parameter] public EventCallback<bool> OnQuitConfirmationChanged { get; set; }
    [Parameter] public EventCallback<int> OnTeamLevelChanged { get; set; }

    private VsMatchViewBuilder _builder = default!;
    private VsQueueViewData? _queue;
    private VsMatchViewData? _match;
    private string _errorText = string.Empty;
    private readonly CancellationTokenSource _lifetimeCts = new();
    private bool _battleMusicStarted;
    private bool _requiresQuitConfirmation;
    private bool _completionHandled;
    private bool _disposed;
    private int _lastEffectQuestionNumber = -1;

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

        if (phase == VsMatchPhase.QuestionResult &&
            _match?.Game.QuestionKind == VsQuestionKind.Choice)
            await PlayQuestionResultEffectAsync();

        if (phase == VsMatchPhase.GameCompleted && !_completionHandled)
        {
            _completionHandled = true;
            await PlayMatchCompletionEffectAsync();
            await Ui.ReloadAsync(
                ReqStates.Home,
                ReqStates.Question,
                ReqStates.Team,
                ReqStates.VsGame);

            var newTeamLevel =
                _match?.Reward.MyReward?.NewTeamLevel ?? 0;

            if (newTeamLevel > 0)
                await OnTeamLevelChanged.InvokeAsync(newTeamLevel);
        }

        if (!_battleMusicStarted &&
            phase == VsMatchPhase.PreparationStarting)
        {
            _battleMusicStarted = true;
            await Audio.PlayMusicAsync(
                AudioService.MUSIC_BATTLE);
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

    private async Task SelectCharacterAsync(int slotNumber)
    {
        await PlaySelectionEffectAsync();
        await MatchClient.SelectCharacterAsync(slotNumber);
    }

    private async Task AssignLoadoutAsync(
        VsLoadoutAssignmentRequest request)
    {
        await PlaySelectionEffectAsync();
        await MatchClient.AssignLoadoutAsync(request);
    }

    private async Task AssignHelpAsync(
        VsHelpAssignmentRequest request)
    {
        await PlaySelectionEffectAsync();
        await MatchClient.AssignHelpAsync(request);
    }

    private Task ResetPreparationAsync() =>
        MatchClient.ResetPreparationAsync();

    private Task FinishPreparationAsync() =>
        MatchClient.FinishPreparationAsync();

    private async Task SubmitGuessAsync(
        VsGuessAnswerRequest request)
    {
        await PlaySelectionEffectAsync();
        await MatchClient.SubmitGuessAsync(request);
    }

    private async Task SubmitChoiceAsync(
        VsChoiceAnswerRequest request)
    {
        await PlaySelectionEffectAsync();
        await MatchClient.SubmitChoiceAsync(request);
    }

    private async Task UseHelpAsync(
        VsUseHelpRequest request)
    {
        await PlaySelectionEffectAsync();
        await MatchClient.UseHelpAsync(request);
    }

    private async Task SelectCaptainQuestionAsync(
        VsCaptainQuestionRequest request)
    {
        await PlaySelectionEffectAsync();
        await MatchClient.SelectCaptainQuestionAsync(request);
    }

    private Task PlaySelectionEffectAsync() =>
        Audio.PlaySfxAsync(AudioService.SFX_SELECT);

    private async Task PlayQuestionResultEffectAsync()
    {
        if (_match is null ||
            _match.Game.QuestionNumber ==
            _lastEffectQuestionNumber)
        {
            return;
        }

        _lastEffectQuestionNumber =
            _match.Game.QuestionNumber;

        var myPosition = _match.Players
            .FirstOrDefault(player => player.IsMe)
            ?.Position;
        if (!myPosition.HasValue)
            return;

        var result = _match.Game.QuestionPlayers
            .FirstOrDefault(player =>
                player.Position == myPosition.Value);

        if (result is null)
            return;

        var effect = !result.HasAnswered
            ? AudioService.SFX_EMPTY
            : result.IsCorrect
                ? AudioService.SFX_HIT
                : AudioService.SFX_MISS;

        await Audio.PlaySfxAsync(effect);
    }

    private Task PlayMatchCompletionEffectAsync()
    {
        var myReward = _match?.Reward.MyReward;
        if (myReward is null)
            return Task.CompletedTask;

        var effect = myReward.FinalPosition == 1
            ? AudioService.SFX_MATCH_WIN
            : myReward.FinalPosition ==
              _match!.Reward.Standings.Length
                ? AudioService.SFX_MATCH_LOSS
                : AudioService.SFX_MATCH_COMPLETE;

        return Audio.PlaySfxAsync(effect);
    }

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

    private string ConnectionResultText
    {
        get
        {
            var check = MatchClient.ConnectionCheck;

            return check is null
                ? string.Empty
                : Lang["vsgame.Match.Connection.ResponseTime"]
                    .FormatSafe(
                        check.ResponseTimeMilliseconds,
                        Lang[ConnectionQualityTextKey(
                            check.Quality)]);
        }
    }

    private string ConnectionResultClass =>
        MatchClient.ConnectionCheck?.Quality switch
        {
            VsConnectionQuality.Good => "good",
            VsConnectionQuality.Medium => "medium",
            VsConnectionQuality.Bad => "bad",
            _ => string.Empty
        };

    private static string ConnectionQualityTextKey(
        VsConnectionQuality quality) =>
        quality switch
        {
            VsConnectionQuality.Good =>
                "vsgame.Match.Connection.Good",
            VsConnectionQuality.Medium =>
                "vsgame.Match.Connection.Medium",
            VsConnectionQuality.Bad =>
                "vsgame.Match.Connection.Bad",
            _ => string.Empty
        };

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
                await Audio.PlayMusicAsync(
                    AudioService.MUSIC_MENU);
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
 * MÓDOSÍTÁS: az egyszeri kapcsolatellenőrzés lokalizált válaszidejét
 * és szerveroldali minősítését közvetlenül megjelenítési szöveggé
 * alakítja; a minősítési határokat nem ismétli meg kliensoldalon.
 * MÓDOSÍTÁS: a GameCompleted snapshotot egyszer dolgozza fel, majd a
 * tényleges új csapatszintet továbbítja a VS oldal előléptetési modaljához.
 * MÓDOSÍTÁS: a preparációs és játékbeli kijelöléseket közös rövid
 * effekt jelzi. A saját kérdéseredmény találat-, üres tár- vagy
 * mellélövés-hangot, a meccs vége helyezés szerinti győzelmi,
 * normál vagy vereséghangot kap; minden snapshot-esemény csak egyszer szól.
 */
