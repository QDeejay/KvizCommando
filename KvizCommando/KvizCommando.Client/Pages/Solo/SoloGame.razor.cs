using KvizCommando.Client.Models.ViewModels;
using KvizCommando.Client.Pages.Solo.Features;
using KvizCommando.Client.Pages.Solo.ViewModels;
using KvizCommando.Client.Services.Audio;
using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Client.Utilities;
using KvizCommando.Shared.Contracts.SoloGame;
using KvizCommando.Shared.Models.Dtos;
using Microsoft.AspNetCore.Components;
using System.Diagnostics;



namespace KvizCommando.Client.Pages.Solo
{
    public partial class SoloGame : KcComponentBase, IDisposable
    {
        [CascadingParameter]
        private AppState AppStates { get; set; } = default!;

        [Inject] AudioService Audio { get; set; } = default!;

        private readonly Dictionary<string, ContentBoxVm> _boxes = [];
        private readonly Stopwatch _gameWatch = new();
        private readonly Stopwatch _questionWatch = new();

        private readonly SoloPlayerViewData _player = new()
        {
            Name = "Karl Brimfield",
            RankName = "Közlegény",
            Level = 12,
            OrientationName = "Játékos",
            PictureCode = "M7270BG"
        };

        private string[] _boxOrder = [];
        private string _gameTitle = string.Empty;
        private SoloPagePhase _phase = SoloPagePhase.Selector;
        private StartSoloGameResponse? _game;
        private FinishSoloGameResponse? _result;
        private SoloAnswerDto[] _answers = [];
        private SoloQuestionState[] _progress = [];
        private TaskCompletionSource<int>? _answerSignal;
        private TaskCompletionSource? _skipSignal;
        private CancellationTokenSource? _gameCts;
        private int _questionIndex;
        private int _remainingSeconds;
        private int _points;
        private int _evaluatedCount;
        private bool _answerEnabled;
        private bool _isReady;
        private bool _isLoaded;
        private string _statusKey = string.Empty;

        private string Culture => AppStates.Culture;
        private SoloGameDtos SState => AppStates.SoloGame!;
        private string SelectorCss => _boxOrder.Length > SgameBoxBuilder.Root.Length
           ? "kc-solo-selector-sub"
           : "kc-solo-selector-root";
        private ContentBoxVm Box(string key) => _boxes[key];
        private SoloPlayViewData PlayViewData => new()
        {
            Player = _player,
            Game = new SoloGameViewData
            {
                Title = _gameTitle,
                Points = _points,
                CurrentQuestion = _game is null ? 0 : Math.Min(_questionIndex + 1, _game.QuestionCount),
                TotalQuestions = _game?.QuestionCount ?? 0,
                TotalSeconds = _game?.AnswerTimeSeconds ?? 0,
                RemainingSeconds = _remainingSeconds
            },
            Panel = BuildPanelData()
        };

        private void BuildBoxes()
        {
            foreach (var box in SgameBoxBuilder.BuildBoxes(SState, Culture, Ui.Lang))
                _boxes[box.Key] = box.Value;

            _isReady = _isLoaded;
        }
        protected override void OnInitialized()
        {
            Ui.Header.OnBackBtnClicked += UpdateBackClick;
        }
        protected override async Task OnInitializedAsync()
        {
            Ui.Header.SetTitle(Ui.Lang["mainlayout.Header.GameSolo"], 4);
            _boxOrder = SgameBoxBuilder.Root;
            _isLoaded = true;

            if (!_isReady)
                BuildBoxes();

            await Task.Delay(1);
        }

        private void OnBoxClick(int boxId)
        {
            if (boxId is >= 420 and <= 440)
            {
                BeginGame(SoloGameMode.Category, boxId - 420, boxId);
                return;
            }

            if (boxId is >= 450 and <= 460)
            {
                BeginGame(SoloGameMode.Orientation, boxId - 450, boxId);
                return;
            }

            _boxOrder = SgameBoxBuilder.Root;
            var headerTitle = Ui.Lang["mainlayout.Header.GameSolo"];

            switch (boxId)
            {
                case 401:
                    _boxOrder = SgameBoxBuilder.SubCat;
                    headerTitle = _boxes[SgameBoxKeyRoot.RtBtnCategory.ToString()].Header;
                    break;
                case 402:
                    _boxOrder = SgameBoxBuilder.SubOri;
                    headerTitle = _boxes[SgameBoxKeyRoot.RtBtnOrient.ToString()].Header;
                    break;
                case 403:
                    headerTitle = _boxes[SgameBoxKeyRoot.RtBtnCampaign.ToString()].Header;
                    break;
            }

            Ui.Header.SetTitle(headerTitle, boxId);
            Ui.Header.SetBackBtnEna(boxId > 4);
        }

