using KvizCommando.Server.Domain.Entities.Questions;
using KvizCommando.Server.Domain.Entities.Statistics;
using KvizCommando.Server.Services.PlayerCache;
using KvizCommando.Server.Services.SoloGame.CategoryQuestionIndex;
using KvizCommando.Server.Services.SoloGame.GameCache;
using KvizCommando.Shared.Constants;
using KvizCommando.Shared.Contracts.SoloGame;
using System.Text.Json;

namespace KvizCommando.Server.Services.SoloGame
{
    public sealed class SoloGameService : ISoloGameService
    {
        private const int ANSWER_SECONDS = 20;
        private const int FEEDBACK_SECONDS = 2;
        private const int GRACE_SECONDS = 60;

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

        public async Task<(StartSoloGameResponse? Response, bool? Success)> StartAsync(
            int playerId,
            StartSoloGameRequest request,
            CancellationToken ct = default)
        {
            var (player, _) = await _playerCache.GetOrLoadLockedAsync(playerId, request.SessionId, ct);

            if (player is null)
                return (null, false);

            if (player.SessionId == "denied")
                return (null, null);

            if (_gameCache.TryGetActiveGame(
                    playerId,
                    request.SessionId,
                    out var activeGame) &&
                    activeGame is not null)
            {
                await AbandonAsync(
                    playerId,
                    activeGame.GameId,
                    request.SessionId,
                    ct);

                return (null, false);
            }

            var level = request.Mode == SoloGameMode.Category
                ? player.Core.RankEnum
                : player.Characters[request.SelectionId - 1].Rank;

            if (level < 0)
                return (null, false);

            var isHealing = request.Mode == SoloGameMode.Orientation && player.Characters[request.SelectionId - 1].EnergyPoints == 0;

            var categoryIds = request.Mode == SoloGameMode.Category
                ? [request.SelectionId]
                : GetOrientationCategories(player, request.SelectionId);

            if (categoryIds.Length == 0)
                return (null, false);

            var questionCount = GetQuestionCount(level);

            var selectedIds = GetQuestionIds(categoryIds, questionCount);

            if (selectedIds.Count != questionCount)
                return (null, false);

            var entities = await _questionRepository.LoadByIdsAsync(selectedIds, ct);
            if (entities.Count != questionCount)
                return (null, false);

            var entityMap = entities.ToDictionary(question => question.Id);

            var cachedQuestions = new List<CachedSoloQuestion>();

            foreach (var questionId in selectedIds)
            {
                var cachedQuestion = CreateCachedQuestion(entityMap[questionId]);

                if (cachedQuestion is null)
                    return (null, false);

                cachedQuestions.Add(cachedQuestion);
            }

            var now = DateTime.UtcNow;
            var gameTime = TimeSpan.FromSeconds(questionCount * (ANSWER_SECONDS + FEEDBACK_SECONDS));

            var maxPointPerQuestion = 100 + level / 2 * 10;

            var game = new SoloGameSession
            {
                GameId = Guid.NewGuid(),
                PlayerId = playerId,
                SessionId = request.SessionId,
                Mode = request.Mode,
                SelectionId = request.SelectionId,
                Level = level,
                isHealing = isHealing,
                StartedAtUtc = now,
                PointsPerLevel = maxPointPerQuestion,
                GameplayDeadlineUtc = now.Add(gameTime),
                ExpiresAtUtc = now.Add(gameTime).AddSeconds(GRACE_SECONDS),
                Questions = cachedQuestions
            };

            if (_gameCache.TryCreate(game) == false)
                return (null, false);

            var response = new StartSoloGameResponse
            {
                GameId = game.GameId,
                QuestionCount = questionCount,
                AnswerTimeSeconds = ANSWER_SECONDS,
                FeedbackTimeSeconds = FEEDBACK_SECONDS,
                MaxPointsPerQuestion = maxPointPerQuestion,
                Questions = [.. cachedQuestions.Select(question => new SoloQuestionDto
                {
                    QuestionToken = question.QuestionToken,
                    Question = question.Question,
                    Answers = question.Answers
                })]
            };

            return (response, true);
        }



