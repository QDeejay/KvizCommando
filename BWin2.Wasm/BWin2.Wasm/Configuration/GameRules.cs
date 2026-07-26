namespace BWin2.Wasm.Configuration;

internal static class GameRules
{
    public const int FirstDivisionTeamCount = 18;
    public const int AllTeamCount = 32;
    public const int LeagueRoundCount = 34;
    public const int CupRoundCount = 5;
    public const int PlayerCountPerTeam = 11;

    public const int MinimumStake = 200;
    public const int MaximumStake = 20_000;
    public const int StartingCredit = 2_000;

    public const int DelayTiny = 1;
    public const int DelaySlide = 8;
    public const int DelayShort = 12;
    public const int DelayMedium = 25;
    public const int DelayLong = 50;
    public const int DelayPause = 100;
    public const int DelayComment = 1_100;

    public const int MatchMinuteMilliseconds = 600;
    public const int CommentaryMilliseconds = 1_000;

    public static readonly string[] CupRoundNames =
    [
        string.Empty,
        "Round 1     ",
        "Round 2     ",
        "Quater-final",
        "Semi-final  ",
        "Final       "
    ];

    public static readonly string[] CupEventCodes =
    [
        string.Empty,
        "07RND1",
        "15RND2",
        "22QUAT",
        "27SEMI",
        "35FIN "
    ];

    public static readonly int[] CupWeeks = [0, 7, 15, 22, 27, 35];
}
