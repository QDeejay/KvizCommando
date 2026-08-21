using KvizCommando.Shared.Models.Rules;

namespace KvizCommando.Server.Services.VsGame.Match;

public sealed class VsMatchProfile
{
    public int PreparationSeconds { get; init; } =
        VsRankedMatchRules.PREPARATION_SECONDS;
    public int GuessSeconds { get; init; } =
        VsRankedMatchRules.GUESS_SECONDS;
    public int QuestionSeconds { get; init; } =
        VsRankedMatchRules.QUESTION_SECONDS;
    public int AnswerRevealDelaySeconds { get; init; } = 1;
    public int QuestionPauseSeconds { get; init; } = 2;
    public int RoundResultSeconds { get; init; } = 5;
    public int PhasePauseSeconds { get; init; } = 3;
    public int CaptainSelectionSeconds { get; init; } = 10;
    public int BotMinimumAnswerSeconds { get; init; } = 3;
    public int BotMaximumAnswerSeconds { get; init; } = 7;
    public double TimeFreezeModifierSeconds { get; init; } = -99;
    public int TimeFreezeWrongAnswerPenaltySeconds { get; init; } =
        VsRankedMatchRules.TIME_FREEZE_WRONG_ANSWER_PENALTY_SECONDS;
    public int PointUnit { get; init; } =
        VsRankedMatchRules.POINT_UNIT;
    public int CaptainMultiplier { get; init; } =
        VsRankedMatchRules.CAPTAIN_MULTIPLIER;
    public int GoodResponseTimeMilliseconds { get; init; } = 100;
    public int MaximumResponseTimeMilliseconds { get; init; } = 250;
    public bool PausePreparationOnTimeout { get; init; } = false;
}

public static class VsMatchProfiles
{
    public static readonly VsMatchProfile Ranked = new();
}