        private void BeginGame(SoloGameMode mode, int selectionId, int boxId)
        {
            _boxOrder = mode == SoloGameMode.Category
                ? SgameBoxBuilder.GameCat
                : SgameBoxBuilder.GameOri;

            _gameTitle = mode == SoloGameMode.Category
               ? _boxes[$"{SgameBoxKeySub.BtnCat}{selectionId}"].Header
               : _boxes[$"{SgameBoxKeySub.BtnOri}{selectionId}"].Header;

            var title = Ui.Header.Title;

            Ui.Header.SetTitle(title, boxId);
            Ui.Header.SetBackBtnEna(true);

            _ = StartGameAsync(mode, selectionId);
        }

        private async Task StartGameAsync(SoloGameMode mode, int selectionId)
        {
            await Audio.PlayMusicAsync("Battle01.webm");
            CancelGameWork();
            _gameCts = new CancellationTokenSource();
            var ct = _gameCts.Token;
            _phase = SoloPagePhase.Status;
            _remainingSeconds = 0;

            try
            {
                await ShowStatusAsync("solo.Label.GameProcess.Preparing", 1000, ct);
                _statusKey = "solo.Label.GameProcess.Connecting";
                await InvokeAsync(StateHasChanged);

                _game = await Api.StartSoloGameAsync(new StartSoloGameRequest
                {
                    Mode = mode,
                    SelectionId = selectionId
                }, ct);

                if (_game is null)
                {
                    ShowFailure();
                    return;
                }

                _answers = [.. _game.Questions.Select(question => new SoloAnswerDto
                {
                    QuestionToken = question.QuestionToken,
                    SelectedOptionIndex = -1
                })];

                _progress = [.. Enumerable.Repeat(SoloQuestionState.Pending, _game.QuestionCount)];
                _points = _game.MaxPointsPerQuestion;
                _remainingSeconds = _game.AnswerTimeSeconds;

                await ShowStatusAsync("solo.Label.GameProcess.Starting", 1000, ct);
                await Audio.PlayMusicAsync("Battle01.webm");
                await PlayAsync(ct);
            }
            catch (OperationCanceledException)
            {
            }
        }
        private async Task PlayAsync(CancellationToken ct)
        {
            _phase = SoloPagePhase.Playing;
            _gameWatch.Restart();

            for (_questionIndex = 0; _questionIndex < _answers.Length; _questionIndex++)
            {
                _progress[_questionIndex] = SoloQuestionState.Active;
                await PlayQuestionAsync(ct);

                _answerEnabled = false;
                await InvokeAsync(StateHasChanged);

                await Task.Delay(1000, ct);
            }

            _gameWatch.Stop();
            await FinishGameAsync(ct);
        }
        private async Task PlayQuestionAsync(CancellationToken ct)
        {
            _answerSignal = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
            _questionWatch.Restart();
            _remainingSeconds = _game!.AnswerTimeSeconds;
            _answerEnabled = true;
            await InvokeAsync(StateHasChanged);

            while (_questionWatch.ElapsedMilliseconds < _game.AnswerTimeSeconds * 1000L)
            {
                if (_answerSignal.Task.IsCompleted)
                {
                    SaveAnswer(await _answerSignal.Task, (int)_questionWatch.ElapsedMilliseconds);
                    return;
                }

                _remainingSeconds = Math.Max(
                    0,
                    _game.AnswerTimeSeconds - (int)(_questionWatch.ElapsedMilliseconds / 1000));

                await InvokeAsync(StateHasChanged);
                await Task.Delay(100, ct);
            }

            _remainingSeconds = 0;
            SaveAnswer(-1, _game.AnswerTimeSeconds * 1000);
        }
        private void SaveAnswer(int selectedOptionIndex, int answerTimeMs)
        {
            _questionWatch.Stop();
            _answerEnabled = false;
            _answers[_questionIndex].SelectedOptionIndex = selectedOptionIndex;
            _answers[_questionIndex].AnswerTimeMs = Math.Clamp(
                answerTimeMs, 0, _game!.AnswerTimeSeconds * 1000);
            _progress[_questionIndex] = SoloQuestionState.Pending;
        }
        private Task SelectAnswerAsync(int answerIndex)
        {
            if (_phase == SoloPagePhase.Playing && _answerEnabled)
            {
                _answerEnabled = false;
                _answerSignal?.TrySetResult(answerIndex);
            }

            return Task.CompletedTask;
        }
        private async Task FinishGameAsync(CancellationToken ct)
        {
            _phase = SoloPagePhase.Status;
            _statusKey = "solo.Label.GameProcess.ProcessingQuestions";
            await InvokeAsync(StateHasChanged);

            _result = await Api.FinishSoloGameAsync(_game!.GameId, new FinishSoloGameRequest
            {
                ClientElapsedMs = (int)_gameWatch.ElapsedMilliseconds,
                Answers = _answers
            }, ct);

            if (_result is null)
            {
                ShowFailure();
                return;
            }
            await Audio.PlayMusicAsync("Menu02.webm");
            await ShowStatusAsync("solo.Label.GameProcess.Evaluating", 1000, ct);
            await ShowStatusAsync("solo.Label.GameProcess.EvaluationReady", 1000, ct);
            await EvaluateAsync(ct);
        }

