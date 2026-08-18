using System.ComponentModel.DataAnnotations.Schema;

namespace KvizCommando.Server.Domain.Entities.Statistics;

public sealed class TeamStatistic
{
    public int PlayerId { get; set; }
    public int RankedPlayed { get; set; }
    public int RankedWon { get; set; }
    public double RankedHighScore { get; set; }
    public double RankedHighScoreTime { get; set; }
    public int RankedGuessCount { get; set; }
    public decimal RankedGuessErrorTotal { get; set; }

    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public decimal RankedGuessErrorRatio { get; private set; }

    public string RankedPlacementsJson { get; set; } =
        "{\"Players2\":[0,0],\"Players3\":[0,0,0],\"Players4\":[0,0,0,0]}";

    [NotMapped]
    public RankedPlacementStatistic RankedPlacements { get; set; } = new();
}

public sealed class RankedPlacementStatistic
{
    public int[] Players2 { get; set; } = new int[2];
    public int[] Players3 { get; set; } = new int[3];
    public int[] Players4 { get; set; } = new int[4];
}
