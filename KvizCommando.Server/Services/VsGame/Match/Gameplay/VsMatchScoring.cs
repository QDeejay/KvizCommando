using KvizCommando.Shared.Models.Enums.VsGame;

namespace KvizCommando.Server.Services.VsGame.Match;

internal static class VsMatchScoring
{
    internal static void CloseGuess(VsMatchSession match)
    {
        var question = match.Game.CurrentQuestion!;
        var ordered = match.Players
            .OrderBy(player => player.CurrentAnswer?.Guess.HasValue != true)
            .ThenBy(player => player.CurrentAnswer?.Guess is double guess
                ? Math.Abs(guess - question.CorrectGuess)
                : double.MaxValue)
            .ThenBy(player =>
                player.CurrentAnswer?.AnswerTimeSeconds ??
                double.MaxValue)
            .ThenBy(player => player.Position)
            .ToArray();

        match.Game.QuestionerOrder =
        [
            .. ordered.Select(player => player.Position)
        ];

        var winner = ordered.FirstOrDefault(player =>
            player.CurrentAnswer?.Guess.HasValue == true);
        var results = new List<VsMatchQuestionPlayerResultState>(
            match.Players.Count);

        foreach (var player in match.Players)
        {
            var answer = player.CurrentAnswer;
            var points = player == winner
                ? match.Profile.PointUnit
                : 0;
            var answerTime =
                answer?.AnswerTimeSeconds ??
                match.Profile.GuessSeconds;

            if (!player.IsBot && question.CorrectGuess != 0)
            {
                var errorRatio = answer?.Guess is double guess
                    ? Math.Abs(guess - question.CorrectGuess) /
                      Math.Abs(question.CorrectGuess)
                    : 1.0;

                player.Statistics.GuessCount++;
                player.Statistics.GuessErrorTotal += errorRatio;
            }

            player.RoundPoints += points;
            player.RoundTimeSeconds += answerTime;
            player.RoundProgress.Add(points);

            results.Add(new VsMatchQuestionPlayerResultState
            {
                Position = player.Position,
                Guess = answer?.Guess,
                AnswerTimeSeconds = answerTime,
                Points = points
            });
        }

        match.Game.QuestionResult = new VsMatchQuestionResultState
        {
            Kind = question.Kind,
            CorrectGuess = question.CorrectGuess,
            Players = [.. results]
        };
    }

    internal static void CloseChoice(VsMatchSession match)
    {
        var question = match.Game.CurrentQuestion!;
        var isCaptain =
            match.Game.CurrentRoundNumber >
            match.Classification.RequiredPartySize;
        var unit = match.Profile.PointUnit *
                   (isCaptain
                       ? match.Profile.CaptainMultiplier
                       : 1);
        var questioner = match.Players.First(player =>
            player.Position == question.QuestionerPosition);
        var correctResponders = GetCorrectResponders(
            match,
            question,
            questioner);
        var speedWinner = ResolveSpeedWinner(correctResponders);

        if (correctResponders.Length > 1 &&
            speedWinner is null)
        {
            match.Game.SpeedBonusPoolPoints += unit;
        }

        var otherCorrect = correctResponders.Length;
        var otherNoAnswer = match.Players.Count(player =>
            player != questioner &&
            player.CurrentAnswer?.AnswerIndex.HasValue != true);
        var questionerScore =
            unit * (match.Players.Count - 1) -
            unit * otherCorrect +
            unit * otherNoAnswer;

        UpdateQuestionerStatistics(
            match,
            question,
            questioner,
            isCaptain,
            otherCorrect);

        var results = new List<VsMatchQuestionPlayerResultState>(
            match.Players.Count);

        foreach (var player in match.Players)
        {
            var result = CreateChoicePlayerResult(
                match,
                question,
                player,
                questioner,
                speedWinner,
                questionerScore,
                unit);

            ApplyChoicePlayerResult(
                match,
                question,
                player,
                questioner,
                result,
                isCaptain);
            results.Add(result);
        }

        match.Game.QuestionResult = new VsMatchQuestionResultState
        {
            Kind = question.Kind,
            CorrectOptionIndex = question.CorrectOptionIndex,
            Players = [.. results]
        };
    }

