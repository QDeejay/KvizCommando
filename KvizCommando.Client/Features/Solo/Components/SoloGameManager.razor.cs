using KvizCommando.Client.Data;
using KvizCommando.Client.Features.Solo.Services;
using KvizCommando.Client.Features.Solo.ViewModels;
using KvizCommando.Client.Helpers;
using KvizCommando.Client.Services.Audio;
using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Client.Services.Visual.UiService;
using KvizCommando.Shared.Contracts.SoloGame;
using KvizCommando.Shared.Models.Dtos;
using KvizCommando.Shared.Models.Enums.VsGame;
using KvizCommando.Shared.Models.Rules;
using KvizCommando.Shared.Models.User;
using Microsoft.AspNetCore.Components;
using System.Diagnostics;

namespace KvizCommando.Client.Features.Solo.Components;

/// <summary>
/// A teljes szólójáték kliensoldali folyamatát vezérli.
/// </summary>
public partial class SoloGameManager : IAsyncDisposable
{
[Inject] private ISoloGameClientService GameService { get; set; } = default!;
    [Inject] private AudioService Audio { get; set; } = default!;
    [Inject] private UiServices Ui { get; set; } = default!;
    [Inject] private ILogger<SoloGameManager> Logger { get; set; } = default!;

    [CascadingParameter]
    private AppState AppStates { get; set; } = default!;

    /// <summary>A kiválasztott szólójátékmód.</summary>
    [Parameter] public SoloGameMode Mode { get; set; }

    /// <summary>A kiválasztott kategória vagy orientáció azonosítója.</summary>
    [Parameter] public int SelectionId { get; set; }

    /// <summary>A játék nézetében megjelenő cím.</summary>
    [Parameter] public string Title { get; set; } = string.Empty;

    /// <summary>A játék befejezését jelző visszahívás.</summary>
    [Parameter] public EventCallback<bool> OnGameCompletedChanged { get; set; }

    /// <summary>A csapatszint változását továbbító visszahívás.</summary>
    [Parameter] public EventCallback<int> OnTeamLevelChanged { get; set; }

    private readonly CancellationTokenSource _lifetimeCts = new();
    private readonly Stopwatch _questionWatch = new();

    private SoloPlayerViewData _player = new();
    private StartSoloGameResponse? _game;
    private FinishSoloGameResponse? _result;
    private SoloAnswerDto[] _answers = [];
    private SoloQuestionState[] _progress = [];
    private TaskCompletionSource<int>? _answerSignal;
    private TaskCompletionSource? _skipSignal;
    private Task? _gameTask;
    private bool _hasActiveGame;
    private SoloGamePhase _phase = SoloGamePhase.Status;
    private int _questionIndex;
    private int _remainingSeconds;
    private int _points;
    private int _evaluatedCount;
    private int _evaluatedCorrectAnswers;
    private bool _answerEnabled;
    private bool _isDisposed;
    private string _statusKey = "solo.Label.GameProcess.Connecting";

    private string Culture => AppStates.Culture;
    private UserMainData UserData => AppStates.Home!.UserMainData;
    private TeamMemberDto[] Members => AppStates.Team!.TeamMembers;
    private bool IsExperienceGame
    {
        get
        {
            if (Mode != SoloGameMode.Orientation ||
                SelectionId is < 1 or > 8)
            {
                return false;
            }

            TeamMemberDto? member = Members[SelectionId];

            return member is not null &&
                   SoloGameRules.CanEarnMemberExperience(
                       member.Level);
        }
    }

    private SoloPlayViewData PlayViewData => new()
    {
        Player = _player,
        Game = new SoloGameViewData
        {
            Title = Title,
            Points = _points,
            CurrentQuestion = _game is null
                ? 0
                : Math.Min(_questionIndex + 1, _game.QuestionCount),
            TotalQuestions = _game?.QuestionCount ?? 0,
            TotalSeconds = _game?.AnswerTimeSeconds ?? 0,
            RemainingSeconds = _remainingSeconds,
            ResponseTimeMilliseconds =
                GameService.ConnectionCheck
                    ?.ResponseTimeMilliseconds ?? 0,
            ConnectionQuality =
                GameService.ConnectionCheck?.Quality ??
                VsConnectionQuality.Unknown,
            IsConnectionActive = GameService.IsConnected,
            IsExperienceGame = IsExperienceGame,
            IsHealing = _game?.IsHealing == true,
            IsHealingCompleted =
                _game is { IsHealing: true } game &&
                _evaluatedCorrectAnswers * 2 >= game.QuestionCount
        },
        Panel = BuildPanelData()
    };

    protected override void OnInitialized()
    {
        _player = BuildPlayerProfile();
        GameService.OnChanged += HandleConnectionChanged;
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
            _gameTask = RunGameAsync(_lifetimeCts.Token);
    }

    private Task RenderAsync() =>
        _isDisposed
            ? Task.CompletedTask
            : InvokeAsync(StateHasChanged);

    private void HandleConnectionChanged() =>
        _ = InvokeAsync(RefreshConnectionState);

    private void RefreshConnectionState()
    {
        if (_isDisposed)
            return;

        if (_hasActiveGame &&
            !string.IsNullOrWhiteSpace(
                GameService.ErrorMessageKey))
        {
            _questionWatch.Stop();
            _answerEnabled = false;
            _phase = SoloGamePhase.Failed;
            _statusKey = GameService.ErrorMessageKey;
            _lifetimeCts.Cancel();
        }

        StateHasChanged();
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_isDisposed)
            return;

        _isDisposed = true;
        GameService.OnChanged -= HandleConnectionChanged;
        _answerEnabled = false;
        _lifetimeCts.Cancel();
        _answerSignal?.TrySetCanceled();
        _skipSignal?.TrySetResult();

        if (_gameTask is not null)
            await _gameTask;

        var hasActiveGame = _hasActiveGame;
        _hasActiveGame = false;

        if (hasActiveGame)
            await GameService.AbandonAsync();

        await GameService.StopAsync();

        try
        {
            await Audio.PlayMusicAsync(
                MusicTrack.MenuMain);
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Failed to restore menu music.");
        }

        _lifetimeCts.Dispose();
        GC.SuppressFinalize(this);
    }

    private enum SoloGamePhase
    {
        Status,
        Playing,
        Evaluation,
        Reward,
        Failed
    }
}
