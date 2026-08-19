using KvizCommando.Client.Data;
using KvizCommando.Client.Features.VsGame.ViewModels;
using KvizCommando.Client.Helpers;
using KvizCommando.Client.Services.Visual.UiService.Language;
using KvizCommando.Shared.Contracts.VsGame.Match;
using KvizCommando.Shared.Models.Enums.VsGame;

namespace KvizCommando.Client.Features.VsGame.Builders;

partial class VsMatchViewBuilder
{
    private VsGameViewData BuildGame(
        VsGameDto data,
        string culture) =>
        new()
        {
            CurrentRoundNumber = data.CurrentRoundNumber,
            NormalRoundCount = data.NormalRoundCount,
            QuestionNumber = data.QuestionNumber,
            SpeedBonusPoolPoints =
                data.SpeedBonusPoolPoints,
            QuestionKind = data.QuestionKind,
            QuestionerPosition = data.QuestionerPosition,
            Question = data.Question,
            Answers = data.Answers,
            CorrectAnswerIndex = data.CorrectAnswerIndex,
            CorrectGuess = data.CorrectGuess,
            MyAnswerIndex = data.MyAnswerIndex,
            MyGuess = data.MyGuess,
            MyTimeModifierSeconds =
                data.MyTimeModifierSeconds,
            MyGuessRangeMinimum =
                data.MyGuessRangeMinimum,
            MyGuessRangeMaximum =
                data.MyGuessRangeMaximum,
            MyHiddenAnswerIndices =
                data.MyHiddenAnswerIndices,
            MySuggestedAnswerIndex =
                data.MySuggestedAnswerIndex,
            MyHelp = data.MyHelpType == VsHelpType.None
                ? null
                : BuildHelp(new VsHelpCardDto
                {
                    HelpType = data.MyHelpType,
                    Count = 1
                }),
            MyHelpUsesRemaining =
                data.MyHelpUsesRemaining,
            IsMyHelpUnlimited =
                data.IsMyHelpUnlimited,
            CanUseHelp = data.CanUseHelp,
            MyRoundPoints = data.MyRoundPoints,
            MyRoundTimeSeconds =
                data.MyRoundTimeSeconds,
            CanAnswer = data.CanAnswer,
            CanChooseCaptainQuestion =
                data.CanChooseCaptainQuestion,
            QuestionPlayers =
            [
                .. data.QuestionPlayers.Select(item =>
                    new VsQuestionPlayerVm
                    {
                        Position = item.Position,
                        HasAnswered = item.HasAnswered,
                        AnswerIndex = item.AnswerIndex,
                        Guess = item.Guess,
                        IsCorrect = item.IsCorrect,
                        AnswerTimeSeconds =
                            item.AnswerTimeSeconds,
                        ModifiedTimeSeconds =
                            item.ModifiedTimeSeconds,
                        Points = item.Points,
                        HasSpeedBonus =
                            item.HasSpeedBonus
                    })
            ],
            Progress =
            [
                .. data.Progress.Select(item =>
                    new VsRoundProgressVm
                    {
                        StepNumber = item.StepNumber,
                        PlayerPosition =
                            item.PlayerPosition,
                        IsGuess = item.IsGuess,
                        IsCompleted = item.IsCompleted,
                        IsCurrent = item.IsCurrent,
                        Points = item.Points
                    })
            ],
            RoundResult =
            [
                .. data.RoundResult.Select(item =>
                    new VsRoundResultVm
                    {
                        Position = item.Position,
                        TotalBefore = item.TotalBefore,
                        RoundPoints = item.RoundPoints,
                        TotalAfter = item.TotalAfter,
                        RoundTimeSeconds =
                            item.RoundTimeSeconds,
                        HasWinnerBonus =
                            item.HasWinnerBonus,
                        FastestBonusPoints =
                            item.FastestBonusPoints,
                        HasSpeedPositionGain =
                            item.HasSpeedPositionGain,
                        CharacterSlotNumber =
                            item.CharacterSlotNumber,
                        CharacterXp = item.CharacterXp,
                        EnergyLoss = item.EnergyLoss
                    })
            ],
            CaptainQuestions =
            [
                .. data.CaptainQuestions.Select(item =>
                    new VsCaptainQuestionVm
                    {
                        LoadoutPosition =
                            item.LoadoutPosition,
                        CategoryId = item.CategoryId,
                        CategoryName =
                            ResolveCategoryName(
                                item.CategoryId,
                                culture),
                        ImageSrc =
                            ResolveCategoryImage(
                                item.CategoryId),
                        Question = item.Question
                    })
            ],
            CaptainOrder = data.CaptainOrder,
            CaptainOrderIndex = data.CaptainOrderIndex
        };
}