    private static SpeedCandidate[] GetCorrectResponders(
        VsMatchSession match,
        VsMatchQuestionState question,
        VsMatchPlayerState questioner) =>
        match.Players
            .Where(player =>
                player != questioner &&
                player.CurrentAnswer?.AnswerIndex ==
                question.CorrectOptionIndex)
            .Select(player => new SpeedCandidate(
                player,
                CalculateModifiedTime(
                    match,
                    player,
                    question)))
            .ToArray();

    private static void UpdateQuestionerStatistics(
        VsMatchSession match,
        VsMatchQuestionState question,
        VsMatchPlayerState questioner,
        bool isCaptain,
        int otherCorrect)
    {
        if (isCaptain)
        {
            questioner.Statistics.QuestionsAsked +=
                match.Players.Count - 1;
            questioner.Statistics.CorrectAnswersToAskedQuestions +=
                otherCorrect;
        }

        if (!question.IsOwnQuestion)
            return;

        if (!questioner.Statistics.OwnQuestions.TryGetValue(
                question.QuestionId,
                out var ownQuestion))
        {
            ownQuestion = new VsMatchOwnQuestionStatisticsState();
            questioner.Statistics.OwnQuestions.Add(
                question.QuestionId,
                ownQuestion);
        }

        ownQuestion.Asked += match.Players.Count - 1;
        ownQuestion.CorrectAnswers += otherCorrect;
    }

    private static VsMatchQuestionPlayerResultState
        CreateChoicePlayerResult(
            VsMatchSession match,
            VsMatchQuestionState question,
            VsMatchPlayerState player,
            VsMatchPlayerState questioner,
            VsMatchPlayerState? speedWinner,
            int questionerScore,
            int unit)
    {
        var answer = player.CurrentAnswer;
        var hasAnswer = answer?.AnswerIndex.HasValue == true;
        var isCorrect =
            answer?.AnswerIndex == question.CorrectOptionIndex;
        var answerTime = isCorrect
            ? answer!.AnswerTimeSeconds
            : match.Profile.QuestionSeconds +
              (hasAnswer &&
               player.ActiveQuestionHelp == VsHelpType.TimeFreeze
                  ? match.Profile.TimeFreezeWrongAnswerPenaltySeconds
                  : 0);
        var points = player == questioner
            ? ResolveQuestionerPoints(
                hasAnswer,
                isCorrect,
                questionerScore,
                unit * (match.Players.Count - 1))
            : ResolveResponderPoints(
                hasAnswer,
                isCorrect,
                unit);
        var hasSpeedBonus = player == speedWinner;

        if (hasSpeedBonus)
            points += unit;

        return new VsMatchQuestionPlayerResultState
        {
            Position = player.Position,
            AnswerIndex = answer?.AnswerIndex,
            IsCorrect = isCorrect,
            AnswerTimeSeconds = answerTime,
            ModifiedTimeSeconds =
                isCorrect && player != questioner
                    ? CalculateModifiedTime(
                        match,
                        player,
                        question)
                    : null,
            Points = points,
            HasSpeedBonus = hasSpeedBonus
        };
    }

    private static void ApplyChoicePlayerResult(
        VsMatchSession match,
        VsMatchQuestionState question,
        VsMatchPlayerState player,
        VsMatchPlayerState questioner,
        VsMatchQuestionPlayerResultState result,
        bool isCaptain)
    {
        UpdateCategoryStatistics(player, question, result.IsCorrect);

        if (result.IsCorrect)
            player.Statistics.CorrectAnswers++;

        var losesVitality =
            !isCaptain &&
            player != questioner &&
            result.AnswerIndex.HasValue &&
            !result.IsCorrect;

        if (losesVitality)
            VsMatchGameRules.DeductVitality(match, player);

        player.RoundPoints += result.Points;

        if (player != questioner)
            player.RoundTimeSeconds += result.AnswerTimeSeconds;

        player.RoundProgress.Add(result.Points);
    }

