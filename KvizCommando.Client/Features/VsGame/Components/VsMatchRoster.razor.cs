using KvizCommando.Client.Features.VsGame.ViewModels;
using KvizCommando.Shared.Models.Enums.VsGame;
using Microsoft.AspNetCore.Components;

namespace KvizCommando.Client.Features.VsGame.Components;

public partial class VsMatchRoster
{
    private const int EXCELLENT_RESPONSE_TIME_MILLISECONDS = 50;

    [Parameter, EditorRequired]
    public VsRosterPlayerVm[] Players { get; set; } = [];

    [Parameter] public int ExpectedPlayers { get; set; }
    [Parameter] public string Title { get; set; } = string.Empty;
    [Parameter] public bool ShowScore { get; set; }
    [Parameter] public bool IsConnectionActive { get; set; }
    [Parameter] public double? CurrentTimeModifierSeconds { get; set; }

    private int MissingPlayers =>
        Math.Max(0, ExpectedPlayers - Players.Length);

    private int RosterCardCount =>
        Players.Length + MissingPlayers;

    private int CompactColumnCount =>
        RosterCardCount is 3 or 4
            ? 2
            : RosterCardCount;

    private static string Seconds(double value) =>
        $"{value:0.0}s";

    private static string ModifierText(double value) =>
        $"{(value > 0 ? "+" : string.Empty)}{value:0.0}s";

    private static string ModifierClass(double value) =>
        value > 0
            ? "positive"
            : value < 0
                ? "negative"
                : "neutral";

    private static string PlayerToneClass(int position) =>
        $"player-tone-{position}";

    private VsConnectionQuality DisplayedConnectionQuality(
        VsRosterPlayerVm player) =>
        IsConnectionActive
            ? player.ConnectionQuality
            : VsConnectionQuality.Unknown;

    private string ConnectionTitle(VsRosterPlayerVm player) =>
        IsConnectionActive
            ? $"{player.ResponseTimeMilliseconds} ms"
            : string.Empty;

    private static string ConnectionQualityClass(
        VsConnectionQuality quality) =>
        quality switch
        {
            VsConnectionQuality.Good => "good",
            VsConnectionQuality.Medium => "medium",
            VsConnectionQuality.Bad => "bad",
            _ => "unknown"
        };

    private string ConnectionIconClass(
        VsRosterPlayerVm player) =>
        DisplayedConnectionQuality(player) switch
        {
            VsConnectionQuality.Good when
                player.ResponseTimeMilliseconds <=
                EXCELLENT_RESPONSE_TIME_MILLISECONDS =>
                    "bi-reception-4",
            VsConnectionQuality.Good => "bi-reception-3",
            VsConnectionQuality.Medium => "bi-reception-2",
            VsConnectionQuality.Bad => "bi-reception-1",
            _ => "bi-reception-0"
        };
}