        private async Task EvaluateAsync(CancellationToken ct)
        {
            _phase = SoloPagePhase.Evaluation;
            _points = 0;
            _evaluatedCount = 0;
            _skipSignal = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

            for (_questionIndex = 0; _questionIndex < _result!.AnswerResults.Length; _questionIndex++)
            {
                SetEvaluationTime(_questionIndex);

                _progress[_questionIndex] = _answers[_questionIndex].SelectedOptionIndex == -1
                    ? SoloQuestionState.Unanswered
                    : _result.AnswerResults[_questionIndex]
                        ? SoloQuestionState.Correct
                        : SoloQuestionState.Wrong;

                _evaluatedCount++;
                _points += _result.TotalPoints[_questionIndex];
                await InvokeAsync(StateHasChanged);

                var delay = Task.Delay(1000, ct);
                if (await Task.WhenAny(delay, _skipSignal.Task) == _skipSignal.Task)
                    break;
            }

            CompleteEvaluationProgress();
            await InvokeAsync(StateHasChanged);
            await Task.Delay(700, ct);
            ShowReward();
        }

        private void CompleteEvaluationProgress()
        {
            for (var i = _evaluatedCount; i < _result!.AnswerResults.Length; i++)
            {
                _progress[i] = _answers[i].SelectedOptionIndex == -1
                    ? SoloQuestionState.Unanswered
                    : _result.AnswerResults[i]
                        ? SoloQuestionState.Correct
                        : SoloQuestionState.Wrong;
            }

            _evaluatedCount = _result.AnswerResults.Length;
            _points = _result.TotalPoints.Sum();
        }

        private void SetEvaluationTime(int questionIndex)
        {
            var answerTimeMs = _answers[questionIndex].AnswerTimeMs;
            var elapsedSeconds = answerTimeMs / 1000;

            _remainingSeconds = Math.Max(
                _game!.AnswerTimeSeconds - elapsedSeconds,
                0);
        }
        private Task SkipEvaluationAsync()
        {
            _skipSignal?.TrySetResult();
            return Task.CompletedTask;
        }
        private void ShowReward()
        {
            _phase = SoloPagePhase.Reward;
            _points = _result?.TotalPoints.Sum() ?? 0;
            InvokeAsync(StateHasChanged);
        }
        private async Task ShowStatusAsync(string resourceKey, int delayMs, CancellationToken ct)
        {
            _phase = SoloPagePhase.Status;
            _statusKey = resourceKey;
            await InvokeAsync(StateHasChanged);
            await Task.Delay(delayMs, ct);
        }

