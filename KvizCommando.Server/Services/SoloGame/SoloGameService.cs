using KvizCommando.Server.Domain.Entities.Questions;
using KvizCommando.Server.Domain.Entities.Statistics;
using KvizCommando.Server.Services.PlayerCache;
using KvizCommando.Server.Services.SoloGame.CategoryQuestionIndex;
using KvizCommando.Server.Services.SoloGame.GameCache;
using KvizCommando.Shared.Contracts.SoloGame;
using KvizCommando.Shared.Models;
using KvizCommando.Shared.Models.Rules;
using System.Text.Json;

namespace KvizCommando.Server.Services.SoloGame;

public sealed class SoloGameService : ISoloGameService
{
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

    /// <inheritdoc />
    public async Task<SoloStartResult> StartAsync(
            int playerId,
            StartSoloGameRequest request,
            CancellationToken ct = default)
    {
        var cacheResult = await _playerCache.GetOrLoadLockedAsync(
            playerId,
            request.SessionId,
            ct);

        if (cacheResult.Status == CacheReadStatus.SessionMismatch)
        {
            return new SoloStartResult
            {
                Status = SoloGameOperationStatus.SessionMismatch
            };
        }

        var player = cacheResult.Player;
        if (player is null)
        {
            return new SoloStartResult
            {
                Status = SoloGameOperationStatus.Rejected
            };
        }

        if (request.Mode is not SoloGameMode.Category and
            not SoloGameMode.Orientation ||
            request.SelectionId < 1 ||
            request.Mode == SoloGameMode.Orientation &&
            request.SelectionId > player.Characters.Length)
        {
            return new SoloStartResult
            {
                Status = SoloGameOperationStatus.Rejected
            };
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

            return new SoloStartResult
            {
                Status = SoloGameOperationStatus.Rejected
            };
        }

        var character = request.Mode == SoloGameMode.Orientation
            ? player.Characters[request.SelectionId - 1]
            : null;

        if (request.Mode == SoloGameMode.Orientation &&
            character is null)
        {
            return new SoloStartResult
            {
                Status = SoloGameOperationStatus.Rejected
            };
        }

        var utcNow = DateTime.UtcNow;
        var isHealing = character is not null &&
            TeamRules.CanStartSoloHealingGame(
                character.EnergyPoints,
                character.DevPoints,
                character.NextHealingGameUtc,
                utcNow);

        var level = request.Mode == SoloGameMode.Category
            ? player.Core.RankEnum
            : character!.Rank;
        var categoryIds = request.Mode == SoloGameMode.Category
            ? [request.SelectionId]
            : GetOrientationCategories(character!.Attitude.Main.CatNo);

        if (level < 0 || categoryIds.Length == 0)
            return new SoloStartResult
            {
                Status = SoloGameOperationStatus.Rejected
            };

        var questionCount = SoloGameRules.GetQuestionCount(level);
        var questionIds = GetQuestionIds(categoryIds, questionCount);

        if (questionIds.Count != questionCount)
            return new SoloStartResult
            {
                Status = SoloGameOperationStatus.Rejected
            };

        var entities = await _questionRepository.LoadByIdsAsync(
            questionIds,
            ct);

        if (entities.Count != questionCount)
            return new SoloStartResult
            {
                Status = SoloGameOperationStatus.Rejected
            };

        var entityMap = entities.ToDictionary(question => question.Id);
        var questions = new List<CachedSoloQuestion>(questionCount);

        foreach (var questionId in questionIds)
        {
            var question = CreateQuestion(entityMap[questionId]);
            if (question is null)
                return new SoloStartResult
                {
                    Status = SoloGameOperationStatus.Rejected
                };

            questions.Add(question);
        }

        var gameTime = TimeSpan.FromSeconds(
            questionCount *
            (SoloGameRules.ANSWER_SECONDS + FEEDBACK_SECONDS));
        var game = new SoloGameSession
        {
            GameId = Guid.NewGuid(),
            PlayerId = playerId,
            SessionId = request.SessionId,
            Mode = request.Mode,
            SelectionId = request.SelectionId,
            Level = level,
            IsHealing = isHealing,
            PointsPerLevel = SoloGameRules.GetMaxPointsPerQuestion(level),
            ExpiresAtUtc = utcNow.Add(gameTime)
                .AddSeconds(EXPIRATION_ALLOWANCE_SECONDS),
            Questions = questions
        };

        if (!_gameCache.TryCreate(game))
        {
            return new SoloStartResult
            {
                Status = SoloGameOperationStatus.Rejected
            };
        }

        return new SoloStartResult
        {
            Status = SoloGameOperationStatus.Success,
            Response = new StartSoloGameResponse
            {
                GameId = game.GameId,
                IsHealing = game.IsHealing,
                QuestionCount = questionCount,
                AnswerTimeSeconds = SoloGameRules.ANSWER_SECONDS,
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
            }
        };
    }