    private static void UpdateCategoryStatistics(
        VsMatchPlayerState player,
        VsMatchQuestionState question,
        bool isCorrect)
    {
        if (question.CategoryId is < 1 or > 16)
            return;

        if (!player.Statistics.Categories.TryGetValue(
                question.CategoryId,
                out var category))
        {
            category = new VsMatchCategoryStatisticsState();
            player.Statistics.Categories.Add(
                question.CategoryId,
                category);
        }

        category.Answered++;

        if (isCorrect)
            category.Correct++;
    }

    internal static void BuildRoundResult(
        VsMatchSession match,
        bool isCaptainRound)
    {
        var placement = match.Players
            .OrderByDescending(player => player.RoundPoints)
            .ThenBy(player => RoundToTenth(player.RoundTimeSeconds))
            .ThenBy(player => player.Position)
            .ToArray();

        VsMatchPlayerState? winner = null;
        var questionerOrder = isCaptainRound
            ? match.Game.CaptainOrder
            : match.Game.QuestionerOrder;
        var fastest = match.Players
            .OrderBy(player =>
                RoundToTenth(player.RoundTimeSeconds))
            .ThenBy(player =>
                Array.IndexOf(
                    questionerOrder,
                    player.Position))
            .First();

        if (!isCaptainRound)
        {
            winner = placement[0];
            winner.RoundPoints += match.Profile.PointUnit;
        }

        fastest.RoundPoints +=
            match.Game.SpeedBonusPoolPoints;
        var finalPlacement = match.Players
            .OrderByDescending(player => player.RoundPoints)
            .ThenBy(player =>
                RoundToTenth(player.RoundTimeSeconds))
            .ThenBy(player => player.Position)
            .ToArray();
        var fastestPositionGain =
            Array.IndexOf(finalPlacement, fastest) <
            Array.IndexOf(placement, fastest);

        var weights = ResolvePlacementWeights(match.Players.Count);
        var result = new List<VsMatchRoundResultState>(
            match.Players.Count);

        for (var placementIndex = 0;
             placementIndex < placement.Length;
             placementIndex++)
        {
            var player = placement[placementIndex];
            var character = isCaptainRound
                ? null
                : ResolveRoundCharacter(match, player);
            var characterXp = character is null
                ? 0
                : CalculateCharacterXp(
                    match,
                    player,
                    character,
                    weights[placementIndex]);
            var energyLoss = character is null
                ? 0
                : player.CharacterRewardTotals
                    .First(item =>
                        item.SlotNumber == character.SlotNumber)
                    .EnergyLoss;

            result.Add(new VsMatchRoundResultState
            {
                Position = player.Position,
                TotalBefore = player.TotalPoints,
                RoundPoints = player.RoundPoints,
                TotalAfter =
                    player.TotalPoints + player.RoundPoints,
                RoundTimeSeconds = player.RoundTimeSeconds,
                HasWinnerBonus = player == winner,
                FastestBonusPoints = player == fastest
                    ? match.Game.SpeedBonusPoolPoints
                    : 0,
                HasSpeedPositionGain =
                    player == fastest &&
                    fastestPositionGain,
                CharacterSlotNumber =
                    character?.SlotNumber ?? 0,
                CharacterXp = characterXp,
                EnergyLoss = energyLoss
            });
        }

        match.Game.RoundResult = [.. result];
    }

    internal static void CommitRoundResult(VsMatchSession match)
    {
        foreach (var player in match.Players)
        {
            var roundReward = match.Game.RoundResult.First(result =>
                result.Position == player.Position);

            if (roundReward.CharacterSlotNumber > 0)
            {
                var characterTotal =
                    player.CharacterRewardTotals.First(item =>
                        item.SlotNumber ==
                        roundReward.CharacterSlotNumber);

                characterTotal.CharacterXp += roundReward.CharacterXp;
                characterTotal.PlayDuels++;

                if (roundReward.HasWinnerBonus)
                    characterTotal.WinDuels++;
            }

            player.TotalPoints += player.RoundPoints;
            player.TotalTimeSeconds += player.RoundTimeSeconds;
        }
    }

    internal static IOrderedEnumerable<VsMatchPlayerState>
        OrderByStanding(IEnumerable<VsMatchPlayerState> players) =>
        players
            .OrderByDescending(player => player.TotalPoints)
            .ThenBy(player =>
                RoundToTenth(player.TotalTimeSeconds))
            .ThenBy(player => player.Position);

