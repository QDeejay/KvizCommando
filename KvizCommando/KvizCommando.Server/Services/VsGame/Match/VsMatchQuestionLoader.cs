using KvizCommando.Server.Domain.Entities.Questions;
using KvizCommando.Server.Services.SoloGame;
using KvizCommando.Server.Services.SoloGame.CategoryQuestionIndex;
using KvizCommando.Shared.Models.Enums.VsGame;
using System.Text.Json;

namespace KvizCommando.Server.Services.VsGame.Match;

public sealed class VsMatchQuestionLoader : IVsMatchQuestionLoader
{
    private const int GuessCategoryId = 99;

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ICategoryQuestionIndexCache _questionIndex;

    public VsMatchQuestionLoader(
        IServiceScopeFactory scopeFactory,
        ICategoryQuestionIndexCache questionIndex)
    {
        _scopeFactory = scopeFactory;
        _questionIndex = questionIndex;
    }

    public async Task<VsMatchQuestionSet> LoadAsync(
            IReadOnlyCollection<VsMatchPlayerSeed> players,
            int loadoutSize,
            int normalRoundCount,
            CancellationToken ct = default)
    {
        var usedFactoryIds = new HashSet<int>();
        var plans = players.ToDictionary(
            player => player.PlayerId,
            player => BuildPlan(
                player,
                loadoutSize,
                usedFactoryIds));

        var factoryIds = plans.Values
            .SelectMany(plan => plan)
            .Where(item => item.FactoryQuestionId.HasValue)
            .Select(item => item.FactoryQuestionId!.Value)
            .ToArray();

        using var scope = _scopeFactory.CreateScope();
        var repository =
            scope.ServiceProvider.GetRequiredService<ISoloQuestionRepository>();

        var factoryQuestions = await repository.LoadByIdsAsync(
            factoryIds,
            ct);
        var guessIds = SelectGuessQuestionIds(normalRoundCount);
        var guessQuestions = await repository.LoadGuessByIdsAsync(
            guessIds,
            ct);

        if (factoryQuestions.Count != factoryIds.Length)
        {
            throw new InvalidOperationException(
                "The VS question index and FactoryQuestions table are inconsistent.");
        }

        var factoryMap = factoryQuestions.ToDictionary(
            question => question.Id);

        if (guessQuestions.Count != guessIds.Length)
        {
            throw new InvalidOperationException(
                "The VS guess-question index and GuessQuestions table are inconsistent.");
        }

        var guessMap = guessQuestions.ToDictionary(
            question => question.Id);

        return new VsMatchQuestionSet
        {
            Loadouts = plans.ToDictionary(
                item => item.Key,
                item => item.Value
                    .Select(plan => BuildLoadoutItem(plan, factoryMap))
                    .ToArray()),
            GuessQuestions =
            [
                .. guessIds.Select(id =>
                    new VsMatchGuessQuestionState
                    {
                        QuestionId = id,
                        Question = guessMap[id].Question,
                        CorrectAnswer = guessMap[id].Answer
                    })
            ]
        };
    }

    private int[] SelectGuessQuestionIds(int count)
    {
        var ids = _questionIndex
            .GetQuestionIds(GuessCategoryId)
            .ToArray();

        Shuffle(ids);
        return [.. ids.Take(count)];
    }

    private VsQuestionLoadPlan[] BuildPlan(
        VsMatchPlayerSeed player,
        int loadoutSize,
        ISet<int> usedFactoryIds)
    {
        if (player.LoadoutCategories.Length < loadoutSize)
        {
            throw new InvalidOperationException(
                $"Player {player.PlayerId} has an incomplete VS loadout.");
        }

        var result = new VsQuestionLoadPlan[loadoutSize];
        var ownQuestionIndex = 0;

        for (var index = 0; index < loadoutSize; index++)
        {
            var categoryId = player.LoadoutCategories[index];

            if (categoryId == VsLoadoutCategoryIds.OwnQuestion)
            {
                if (ownQuestionIndex >= player.OwnQuestions.Length)
                {
                    throw new InvalidOperationException(
                        $"Player {player.PlayerId} has an invalid own-question loadout.");
                }

                result[index] = new VsQuestionLoadPlan
                {
                    LoadoutPosition = index,
                    DisplayCategoryId = categoryId,
                    OwnQuestion =
                        player.OwnQuestions[ownQuestionIndex++]
                };
                continue;
            }

            var selectedCategory = categoryId ==
                                   VsLoadoutCategoryIds.AllCategories
                ? Random.Shared.Next(
                    VsLoadoutCategoryIds.MinimumFactoryCategory,
                    VsLoadoutCategoryIds.MaximumFactoryCategory + 1)
                : categoryId;

            if (selectedCategory is <
                    VsLoadoutCategoryIds.MinimumFactoryCategory or >
                    VsLoadoutCategoryIds.MaximumFactoryCategory)
            {
                throw new InvalidOperationException(
                    $"Player {player.PlayerId} has an invalid VS loadout category.");
            }

            result[index] = new VsQuestionLoadPlan
            {
                LoadoutPosition = index,
                DisplayCategoryId = categoryId,
                FactoryQuestionId = SelectFactoryQuestionId(
                    selectedCategory,
                    usedFactoryIds)
            };
        }

        return result;
    }

