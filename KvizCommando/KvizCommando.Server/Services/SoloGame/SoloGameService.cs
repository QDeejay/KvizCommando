using KvizCommando.Server.Domain.Entities.Questions;
using KvizCommando.Server.Domain.Entities.Statistics;
using KvizCommando.Server.Services.PlayerCache;
using KvizCommando.Server.Services.SoloGame.CategoryQuestionIndex;
using KvizCommando.Server.Services.SoloGame.GameCache;
using KvizCommando.Shared.Constants;
using KvizCommando.Shared.Contracts.SoloGame;
using KvizCommando.Shared.Models;
using System.Text.Json;

namespace KvizCommando.Server.Services.SoloGame;

public sealed class SoloGameService : ISoloGameService
{
    private const int ANSWER_SECONDS = 20;
    private const int FEEDBACK_SECONDS = 2;
    private const int EXPIRATION_ALLOWANCE_SECONDS = 10;

    private readonly IPlayerCacheService _playerCache;
    private readonly ICategoryQuestionIndexCache _questionIndex;
    private readonly ISoloQuestionRepository _questionRepository;
    private readonly ISoloGameCache _gameCache;

    public SoloGameService(
        IPlayerCacheService playerCache,
        ICategoryQuestionIndexCache questionIndex,
        ISoloQuestionRepository questionRepository,
        ISoloGameCache gameCache)
    {
        _playerCache = playerCache;
        _questionIndex = questionIndex;
        _questionRepository = questionRepository;
        _gameCache = gameCache;
    }

    public async Task<(StartSoloGameResponse? Response, bool? Success)>
        StartAsync(
            int playerId,
            StartSoloGameRequest request,
            CancellationToken ct = default)
    {
        var (player, _) = await _playerCache.GetOrLoadLockedAsync(
            playerId,
            request.SessionId,
            ct);

        if (player is null)
            return (null, false);

        if (player.SessionId == "denied")
            return (null, null);

        if (request.Mode is not SoloGameMode.Category and
            not SoloGameMode.Orientation ||
            request.SelectionId < 1 ||
            request.Mode == SoloGameMode.Orientation &&
            request.SelectionId > player.Characters.Length)
        {
            return (null, false);
        }

        if (_gameCache.TryGetActiveGame(playerId, out var activeGame) &&
            activeGame is not null)
        {
            await activeGame.Lock.WaitAsync(ct);
            try
            {
                activeGame.Status = SoloGameStatus.Abandoned;
                _gameCache.Remove(activeGame.GameId);
            }
            finally
            {
                activeGame.Lock.Release();
            }

            return (null, false);
        }

        var character = request.Mode == SoloGameMode.Orientation
            ? player.Characters[request.SelectionId - 1]
            : null;

        if (request.Mode == SoloGameMode.Orientation &&
            character is null)
        {
            return (null, false);
        }

        var level = request.Mode == SoloGameMode.Category
            ? player.Core.RankEnum
            : character!.Rank;
        var categoryIds = request.Mode == SoloGameMode.Category
            ? [request.SelectionId]
            : GetOrientationCategories(character!.Attitude.Main.CatNo);

        if (level < 0 || categoryIds.Length == 0)
            return (null, false);

        var questionCount = GetQuestionCount(level);
        var questionIds = GetQuestionIds(categoryIds, questionCount);

        if (questionIds.Count != questionCount)
            return (null, false);

        var entities = await _questionRepository.LoadByIdsAsync(
            questionIds,
            ct);

        if (entities.Count != questionCount)
            return (null, false);

        var entityMap = entities.ToDictionary(question => question.Id);
        var questions = new List<CachedSoloQuestion>(questionCount);

        foreach (var questionId in questionIds)
        {
            var question = CreateQuestion(entityMap[questionId]);
            if (question is null)
                return (null, false);

            questions.Add(question);
        }

        var now = DateTime.UtcNow;
        var gameTime = TimeSpan.FromSeconds(
            questionCount * (ANSWER_SECONDS + FEEDBACK_SECONDS));
        var game = new SoloGameSession
        {
            GameId = Guid.NewGuid(),
            PlayerId = playerId,
            SessionId = request.SessionId,
            Mode = request.Mode,
            SelectionId = request.SelectionId,
            Level = level,
            isHealing = character?.EnergyPoints == 0,
            PointsPerLevel = 100 + level / 2 * 10,
            ExpiresAtUtc = now.Add(gameTime)
                .AddSeconds(EXPIRATION_ALLOWANCE_SECONDS),
            Questions = questions
        };

        if (!_gameCache.TryCreate(game))
            return (null, false);

        return (new StartSoloGameResponse
        {
            GameId = game.GameId,
            QuestionCount = questionCount,
            AnswerTimeSeconds = ANSWER_SECONDS,
            FeedbackTimeSeconds = FEEDBACK_SECONDS,
            MaxPointsPerQuestion = game.PointsPerLevel,
            Questions =
            [
                .. questions.Select(question => new SoloQuestionDto
                {
                    Question = question.Question,
                    Answers = question.Answers
                })
            ]
        }, true);
    }