        private void ShowFailure()
        {
            _phase = SoloPagePhase.Failed;
            _statusKey = "solo.Label.GameProcess.Aborted";
            _answerEnabled = false;
            InvokeAsync(StateHasChanged);
        }
        private SoloPanelViewData BuildPanelData()
        {
            if (_phase == SoloPagePhase.Reward)
                return BuildRewardPanel();

            if (_phase is SoloPagePhase.Status or SoloPagePhase.Failed)
            {
                return new SoloPanelViewData
                {
                    Mode = SoloPanelMode.Status,
                    DisplayLines = [new SoloDisplayLine { ResourceKey = _statusKey }],
                    Progress = _progress
                };
            }

            var question = _game?.Questions.ElementAtOrDefault(_questionIndex);

            return new SoloPanelViewData
            {
                Mode = _phase == SoloPagePhase.Evaluation
                    ? SoloPanelMode.Evaluation
                    : SoloPanelMode.Question,
                DisplayLines = question is null
                    ? []
                    : [new SoloDisplayLine { Text = question.Question }],
                Answers = question?.Answers ?? [],
                Progress = _progress,
                SelectedAnswerIndex = _answers.ElementAtOrDefault(_questionIndex)?.SelectedOptionIndex ?? -1,
                CurrentAnswerResult = _phase == SoloPagePhase.Evaluation &&
                                      _questionIndex < _evaluatedCount
                    ? _result?.AnswerResults[_questionIndex]
                    : null,
                AnswerEnabled = _answerEnabled,
                ShowSkip = _phase == SoloPagePhase.Evaluation
            };
        }
        private SoloPanelViewData BuildRewardPanel()
        {
            var answered = _answers.Count(answer => answer.SelectedOptionIndex >= 0);
            var totalPoints = _result?.TotalPoints.Sum() ?? 0;

            var lines = new List<SoloDisplayLine>
            {
                new() { ResourceKey = "solo.Label.Game.Reward.Answered", Value = $"{answered} / {_answers.Length}" },
                new() { ResourceKey = "solo.Label.Game.Reward.Correct", Value = (_result?.CorrectAnswers ?? 0).ToString() },
                new() { ResourceKey = "solo.Label.Game.Reward.Time", Value = FormatTime(_result?.TotalAnswerTimeMs ?? 0) },
                new() { ResourceKey = "solo.Label.Game.Reward.TotalPoints", Value = totalPoints.ToString() }
            };

            if (_result?.IsNewHighScore == true)
                lines.Add(new SoloDisplayLine
                {
                    ResourceKey = "solo.Label.Game.Message.NewRecord",
                    Emphasized = true
                });

            AddRewardLine(lines, "solo.Label.Game.Reward.TeamXp", _result?.Rewards.TeamXp ?? 0);
            AddRewardLine(lines, "solo.Label.Game.Reward.TeamDev", _result?.Rewards.TeamDevPoints ?? 0);
            AddRewardLine(lines, "solo.Label.Game.Reward.MemberXp", _result?.Rewards.MemberXp ?? 0);
            AddRewardLine(lines, "solo.Label.Game.Reward.MemberDev", _result?.Rewards.MemberDevPoints ?? 0);

            return new SoloPanelViewData
            {
                Mode = SoloPanelMode.Reward,
                DisplayLines = lines,
                Progress = _progress
            };
        }
        private static void AddRewardLine(List<SoloDisplayLine> lines, string resourceKey, int value)
        {
            if (value != 0)
                lines.Add(new SoloDisplayLine { ResourceKey = resourceKey, Value = value.ToString() });
        }
        private static string FormatTime(int milliseconds) =>
           TimeSpan.FromMilliseconds(milliseconds).ToString(@"mm\:ss");
        private void UpdateBackClick()
        {
            _ = HandleBackAsync();
        }
        private async Task HandleBackAsync()
        {
            var abandon = _game is not null &&
                _phase is SoloPagePhase.Playing or SoloPagePhase.Status;

            CancelGameWork();
            await Audio.PlayMusicAsync("Menu02.webm");
            if (abandon)
                await Api.AbandonSoloGameAsync(_game!.GameId);

            if (Ui.Header.PageIndex == 4)
            {
                Ui.Nav.NavigateTo("/home");
                return;
            }

            ResetGame();
            OnBoxClick(4);
            await InvokeAsync(StateHasChanged);
        }

