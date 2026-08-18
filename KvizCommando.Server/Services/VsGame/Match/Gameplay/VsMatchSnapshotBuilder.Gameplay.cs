using KvizCommando.Shared.Contracts.VsGame.Match;
using KvizCommando.Shared.Models.Enums.VsGame;

namespace KvizCommando.Server.Services.VsGame.Match;

internal static partial class VsMatchSnapshotBuilder
{
    private static VsGameDto BuildGame(
        VsMatchSession match,
        VsMatchPlayerState currentPlayer)
    {
        var game = match.Game;
        var question = game.CurrentQuestion;
        var showResult =
            match.Phase == VsMatchPhase.QuestionResult &&
            game.QuestionResult is not null;
        var currentHelpType = ResolveCurrentHelpType(
            match,
            currentPlayer);

        return new VsGameDto
        {
            CurrentRoundNumber = game.CurrentRoundNumber,
            NormalRoundCount =
                match.Classification.RequiredPartySize,
            QuestionNumber = game.QuestionNumber,
            SpeedBonusPoolPoints =
                game.SpeedBonusPoolPoints,
            QuestionKind = game.QuestionKind,
            QuestionerPosition =
                question?.QuestionerPosition ?? 0,
            Question = question?.Question ?? string.Empty,
            Answers = question?.Answers ?? [],
            CorrectAnswerIndex = showResult &&
                                 question?.Kind ==
                                 VsQuestionKind.Choice
                ? question!.CorrectOptionIndex
                : null,
            CorrectGuess = showResult &&
                           question?.Kind ==
                           VsQuestionKind.Guess
                ? question!.CorrectGuess
                : null,
            MyAnswerIndex =
                currentPlayer.CurrentAnswer?.AnswerIndex,
            MyGuess = currentPlayer.CurrentAnswer?.Guess,
            MyTimeModifierSeconds =
                ResolveCurrentTimeModifier(
                    match,
                    currentPlayer,
                    question),
            MyGuessRangeMinimum =
                currentPlayer.GuessRangeMinimum,
            MyGuessRangeMaximum =
                currentPlayer.GuessRangeMaximum,
            MyHiddenAnswerIndices =
                currentPlayer.HiddenAnswerIndices,
            MySuggestedAnswerIndex =
                currentPlayer.SuggestedAnswerIndex,
            MyHelpType = currentHelpType,
            MyHelpUsesRemaining =
                ResolveHelpUsesRemaining(
                    match,
                    currentPlayer),
            IsMyHelpUnlimited =
                currentHelpType ==
                VsHelpType.TimeFreeze,
            CanUseHelp = VsMatchGameRules.CanUseHelp(
                match,
                currentPlayer,
                DateTime.UtcNow),
            MyRoundPoints = currentPlayer.RoundPoints,
            MyRoundTimeSeconds =
                currentPlayer.RoundTimeSeconds,
            CanAnswer =
                IsAnswerPhase(match.Phase) &&
                currentPlayer.IsConnected &&
                currentPlayer.CurrentAnswer is null &&
                VsMatchGameRules.CanAnswerCurrentQuestion(
                    match,
                    currentPlayer) &&
                (!match.DeadlineUtc.HasValue ||
                 DateTime.UtcNow <= match.DeadlineUtc.Value),
            CanChooseCaptainQuestion =
                CanChooseCaptainQuestion(match, currentPlayer),
            QuestionPlayers = BuildQuestionPlayers(
                match,
                showResult),
            Progress = BuildProgress(match, currentPlayer),
            RoundResult =
            [
                .. game.RoundResult.Select(ToRoundResultDto)
            ],
            CaptainQuestions =
                BuildCaptainQuestions(match, currentPlayer),
            CaptainOrder = game.CaptainOrder,
            CaptainOrderIndex = game.CaptainOrderIndex
        };
    }

    private static double? ResolveCurrentTimeModifier(
        VsMatchSession match,
        VsMatchPlayerState currentPlayer,
        VsMatchQuestionState? question)
    {
        if (match.Phase is not
                (VsMatchPhase.NormalRoundQuestion or
                 VsMatchPhase.QuestionResult) ||
            question is null ||
            question.Kind != VsQuestionKind.Choice ||
            question.QuestionerPosition == currentPlayer.Position ||
            match.Game.CurrentRoundNumber <= 0 ||
            match.Game.CurrentRoundNumber >
                match.Classification.RequiredPartySize)
        {
            return null;
        }

        if (match.Phase == VsMatchPhase.QuestionResult &&
            currentPlayer.ActiveQuestionHelp ==
                VsHelpType.TimeFreeze)
        {
            var result = match.Game.QuestionResult
                ?.Players.FirstOrDefault(item =>
                    item.Position == currentPlayer.Position);

            if (result is
                {
                    AnswerIndex: not null,
                    IsCorrect: false
                })
            {
                return match.Profile
                    .TimeFreezeWrongAnswerPenaltySeconds;
            }
        }

        var seconds = VsMatchScoring.CalculateTimeModifier(
            match,
            currentPlayer,
            question);

        return Math.Truncate(seconds * 10) / 10;
    }

    private static int ResolveHelpUsesRemaining(
        VsMatchSession match,
        VsMatchPlayerState currentPlayer)
    {
        var round = ResolveCurrentRound(
            match,
            currentPlayer);

        return round is
            {
                HelpType:
                    VsHelpType.FiftyFifty or
                    VsHelpType.AiSuggestion,
                HelpUsed: false
            }
                ? 1
                : 0;
    }