    public async Task<(FinishSoloGameResponse? Response, bool? Success)>
        SubmitAnswerAsync(
            int playerId,
            Guid gameId,
            SoloAnswerDto answer,
            CancellationToken ct = default)
    {
        if (!_gameCache.TryGet(gameId, out var game) || game is null)
            return (null, false);

        await game.Lock.WaitAsync(ct);
        try
        {
            if (game.PlayerId != playerId ||
                game.Status != SoloGameStatus.Active ||
                DateTime.UtcNow > game.ExpiresAtUtc ||
                game.Answers.Count >= game.Questions.Count ||
                answer.SelectedOptionIndex is < -1 or > 3 ||
                answer.AnswerTimeMs is < 0 or > ANSWER_SECONDS * 1000)
            {
                return (null, false);
            }

            game.Answers.Add(new SoloAnswerDto
            {
                SelectedOptionIndex = answer.SelectedOptionIndex,
                AnswerTimeMs = answer.AnswerTimeMs
            });

            if (game.Answers.Count < game.Questions.Count)
                return (null, true);

            game.Status = SoloGameStatus.Finishing;
            var result = await CompleteAsync(game, ct);

            if (result.Success == true)
            {
                game.Status = SoloGameStatus.Completed;
                _gameCache.Remove(gameId);
            }

            return result;
        }
        finally
        {
            game.Lock.Release();
        }
    }

    public async Task<bool?> AbandonAsync(
        int playerId,
        Guid gameId,
        string sessionId,
        CancellationToken ct = default)
    {
        if (!_gameCache.TryGet(gameId, out var game) || game is null)
            return false;

        await game.Lock.WaitAsync(ct);
        try
        {
            if (game.PlayerId != playerId || game.SessionId != sessionId)
                return null;

            game.Status = SoloGameStatus.Abandoned;
            _gameCache.Remove(gameId);
            return true;
        }
        finally
        {
            game.Lock.Release();
        }
    }

    private async Task<(FinishSoloGameResponse? Response, bool? Success)>
        CompleteAsync(
            SoloGameSession game,
            CancellationToken ct)
    {
        var answerResults = game.Questions.Select((question, index) =>
        {
            var answer = game.Answers[index];
            return answer.SelectedOptionIndex == -1
                ? (bool?)null
                : answer.SelectedOptionIndex ==
                  question.CorrectOptionIndex;
        }).ToArray();
        var points = game.Questions.Select((question, index) =>
            CalculateAnswerPoints(
                game.PointsPerLevel,
                game.Answers[index].AnswerTimeMs,
                answerResults[index])).ToArray();
        var totalTimeMs = game.Answers.Sum(answer => answer.AnswerTimeMs);
        var highScore = await SaveResultAsync(
            game,
            points.Sum(),
            totalTimeMs,
            ct);

        if (highScore.Success != true)
            return (null, highScore.Success);

        var reward = await CreateRewardAsync(
            game,
            points.Sum(),
            highScore.OldScore,
            ct);

        if (reward.Success != true)
            return (null, reward.Success);

        return (new FinishSoloGameResponse
        {
            TotalPoints = points,
            AnswerResults =
            [
                .. answerResults.Select(result => result == true)
            ],
            CorrectAnswers = answerResults.Count(result => result == true),
            WrongAnswers = answerResults.Count(result => result != true),
            TotalAnswerTimeMs = totalTimeMs,
            IsNewHighScore = highScore.IsNewHighScore,
            Rewards = reward.Reward
        }, true);
    }