    private int SelectFactoryQuestionId(
        int categoryId,
        ISet<int> usedFactoryIds)
    {
        var ids = _questionIndex.GetQuestionIds(categoryId);

        if (ids.Count == 0)
        {
            throw new InvalidOperationException(
                $"Category {categoryId} has no indexed question.");
        }

        var startIndex = Random.Shared.Next(ids.Count);

        for (var offset = 0; offset < ids.Count; offset++)
        {
            var questionId = ids[(startIndex + offset) % ids.Count];

            if (usedFactoryIds.Add(questionId))
                return questionId;
        }

        throw new InvalidOperationException(
            $"Category {categoryId} has no unused indexed question.");
    }

    private static VsMatchLoadoutItemState BuildLoadoutItem(
        VsQuestionLoadPlan plan,
        IReadOnlyDictionary<int, FactoryQuestion> factoryMap)
    {
        if (plan.OwnQuestion is not null)
        {
            return CreateState(
                plan.LoadoutPosition,
                plan.DisplayCategoryId,
                plan.OwnQuestion.CategoryId,
                plan.OwnQuestion.QuestionId,
                true,
                false,
                plan.OwnQuestion.Question,
                plan.OwnQuestion.AnswersJson);
        }

        var question = factoryMap[plan.FactoryQuestionId!.Value];

        return CreateState(
            plan.LoadoutPosition,
            plan.DisplayCategoryId,
            question.CategoryNo,
            question.Id,
            false,
            plan.DisplayCategoryId ==
                VsLoadoutCategoryIds.AllCategories,
            question.Question,
            question.AnswersJson);
    }

    private static VsMatchLoadoutItemState CreateState(
        int loadoutPosition,
        int categoryId,
        int questionCategoryId,
        int questionId,
        bool isOwnQuestion,
        bool isAllCategories,
        string question,
        string answersJson)
    {
        var answers = JsonSerializer.Deserialize<string[]>(answersJson);

        if (answers is null || answers.Length != 4)
        {
            throw new InvalidOperationException(
                $"Question {questionId} does not contain four answers.");
        }

        var correctAnswer = answers[0];
        Shuffle(answers);

        return new VsMatchLoadoutItemState
        {
            LoadoutPosition = loadoutPosition,
            CategoryId = categoryId,
            QuestionCategoryId = questionCategoryId,
            QuestionId = questionId,
            IsOwnQuestion = isOwnQuestion,
            IsAllCategories = isAllCategories,
            Question = question,
            Answers = answers,
            CorrectOptionIndex = Array.IndexOf(
                answers,
                correctAnswer)
        };
    }

    private static void Shuffle<T>(IList<T> values)
    {
        for (var i = values.Count - 1; i > 0; i--)
        {
            var other = Random.Shared.Next(i + 1);
            (values[i], values[other]) =
                (values[other], values[i]);
        }
    }

    private sealed class VsQuestionLoadPlan
    {
        public int LoadoutPosition { get; init; }
        public int DisplayCategoryId { get; init; }
        public int? FactoryQuestionId { get; init; }
        public VsOwnQuestionSeed? OwnQuestion { get; init; }
    }
}

/**
 * MÓDOSÍTÁS: a betöltött loadoutelemeket a már meglévő, játékoson
 * belül egyedi LoadoutPosition azonosítja; külön GUID nem készül.
 *
 * MÓDOSÍTÁS: a 99-es indexből a normál körök számának megfelelő,
 * egyedi tippkérdést is kiválaszt, és ugyanabban az inicializálási
 * lépésben kötegelve betölti őket.
 * Az „összes kategória” loadoutelemén megőrzi a 0-s megjelenítési
 * értéket, a játékhoz viszont a kisorsolt kérdés valódi kategóriáját
 * adja tovább.
 *
 * A kategóriánkénti ID-indexből kiválasztja a meccs kérdéseit,
 * majd kötegelt adatbázis-lekéréssel meccsszintű, kevert állapotot
 * készít.
 */