    private static int ResolveQuestionerPoints(
        bool hasAnswer,
        bool isCorrect,
        int possiblePoints,
        int noAnswerPenalty)
    {
        if (!hasAnswer)
            return -noAnswerPenalty;

        return isCorrect
            ? possiblePoints
            : -possiblePoints;
    }

    private static int ResolveResponderPoints(
        bool hasAnswer,
        bool isCorrect,
        int unit)
    {
        if (!hasAnswer)
            return 0;

        return isCorrect ? unit : -unit;
    }

    private static VsMatchPlayerState? ResolveSpeedWinner(
        IReadOnlyCollection<SpeedCandidate> correctResponders)
    {
        var entries = correctResponders
            .OrderBy(item =>
                RoundToTenth(item.ModifiedTime))
            .ToArray();

        if (entries.Length == 0)
            return null;

        return entries.Length > 1 &&
               RoundToTenth(entries[0].ModifiedTime) ==
               RoundToTenth(entries[1].ModifiedTime)
            ? null
            : entries[0].Player;
    }

    private static double CalculateModifiedTime(
        VsMatchSession match,
        VsMatchPlayerState player,
        VsMatchQuestionState question)
    {
        var rawTime =
            player.CurrentAnswer?.AnswerTimeSeconds ??
            match.Profile.QuestionSeconds;

        return Math.Max(
            0,
            rawTime + CalculateTimeModifier(
                match,
                player,
                question));
    }

    internal static double CalculateTimeModifier(
        VsMatchSession match,
        VsMatchPlayerState player,
        VsMatchQuestionState question)
    {
        if (player.ActiveQuestionHelp ==
            VsHelpType.TimeFreeze)
        {
            return match.Profile
                .TimeFreezeModifierSeconds;
        }

        if (match.Game.CurrentRoundNumber >
                match.Classification.RequiredPartySize ||
            question.CategoryId is
                <= 0 or >
                16)
        {
            return 0;
        }

        var modifier = 0d;

        foreach (var otherPlayer in match.Players.Where(item =>
                     item.PlayerId != player.PlayerId))
        {
            var character =
                ResolveRoundCharacter(match, otherPlayer);

            if (character?.CategoryModifiers.TryGetValue(
                    question.CategoryId,
                    out var value) == true)
            {
                modifier += value;
            }
        }

        return modifier;
    }

    private static VsMatchCharacterState? ResolveRoundCharacter(
        VsMatchSession match,
        VsMatchPlayerState player)
    {
        var round = player.Rounds.First(item =>
            item.RoundNumber ==
            match.Game.CurrentRoundNumber);

        return round.CharacterSlotNumber.HasValue
            ? player.Characters.First(item =>
                item.SlotNumber ==
                round.CharacterSlotNumber.Value)
            : null;
    }

    private static int CalculateCharacterXp(
        VsMatchSession match,
        VsMatchPlayerState player,
        VsMatchCharacterState character,
        int weight)
    {
        var level = Math.Max(character.Level, 1);
        var otherAverage = AverageOtherCharacterLevel(
            match,
            player);
        var xp = Math.Floor(
            weight * level +
            weight * otherAverage / level);

        return Math.Max(weight, (int)xp);
    }

    private static double AverageOtherCharacterLevel(
        VsMatchSession match,
        VsMatchPlayerState player) =>
        match.Players
            .Where(item => item.PlayerId != player.PlayerId)
            .Select(item =>
                Math.Max(
                    ResolveRoundCharacter(match, item)?.Level ?? 0,
                    1))
            .Average();

    private static int[] ResolvePlacementWeights(
        int playerCount) =>
        playerCount switch
        {
            2 => [3, 1],
            3 => [5, 3, 1],
            _ => [10, 5, 3, 1]
        };

    private static double RoundToTenth(double value) =>
        Math.Round(
            value,
            1,
            MidpointRounding.AwayFromZero);

    private sealed record SpeedCandidate(
        VsMatchPlayerState Player,
        double ModifiedTime);
}