    private async Task<(bool? Success, bool IsNewHighScore, int OldScore)>
        SaveResultAsync(
            SoloGameSession game,
            int newScore,
            int totalTimeMs,
            CancellationToken ct)
    {
        var oldScore = 0;
        var isNewHighScore = false;
        var totalSeconds = totalTimeMs / 1000d;

        var success = await _playerCache.UpdatePlayerLockedAsync(
            game.PlayerId,
            game.SessionId,
            player =>
            {
                if (game.Mode == SoloGameMode.Category)
                {
                    var statistic = player.CategoryStats.FirstOrDefault(
                        item => item.CategoryId == game.SelectionId);

                    if (statistic is null)
                    {
                        statistic = new PlayerCategoryStat
                        {
                            PlayerId = game.PlayerId,
                            CategoryId = (short)game.SelectionId
                        };
                        player.CategoryStats.Add(statistic);
                    }

                    oldScore = statistic.HighScore;
                    isNewHighScore = IsBetter(
                        newScore,
                        totalSeconds,
                        statistic.HighScore,
                        statistic.HighScoreTime);

                    if (isNewHighScore)
                    {
                        statistic.HighScore = newScore;
                        statistic.HighScoreTime = totalSeconds;
                    }

                    return DirtyFlags.CategoryStats;
                }

                var orientationStatistic = player.OrientStats
                    .FirstOrDefault(item =>
                        item.OrientId == game.SelectionId);

                if (orientationStatistic is null)
                {
                    orientationStatistic = new PlayerOrientStat
                    {
                        PlayerId = game.PlayerId,
                        OrientId = (short)game.SelectionId
                    };
                    player.OrientStats.Add(orientationStatistic);
                }

                oldScore = orientationStatistic.HighScore;
                isNewHighScore = IsBetter(
                    newScore,
                    totalSeconds,
                    orientationStatistic.HighScore,
                    orientationStatistic.HighScoreTime);

                if (isNewHighScore)
                {
                    orientationStatistic.HighScore = newScore;
                    orientationStatistic.HighScoreTime = totalSeconds;
                }

                return DirtyFlags.OrientStats;
            },
            ct);

        return (success, isNewHighScore, oldScore);
    }

    private async Task<(bool? Success, SoloRewardDto Reward)>
        CreateRewardAsync(
            SoloGameSession game,
            int newScore,
            int oldScore,
            CancellationToken ct)
    {
        var earnedDevelopmentPoints = Math.Max(
            ScoreConstants.ScorLimits.Count(value => newScore >= value) -
            ScoreConstants.ScorLimits.Count(value => oldScore >= value),
            0);
        var teamXp = 0;
        var memberXp = 0;
        var teamDevelopmentPoints = 0;
        var memberDevelopmentPoints = 0;
        var newTeamLevel = 0;

        var success = await _playerCache.UpdatePlayerLockedAsync(
            game.PlayerId,
            game.SessionId,
            player =>
            {
                if (game.Mode == SoloGameMode.Category)
                {
                    teamDevelopmentPoints = earnedDevelopmentPoints;
                    player.Core.DevPoint += teamDevelopmentPoints;
                }
                else
                {
                    var member = player.Characters[game.SelectionId - 1];
                    if (member is null)
                        return null;

                    memberDevelopmentPoints =
                        earnedDevelopmentPoints + (game.isHealing ? 1 : 0);

                    if (game.Level == 0 && newScore > 0)
                    {
                        memberXp = newScore / 10;
                        teamXp = memberXp / 2;
                    }

                    member.DevPoints += memberDevelopmentPoints;
                    member.XP += memberXp;
                }

                if (teamXp > 0)
                {
                    var oldTeamLevel = player.Core.RankEnum;
                    player.Core.XP += teamXp;

                    while (player.Core.RankEnum <= 21 &&
                           player.Core.XP >= RankRewards
                               .List[player.Core.RankEnum].NextLevel)
                    {
                        player.Core.RankEnum++;
                        var storedDevelopmentPoints = RankRewards
                            .List[player.Core.RankEnum].DevPointToStore;
                        player.Core.DevPoint += storedDevelopmentPoints;
                        teamDevelopmentPoints += storedDevelopmentPoints;
                    }

                    if (player.Core.RankEnum > oldTeamLevel)
                        newTeamLevel = player.Core.RankEnum;
                }

                return game.Mode == SoloGameMode.Category
                    ? DirtyFlags.Core
                    : teamXp > 0
                        ? DirtyFlags.Core | DirtyFlags.Characters
                        : DirtyFlags.Characters;
            },
            ct);

        return (success, new SoloRewardDto
        {
            TeamXp = teamXp,
            TeamDevPoints = teamDevelopmentPoints,
            NewTeamLevel = newTeamLevel,
            MemberXp = memberXp,
            MemberDevPoints = memberDevelopmentPoints
        });
    }

