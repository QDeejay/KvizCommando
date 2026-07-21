using KvizCommando.Server.Domain.Entities.Questions;
using KvizCommando.Server.Domain.Entities.Statistics;
using KvizCommando.Server.Services.PlayerCache;
using KvizCommando.Server.Services.SoloGame.CategoryQuestionIndex;
using KvizCommando.Server.Services.SoloGame.GameCache;
using KvizCommando.Shared.Contracts.SoloGame;
using System.Text.Json;

namespace KvizCommando.Server.Services.SoloGame
{
    public sealed class SoloGameService : ISoloGameService
    {
        private const int AnswerSeconds = 20;
        private const int FeedbackSeconds = 2;
        private const int GraceSeconds = 60;

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

            if (_gameCache.HasActiveGame(playerId, request.SessionId))
                return (null, false);

            var level = request.Mode == SoloGameMode.Category
                ? player.Core.RankEnum
                : player.Characters[request.SelectionId - 1].Rank;

            if (level < 0)
                return (null, false);

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
            var gameTime = TimeSpan.FromSeconds(questionCount * (AnswerSeconds + FeedbackSeconds));
            var maxPointPerQuestion = 100 + level / 2 * 10;
            var game = new SoloGameSession
            {
                GameId = Guid.NewGuid(),
                PlayerId = playerId,
                SessionId = request.SessionId,
                Mode = request.Mode,
                SelectionId = request.SelectionId,
                Level = level,
                StartedAtUtc = now,
                PointsPerLevel = maxPointPerQuestion,
                GameplayDeadlineUtc = now.Add(gameTime),
                ExpiresAtUtc = now.Add(gameTime).AddSeconds(GraceSeconds),
                Questions = cachedQuestions
            };

            if (_gameCache.TryCreate(game) == false)
                return (null, false);

            var response = new StartSoloGameResponse
            {
                GameId = game.GameId,
                QuestionCount = questionCount,
                AnswerTimeSeconds = AnswerSeconds,
                FeedbackTimeSeconds = FeedbackSeconds,
                Questions = cachedQuestions.Select(question => new SoloQuestionDto
                {
                    QuestionToken = question.QuestionToken,
                    Question = question.Question,
                    Answers = question.Answers
                }).ToArray()
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

                var results = game.Questions.Select(question =>
                    submittedAnswers[question.QuestionToken].SelectedOptionIndex == question.CorrectOptionIndex).ToArray();

                var correctAnswers = results.Count(result => result);
                var totalTimeMs = request.Answers.Sum(answer => answer.AnswerTimeMs);

                var highScoreResult = await SaveResultAsync(game, correctAnswers, totalTimeMs, ct);

                if (highScoreResult.Success != true)

                    return (null, highScoreResult.Success);

                var response = new FinishSoloGameResponse
                {
                    AnswerResults = results,
                    CorrectAnswers = correctAnswers,
                    WrongAnswers = results.Length - correctAnswers,
                    TotalAnswerTimeMs = totalTimeMs,
                    IsNewHighScore = highScoreResult.IsNewHighScore,
                    Rewards = CreateRewards(game, correctAnswers, totalTimeMs)
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
            var second = character.Attitude.Main.CatNo[1];

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
                var ids = _questionIndex.GetQuestionIds(categoryId).ToArray();
                if (ids.Length < categoryQuestionCount)
                    return [];

                Shuffle(ids);
                result.AddRange(ids.Take(categoryQuestionCount));
            }

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
                answer.AnswerTimeMs < 0 || answer.AnswerTimeMs > AnswerSeconds * 1000))
                return false;

            var answerTimeMs = request.Answers.Sum(answer => answer.AnswerTimeMs);
            var maximumElapsedMs = game.Questions.Count * (AnswerSeconds + FeedbackSeconds) * 1000;

            return request.ClientElapsedMs >= answerTimeMs &&
                   request.ClientElapsedMs <= maximumElapsedMs &&
                   request.ClientElapsedMs <= answerTimeMs +
                       game.Questions.Count * FeedbackSeconds * 1000 + 1000;
        }

        private async Task<(bool? Success, bool IsNewHighScore)> SaveResultAsync(
            SoloGameSession game,
            int correctAnswers,
            int totalTimeMs,
            CancellationToken ct)
        {
            var (player, _) = await _playerCache.GetOrLoadLockedAsync(game.PlayerId, game.SessionId, ct);

            if (player is null) return (false, false);
            if (player.SessionId == "denied") return (null, false);

            var totalSeconds = totalTimeMs / 1000d;

            if (game.Mode == SoloGameMode.Category)
            {
                var stats = player.CategoryStats.Select(CloneCategoryStat).ToList();
                var stat = stats.FirstOrDefault(item => item.CategoryId == game.SelectionId);
                if (stat is null)
                {
                    stat = new PlayerCategoryStat { PlayerId = game.PlayerId, CategoryId = (short)game.SelectionId };
                    stats.Add(stat);
                }

                stat.Answered += game.Questions.Count;
                stat.Correct += correctAnswers;
                var isHighScore = IsBetter(correctAnswers, totalSeconds, stat.HighScore, stat.HighScoreTime);
                if (isHighScore) { stat.HighScore = correctAnswers; stat.HighScoreTime = totalSeconds; }

                var success = await _playerCache.UpdatePartialCategoryStatsLockedAsync(
                    game.PlayerId, game.SessionId, stats, ct);
                return (success, isHighScore);
            }

            var orientStats = player.OrientStats.Select(CloneOrientStat).ToList();
            var orientStat = orientStats.FirstOrDefault(item => item.OrientId == game.SelectionId);
            if (orientStat is null)
            {
                orientStat = new PlayerOrientStat { PlayerId = game.PlayerId, OrientId = (short)game.SelectionId };
                orientStats.Add(orientStat);
            }

            var orientHighScore = IsBetter(correctAnswers, totalSeconds, orientStat.HighScore, orientStat.HighScoreTime);
            if (orientHighScore) { orientStat.HighScore = correctAnswers; orientStat.HighScoreTime = totalSeconds; }

            var orientSuccess = await _playerCache.UpdatePartialOrientStatsLockedAsync(
                game.PlayerId, game.SessionId, orientStats, ct);
            return (orientSuccess, orientHighScore);
        }

        private static SoloRewardDto CreateRewards(SoloGameSession game, int correctAnswers, int totalTimeMs)
        {
            // PLACEHOLDER: itt készül majd a jutalom és itt kerül a PlayerCache-be.
            return new SoloRewardDto();
        }

        private static bool IsBetter(int score, double time, int oldScore, double oldTime)
            => score > oldScore || score == oldScore && (oldTime <= 0 || time < oldTime);

        private static PlayerCategoryStat CloneCategoryStat(PlayerCategoryStat stat) => new()
        {
            PlayerId = stat.PlayerId,
            CategoryId = stat.CategoryId,
            Answered = stat.Answered,
            Correct = stat.Correct,
            HighScore = stat.HighScore,
            HighScoreTime = stat.HighScoreTime
        };

        private static PlayerOrientStat CloneOrientStat(PlayerOrientStat stat) => new()
        {
            PlayerId = stat.PlayerId,
            OrientId = stat.OrientId,
            HighScore = stat.HighScore,
            HighScoreTime = stat.HighScoreTime
        };

        private static void Shuffle<T>(T[] values)
        {
            for (var i = values.Length - 1; i > 0; i--)
            {
                var j = Random.Shared.Next(i + 1);
                (values[i], values[j]) = (values[j], values[i]);
            }
        }
    }
}
