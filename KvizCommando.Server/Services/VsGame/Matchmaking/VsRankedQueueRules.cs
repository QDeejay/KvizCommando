namespace KvizCommando.Server.Services.VsGame.Matchmaking;

public static class VsRankedQueueRules
{
    public const int MINIMUM_PLAYERS = 2;
    public const int REQUIRED_PLAYERS = 4;
    public const int MAXIMUM_PLAYERS = 4;
    public const int INITIAL_WAIT_SECONDS = 30;
    public const int THIRD_PLAYER_EXTENSION_MAX_SECONDS = 15;
    public const int REENTRY_BLOCK_SECONDS = 30;
    public const int TIMER_INTERVAL_MS = 300;
}