    private static VsHelpType ResolveCurrentHelpType(
        VsMatchSession match,
        VsMatchPlayerState currentPlayer) =>
        ResolveCurrentRound(match, currentPlayer)
            ?.HelpType ?? VsHelpType.None;

    private static VsMatchRoundState? ResolveCurrentRound(
        VsMatchSession match,
        VsMatchPlayerState currentPlayer)
    {
        if (match.Game.CurrentRoundNumber <= 0)
            return null;

        return currentPlayer.Rounds.FirstOrDefault(round =>
            round.RoundNumber ==
            match.Game.CurrentRoundNumber);
    }

    private static VsQuestionPlayerDto[] BuildQuestionPlayers(
        VsMatchSession match,
        bool showResult)
    {
        if (showResult)
        {
            return
            [
                .. match.Game.QuestionResult!.Players.Select(
                    result => new VsQuestionPlayerDto
                    {
                        Position = result.Position,
                        HasAnswered =
                            result.AnswerIndex.HasValue ||
                            result.Guess.HasValue,
                        AnswerIndex = result.AnswerIndex,
                        Guess = result.Guess,
                        IsCorrect = result.IsCorrect,
                        AnswerTimeSeconds =
                            result.AnswerTimeSeconds,
                        ModifiedTimeSeconds =
                            result.ModifiedTimeSeconds,
                        Points = result.Points,
                        HasSpeedBonus =
                            result.HasSpeedBonus
                    })
            ];
        }

        return
        [
            .. match.Players.Select(player =>
                new VsQuestionPlayerDto
                {
                    Position = player.Position,
                    HasAnswered =
                        player.CurrentAnswer
                            ?.QuestionNumber ==
                        match.Game.QuestionNumber
                })
        ];
    }

    private static VsRoundProgressDto[] BuildProgress(
        VsMatchSession match,
        VsMatchPlayerState currentPlayer)
    {
        var isCaptain =
            match.Game.CurrentRoundNumber >
            match.Classification.RequiredPartySize;
        var stepCount = isCaptain
            ? match.Players.Count
            : match.Players.Count + 1;
        var completedCount =
            currentPlayer.RoundProgress.Count;
        var currentStep = ResolveCurrentProgressStep(
            match,
            isCaptain);
        var questionOrder = ResolveQuestionOrder(match);
        var result = new List<VsRoundProgressDto>(stepCount);

        for (var index = 0; index < stepCount; index++)
        {
            var isGuess = !isCaptain && index == 0;
            var orderIndex = isCaptain ? index : index - 1;
            var playerPosition =
                isGuess ||
                orderIndex < 0 ||
                orderIndex >= questionOrder.Length
                    ? 0
                    : questionOrder[orderIndex];

            result.Add(new VsRoundProgressDto
            {
                StepNumber = index + 1,
                PlayerPosition = playerPosition,
                IsGuess = isGuess,
                IsCompleted = index < completedCount,
                IsCurrent = index == currentStep,
                Points = index < completedCount
                    ? currentPlayer.RoundProgress[index]
                    : 0
            });
        }

        return [.. result];
    }

    private static int ResolveCurrentProgressStep(
        VsMatchSession match,
        bool isCaptain)
    {
        if (match.Phase is
            VsMatchPhase.NormalRoundResult or
            VsMatchPhase.CaptainRoundResult or
            VsMatchPhase.GameCompleted)
        {
            return -1;
        }

        if (!isCaptain &&
            match.Game.QuestionKind ==
            VsQuestionKind.Guess)
        {
            return 0;
        }

        return isCaptain
            ? match.Game.CaptainOrderIndex
            : match.Game.CurrentQuestionerIndex + 1;
    }

    private static int[] ResolveQuestionOrder(
        VsMatchSession match) =>
        match.Game.CurrentRoundNumber >
        match.Classification.RequiredPartySize
            ? match.Game.CaptainOrder
            : match.Game.QuestionerOrder;

    private static VsCaptainQuestionDto[] BuildCaptainQuestions(
        VsMatchSession match,
        VsMatchPlayerState currentPlayer)
    {
        if (!CanChooseCaptainQuestion(match, currentPlayer))
            return [];

        return
        [
            .. VsMatchGameRules
                .GetCaptainChoices(match, currentPlayer)
                .Select(item => new VsCaptainQuestionDto
                {
                    LoadoutPosition =
                        item.LoadoutPosition,
                    CategoryId = item.CategoryId,
                    Question = item.Question
                })
        ];
    }

    private static bool CanChooseCaptainQuestion(
        VsMatchSession match,
        VsMatchPlayerState currentPlayer) =>
        match.Phase ==
            VsMatchPhase.CaptainQuestionSelection &&
        currentPlayer.IsConnected &&
        match.Game.CaptainOrder.Length >
            match.Game.CaptainOrderIndex &&
        currentPlayer.Position ==
            match.Game.CaptainOrder[
                match.Game.CaptainOrderIndex];

    private static VsRoundResultDto ToRoundResultDto(
        VsMatchRoundResultState result) =>
        new()
        {
            Position = result.Position,
            TotalBefore = result.TotalBefore,
            RoundPoints = result.RoundPoints,
            TotalAfter = result.TotalAfter,
            RoundTimeSeconds = result.RoundTimeSeconds,
            HasWinnerBonus = result.HasWinnerBonus,
            FastestBonusPoints =
                result.FastestBonusPoints,
            HasSpeedPositionGain =
                result.HasSpeedPositionGain,
            CharacterSlotNumber =
                result.CharacterSlotNumber,
            CharacterXp = result.CharacterXp,
            EnergyLoss = result.EnergyLoss
        };
}