        private void ResetGame()
        {
            _phase = SoloPagePhase.Selector;
            _game = null;
            _result = null;
            _answers = [];
            _progress = [];
            _statusKey = string.Empty;
            //_gameBoxKey = string.Empty;
        }

        private void CancelGameWork()
        {
            _answerEnabled = false;
            _gameCts?.Cancel();
            _gameCts?.Dispose();
            _gameCts = null;
        }
        public void Dispose()
        {
            Ui.Header.OnBackBtnClicked -= UpdateBackClick;
            CancelGameWork();
            GC.SuppressFinalize(this);
        }
        private enum SoloPagePhase
        {
            Selector,
            Status,
            Playing,
            Evaluation,
            Reward,
            Failed
        }
    }
}



/*
 
   [CascadingParameter]
        private AppState AppStates { get; set; } = default!;

        private readonly Dictionary<string, ContentBoxVm> _boxes = [];

        private string[] _boxOrder = [];

        private bool _isReady = false;
        private bool _isLoaded = false;

        private string SelectorCSS => _boxOrder.Length > SgameBoxBuilder.Root.Length ? "kc-solo-selector-sub" : "kc-solo-selector-root";

        private string Culture => AppStates.Culture;
        private SoloGameDtos SState => AppStates.SoloGame!;

        private ContentBoxVm Box(string orx) => _boxes[orx];
        private readonly string[] _previewAnswers =
            [
                "1848",
                "1873",
                "1896",
                "1918"
            ];

        private static Task SelectAnswerAsync(int answerIndex)
        {
            // Később itt megy az API-hívás.
            return Task.CompletedTask;
        }

        private void BuildBoxes()
        {
            /*
            var callback = new SoloCallbacks
            {

            };
var boxes = SgameBoxBuilder.BuildBoxes(SState, Culture, Ui.Lang);

foreach (var box in boxes)
{
    _boxes[box.Key] = box.Value;
}
_isReady = _isLoaded;
        }

        private void OnBoxClick(int boxId)
{
    _boxOrder = SgameBoxBuilder.Root;
    var headerTitle = string.Empty;
    switch (boxId)
    {
        case 4:
            headerTitle = Ui.Lang["mainlayout.Header.GameSolo"];
            break;
        case 401:
            _boxOrder = SgameBoxBuilder.SubCat;
            headerTitle = _boxes[SgameBoxKeyRoot.RtBtnCategory.ToString()].Header;
            break;
        case 402:
            _boxOrder = SgameBoxBuilder.SubOri;
            headerTitle = _boxes[SgameBoxKeyRoot.RtBtnOrient.ToString()].Header;
            break;
        case 403:
            headerTitle = _boxes[SgameBoxKeyRoot.RtBtnCampaign.ToString()].Header;
            break;
        case >= 420 and <= 440:
            _boxOrder = SgameBoxBuilder.GameCat;
            headerTitle = _boxes[SgameBoxKeyRoot.RtBtnCategory.ToString()].Header;
            break;
        case >= 450 and <= 460:
            _boxOrder = SgameBoxBuilder.GameOri;
            headerTitle = _boxes[SgameBoxKeyRoot.RtBtnOrient.ToString()].Header;
            break;
        default:
            headerTitle = Ui.Lang["mainlayout.Header.GameSolo"];
            break;
    }
    Ui.Header.SetTitle(headerTitle, boxId);
    Ui.Header.SetBackBtnEna(boxId > 4);
    StateHasChanged();
}

protected override async Task OnInitializedAsync()
{
    Ui.Header.SetTitle(Ui.Lang["mainlayout.Header.GameSolo"], 4);

    _boxOrder = SgameBoxBuilder.Root;

    _isLoaded = true;

    if (_isReady == false)
        BuildBoxes();

    await Task.Delay(1);
}
protected override void OnInitialized()
{
    Ui.Header.OnBackBtnClicked += UpdateBckClick;
}
private void UpdateBckClick()
{
    if (Ui.Header.PageIndex == 4)
        Ui.Nav.NavigateTo("/home");
    else
        OnBoxClick(4);
    InvokeAsync(StateHasChanged);
}
public void Dispose()
{
    Ui.Header.OnBackBtnClicked -= UpdateBckClick;
    GC.SuppressFinalize(this);
}


*/