    private List<int> GetQuestionIds(int[] categoryIds, int questionCount)
    {
        var result = new List<int>(questionCount);
        var categoryQuestionCount = questionCount / categoryIds.Length;

        foreach (var categoryId in categoryIds)
        {
            var ids = _questionIndex.GetQuestionIds(categoryId);
            if (ids.Count < categoryQuestionCount)
                return [];

            var selectedIndexes = new HashSet<int>();
            while (selectedIndexes.Count < categoryQuestionCount)
            {
                var index = Random.Shared.Next(ids.Count);
                if (selectedIndexes.Add(index))
                    result.Add(ids[index]);
            }
        }

        Shuffle(result);
        return result;
    }

    private static CachedSoloQuestion? CreateQuestion(
        FactoryQuestion question)
    {
        var answers = JsonSerializer.Deserialize<string[]>(
            question.AnswersJson);

        if (answers is null || answers.Length != 4)
            return null;

        var correctAnswer = answers[0];
        Shuffle(answers);

        return new CachedSoloQuestion
        {
            QuestionId = question.Id,
            Question = question.Question,
            Answers = answers,
            CorrectOptionIndex = Array.IndexOf(answers, correctAnswer)
        };
    }

    private static int[] GetOrientationCategories(int[] categoryIds)
    {
        var first = categoryIds[0];
        var second = categoryIds[2];
        return first > 0 && second > 0 ? [first, second] : [];
    }

    private static int GetQuestionCount(int level) =>
        level switch
        {
            <= 0 => 8,
            >= 19 => 20,
            _ => 10 + (level - 1) / 4 * 2
        };

    private static int CalculateAnswerPoints(
        int maximumPoints,
        int elapsedMs,
        bool? isCorrect)
    {
        if (isCorrect is null)
            return 0;

        var decreasingTimeMs = Math.Clamp(elapsedMs - 5000, 0, 15000);
        var multiplier = 1.0 - decreasingTimeMs / 15000.0;
        var points = (int)Math.Round(
            maximumPoints * multiplier,
            MidpointRounding.AwayFromZero);

        return isCorrect.Value ? points : -points;
    }

    private static bool IsBetter(
        int score,
        double time,
        int oldScore,
        double oldTime) =>
        score > 0 &&
        (score > oldScore ||
         score == oldScore && (oldTime <= 0 || time < oldTime));

    private static void Shuffle<T>(IList<T> values)
    {
        for (var index = values.Count - 1; index > 0; index--)
        {
            var other = Random.Shared.Next(index + 1);
            (values[index], values[other]) =
                (values[other], values[index]);
        }
    }
}

/**
 * MÓDOSÍTÁS: a Solo játék kizárólag SignalR parancsokat kezel. A régi
 * HTTP start/finish út, a teljes finish request, elapsed validáció és
 * kérdéstoken megszűnt. A kapcsolat sorrendje azonosítja az aktuális
 * kérdést; az utolsó elfogadott válasz közvetlenül lezárja a játékot.
 * MÓDOSÍTÁS: nulla pontos eredmény nem minősül új rekordnak.
 */