        public async Task<(FinishSoloGameResponse? Response, bool? Success)> FinishAsync(
            int playerId,
            Guid gameId,
            FinishSoloGameRequest request,
            CancellationToken ct = default)
        {
            if (_gameCache.TryGet(gameId, out var game) == false || game is null)
                return (null, false);

            await game.Lock.WaitAsync(ct);
            try
            {
                if (game.PlayerId != playerId || game.SessionId != request.SessionId)
                    return (null, null);

                if (game.Status != SoloGameStatus.Active || ValidateFinish(game, request) == false)
                    return (null, false);

                game.Status = SoloGameStatus.Finishing;

                var submittedAnswers = request.Answers.ToDictionary(answer => answer.QuestionToken);

                var answerResults = game.Questions
                                         .Select(question =>
                                         {
                                             var answer = submittedAnswers[question.QuestionToken];

                                             if (answer.SelectedOptionIndex == -1)
                                                 return (bool?)null;

                                             return answer.SelectedOptionIndex ==
                                                    question.CorrectOptionIndex;
                                         })
                                         .ToArray();
                var correctAnswers =
                     answerResults.Count(result => result == true);

                var wrongAnswers =
                    answerResults.Count(result => result == false);

                var unansweredAnswers =
                    answerResults.Count(result => result == null);

                var Points = game.Questions
                        .Select((question, index) =>
                        {
                            var answer =
                                submittedAnswers[question.QuestionToken];

                            return CalculateAnswerPoints(
                                game.PointsPerLevel,
                                answer.AnswerTimeMs,
                                answerResults[index]);
                        }).ToArray();

                var clientResults = answerResults
                                        .Select(result => result == true)
                                        .ToArray();

                var totalTimeMs = request.Answers.Sum(answer => answer.AnswerTimeMs);

                var highScoreResult = await SaveResultAsync(game, Points.Sum(), totalTimeMs, ct);

                if (highScoreResult.Success != true)
                    return (null, highScoreResult.Success);

                var rewards = await CreateRewardsAsync(game, Points.Sum(), highScoreResult.OldScore, ct);

                if (rewards.Success != true)
                    return (null, rewards.Success);

                var response = new FinishSoloGameResponse
                {
                    TotalPoints = Points,
                    AnswerResults = clientResults,
                    CorrectAnswers = correctAnswers,
                    WrongAnswers = wrongAnswers + unansweredAnswers,
                    TotalAnswerTimeMs = totalTimeMs,
                    IsNewHighScore = highScoreResult.IsNewHighScore,
                    Rewards = rewards.Reward
                };

                game.Status = SoloGameStatus.Completed;
                _gameCache.Remove(gameId);
                return (response, true);
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
            if (_gameCache.TryGet(gameId, out var game) == false || game is null)
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


        private static int[] GetOrientationCategories(CachedPlayer player, int orientationId)
        {
            // PLACEHOLDER: ide kerül a végleges két kategória kinyerése.
            var character = player.Characters.ElementAtOrDefault(orientationId - 1);
            if (character is null)
                return [];

            var first = character.Attitude.Main.CatNo[0];
            var second = character.Attitude.Main.CatNo[2];

            return first > 0 && second > 0 ? [first, second] : [];
        }
        private static int GetQuestionCount(int level)
        {
            if (level <= 0) return 8;
            if (level >= 19) return 20;
            return 10 + ((level - 1) / 4) * 2;
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

                var selectedIndexes = new HashSet<int>(categoryQuestionCount);

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

        private static CachedSoloQuestion? CreateCachedQuestion(FactoryQuestion question)
        {
            var answers = JsonSerializer.Deserialize<string[]>(question.AnswersJson);
            if (answers is null || answers.Length != 4)
                return null;

            var correctAnswer = answers[0];
            Shuffle(answers);

            return new CachedSoloQuestion
            {
                QuestionToken = Guid.NewGuid(),
                QuestionId = question.Id,
                Question = question.Question,
                Answers = answers,
                CorrectOptionIndex = Array.IndexOf(answers, correctAnswer)
            };
        }

        private static bool ValidateFinish(SoloGameSession game, FinishSoloGameRequest request)
        {
            if (request.Answers.Length != game.Questions.Count)
                return false;

            if (request.Answers.Select(answer => answer.QuestionToken).Distinct().Count() != game.Questions.Count)
                return false;

            var validTokens = game.Questions.Select(question => question.QuestionToken).ToHashSet();
            if (request.Answers.Any(answer =>
                validTokens.Contains(answer.QuestionToken) == false ||
                answer.SelectedOptionIndex < -1 || answer.SelectedOptionIndex > 3 ||
                answer.AnswerTimeMs < 0 || answer.AnswerTimeMs > ANSWER_SECONDS * 1000))
                return false;

            var answerTimeMs = request.Answers.Sum(answer => answer.AnswerTimeMs);
            var maximumElapsedMs = game.Questions.Count * (ANSWER_SECONDS + FEEDBACK_SECONDS) * 1000;

            return request.ClientElapsedMs >= answerTimeMs &&
                   request.ClientElapsedMs <= maximumElapsedMs &&
                   request.ClientElapsedMs <= answerTimeMs +
                       game.Questions.Count * FEEDBACK_SECONDS * 1000 + 1000;
        }

        private async Task<(bool? Success, bool IsNewHighScore, int OldScore)> SaveResultAsync(
            SoloGameSession game,
            int newScore,
            int totalTimeMs,
            CancellationToken ct)
        {
            var oldScore = 0;
            var isHighScore = false;
            var totalSeconds = totalTimeMs / 1000d;

            if (game.Mode == SoloGameMode.Category)
            {
                var success = await _playerCache.UpdatePlayerLockedAsync(
                    game.PlayerId,
                    game.SessionId,
                    player =>
                    {
                        var stat = player.CategoryStats
                            .FirstOrDefault(item => item.CategoryId == game.SelectionId);

                        if (stat is null)
                        {
                            stat = new PlayerCategoryStat
                            {
                                PlayerId = game.PlayerId,
                                CategoryId = (short)game.SelectionId
                            };
                            player.CategoryStats.Add(stat);
                        }

                        oldScore = stat.HighScore;
                        isHighScore = IsBetter(
                            newScore,
                            totalSeconds,
                            stat.HighScore,
                            stat.HighScoreTime);

                        if (isHighScore)
                        {
                            stat.HighScore = newScore;
                            stat.HighScoreTime = totalSeconds;
                        }

                        return DirtyFlags.CategoryStats;
                    },
                    ct);

                return (success, isHighScore, oldScore);
            }

            var orientSuccess = await _playerCache.UpdatePlayerLockedAsync(
                game.PlayerId,
                game.SessionId,
                player =>
                {
                    var stat = player.OrientStats
                        .FirstOrDefault(item => item.OrientId == game.SelectionId);

                    if (stat is null)
                    {
                        stat = new PlayerOrientStat
                        {
                            PlayerId = game.PlayerId,
                            OrientId = (short)game.SelectionId
                        };
                        player.OrientStats.Add(stat);
                    }

                    oldScore = stat.HighScore;
                    isHighScore = IsBetter(
                        newScore,
                        totalSeconds,
                        stat.HighScore,
                        stat.HighScoreTime);

                    if (isHighScore)
                    {
                        stat.HighScore = newScore;
                        stat.HighScoreTime = totalSeconds;
                    }

                    return DirtyFlags.OrientStats;
                },
                ct);

            return (orientSuccess, isHighScore, oldScore);
        }
        private static int CalculateAnswerPoints(
                                    int magicNumberMax,
                                    int elapsedMs,
                                    bool? isCorrect)
        {
            if (isCorrect == null)
                return 0;

            var decreasingTimeMs = Math.Clamp(
                elapsedMs - 5000,
                0,
                15000);

            var multiplier =
                1.0 - decreasingTimeMs / 15000.0;

            var points = (int)Math.Round(
                magicNumberMax * multiplier,
                MidpointRounding.AwayFromZero);

            return isCorrect.Value
                ? points
                : -points;
        }

        private async Task<(bool? Success, SoloRewardDto? Reward)> CreateRewardsAsync(SoloGameSession game, int pointsNew, int pointsOld, CancellationToken ct)
        {
            int dev = Math.Max(ScoreConstants.ScorLimits.Count(value => pointsNew >= value) -
                            ScoreConstants.ScorLimits.Count(value => pointsOld >= value),
                            0);

            int xpTeam = 0;
            int xpMember = 0;
            int devTeam = 0;
            int devMember = 0;

            var success = await _playerCache.UpdatePlayerLockedAsync(
                game.PlayerId,
                game.SessionId,
                player =>
                {
                    if (game.Mode == SoloGameMode.Category)
                    {
                        devTeam = dev;
                        player.Core.DevPoint += devTeam;
                        return DirtyFlags.Core;
                    }

                    var member = player.Characters[game.SelectionId - 1];
                    if (member is null)
                        return null;

                    devMember = dev + (game.isHealing ? 1 : 0);

                    if (game.Level == 0 && pointsNew > 0)
                    {
                        xpMember = pointsNew / 10;
                        xpTeam = xpMember / 2;
                        player.Core.XP += xpTeam;
                    }

                    member.DevPoints += devMember;
                    member.XP += xpMember;

                    return xpTeam > 0
                        ? DirtyFlags.Core | DirtyFlags.Characters
                        : DirtyFlags.Characters;
                },
                ct);

            if (success != true)
                return (success, new SoloRewardDto());

            return (true, new SoloRewardDto
            {
                TeamXp = xpTeam,
                TeamDevPoints = devTeam,
                MemberXp = xpMember,
                MemberDevPoints = devMember,
            });
        }

        private static bool IsBetter(int score, double time, int oldScore, double oldTime)
            => score > oldScore || score == oldScore && (oldTime <= 0 || time < oldTime);

        private static void Shuffle<T>(IList<T> values)
        {
            for (var i = values.Count - 1; i > 0; i--)
            {
                var j = Random.Shared.Next(i + 1);
                (values[i], values[j]) = (values[j], values[i]);
            }
        }
    }
}