    /// <inheritdoc />
    public async Task<SoloAnswerResult> SubmitAnswerAsync(
        int playerId,
        Guid gameId,
        SoloAnswerDto answer,
        CancellationToken ct = default)
    {
        if (!_gameCache.TryGet(gameId, out var game) || game is null)
        {
            return new SoloAnswerResult
            {
                Status = SoloGameOperationStatus.Rejected
            };
        }

        await game.Lock.WaitAsync(ct);
        try
        {
            if (game.PlayerId != playerId ||
                game.Status != SoloGameStatus.Active ||
                DateTime.UtcNow > game.ExpiresAtUtc ||
                game.Answers.Count >= game.Questions.Count ||
                answer.SelectedOptionIndex is < -1 or > 3 ||
                answer.AnswerTimeMs is < 0 or
                    > SoloGameRules.ANSWER_SECONDS * 1000)
            {
                return new SoloAnswerResult
                {
                    Status = SoloGameOperationStatus.Rejected
                };
            }

            game.Answers.Add(new SoloAnswerDto
            {
                SelectedOptionIndex = answer.SelectedOptionIndex,
                AnswerTimeMs = answer.AnswerTimeMs
            });

            if (game.Answers.Count < game.Questions.Count)
            {
                return new SoloAnswerResult
                {
                    Status = SoloGameOperationStatus.Success
                };
            }

            game.Status = SoloGameStatus.Finishing;
            var result = await CompleteAsync(game, ct);

            if (result.Status == SoloGameOperationStatus.Success)
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

    /// <inheritdoc />
    public async Task<SoloGameOperationStatus> AbandonAsync(
        int playerId,
        Guid gameId,
        string sessionId,
        CancellationToken ct = default)
    {
        if (!_gameCache.TryGet(gameId, out var game) || game is null)
            return SoloGameOperationStatus.Rejected;

        await game.Lock.WaitAsync(ct);
        try
        {
            if (game.PlayerId != playerId || game.SessionId != sessionId)
                return SoloGameOperationStatus.SessionMismatch;

            game.Status = SoloGameStatus.Abandoned;
            _gameCache.Remove(gameId);
            return SoloGameOperationStatus.Success;
        }
        finally
        {
            game.Lock.Release();
        }
    }

    private async Task<SoloAnswerResult> CompleteAsync(
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

        if (highScore.Status != CacheUpdateResult.Updated)
        {
            return new SoloAnswerResult
            {
                Status = ToSoloStatus(highScore.Status)
            };
        }

        var correctAnswers = answerResults.Count(result => result == true);
        var wrongAnswers = answerResults.Count(result => result != true);
        var reward = await CreateRewardAsync(
            game,
            points.Sum(),
            highScore.OldScore,
            correctAnswers,
            wrongAnswers,
            ct);

        if (reward.Status != CacheUpdateResult.Updated)
        {
            return new SoloAnswerResult
            {
                Status = ToSoloStatus(reward.Status)
            };
        }

        return new SoloAnswerResult
        {
            Status = SoloGameOperationStatus.Success,
            Response = new FinishSoloGameResponse
            {
                TotalPoints = points,
                AnswerResults =
                [
                    .. answerResults.Select(result => result == true)
                ],
                CorrectAnswers = correctAnswers,
                WrongAnswers = wrongAnswers,
                TotalAnswerTimeMs = totalTimeMs,
                IsNewHighScore = highScore.IsNewHighScore,
                Rewards = reward.Reward
            }
        };
    }

    private async Task<(CacheUpdateResult Status, bool IsNewHighScore, int OldScore)>
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

    private async Task<(CacheUpdateResult Status, SoloRewardDto Reward)>
        CreateRewardAsync(
            SoloGameSession game,
            int newScore,
            int oldScore,
            int correctAnswers,
            int wrongAnswers,
            CancellationToken ct)
    {
        var earnedDevelopmentPoints =
            game.Mode == SoloGameMode.Category
                ? SoloGameRules.GetEarnedScoreDevelopmentPoints(
                    newScore,
                    oldScore)
                : 0;
        var teamXp = 0;
        var memberXp = 0;
        var teamDevelopmentPoints = 0;
        var memberDevelopmentPoints = 0;
        var newTeamLevel = 0;
        var isMemberXpCapped = false;
        var healingPointAwarded = false;
        var healingTargetReached =
            SoloGameRules.HasEarnedHeartReward(
                correctAnswers,
                game.Questions.Count);

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

                    earnedDevelopmentPoints = SoloGameRules.GetEarnedScoreDevelopmentPoints(
                        newScore,
                        member.CharStatistic.SoloBestScore);

                    if (newScore > member.CharStatistic.SoloBestScore)
                        member.CharStatistic.SoloBestScore = newScore;

                    var rewardUtc = DateTime.UtcNow;
                    var canAwardHealingPoint =
                        game.IsHealing &&
                        healingTargetReached &&
                        TeamRules.CanStartSoloHealingGame(
                            member.EnergyPoints,
                            member.DevPoints,
                            member.NextHealingGameUtc,
                            rewardUtc);

                    if (canAwardHealingPoint)
                    {
                        healingPointAwarded = true;
                        member.NextHealingGameUtc =
                            TeamRules.GetNextSoloHealingGameUtc(
                                rewardUtc);
                    }

                    memberDevelopmentPoints =
                        earnedDevelopmentPoints +
                        (healingPointAwarded
                            ? SoloGameRules.HEART_REWARD_DEVELOPMENT_POINTS
                            : 0);

                    var earnedMemberXp =
                        SoloGameRules.GetMemberExperience(
                            game.PointsPerLevel,
                            correctAnswers,
                            wrongAnswers,
                            game.Level);
                    memberXp =
                        TeamRules.GetCreditableMemberExperience(
                            earnedMemberXp,
                            member.Rank,
                            member.XP);
                    isMemberXpCapped = memberXp < earnedMemberXp;
                    teamXp = SoloGameRules.GetTeamExperience(
                        memberXp,
                        game.Level);

                    member.DevPoints += memberDevelopmentPoints;
                    member.XP += memberXp;
                }

                if (teamXp > 0)
                {
                    var oldTeamLevel = player.Core.RankEnum;
                    player.Core.XP += teamXp;

                    while (player.Core.RankEnum <= TeamRules.LAST_XP_LEVEL &&
                           player.Core.XP >= RankRewards
                               .List[player.Core.RankEnum].NextLevelTeam)
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
            IsMemberXpCapped = isMemberXpCapped,
            MemberDevPoints = memberDevelopmentPoints,
            HealingPointAwarded = healingPointAwarded
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

    private static int CalculateAnswerPoints(
        int maximumPoints,
        int elapsedMs,
        bool? isCorrect) =>
        SoloGameRules.GetAnswerPoints(
            maximumPoints,
            elapsedMs,
            isCorrect);

    private static SoloGameOperationStatus ToSoloStatus(
        CacheUpdateResult result) =>
        result == CacheUpdateResult.SessionMismatch
            ? SoloGameOperationStatus.SessionMismatch
            : SoloGameOperationStatus.Rejected;

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
