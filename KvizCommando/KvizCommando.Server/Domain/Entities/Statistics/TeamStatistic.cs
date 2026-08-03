using System.ComponentModel.DataAnnotations.Schema;

namespace KvizCommando.Server.Domain.Entities.Statistics;

public sealed class TeamStatistic
{
    public int PlayerId { get; set; }
    public int RankedPlayed { get; set; }
    public int RankedWon { get; set; }
    public int RankedHighScore { get; set; }
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

/**
 * ÚJ FÁJL: a játékos egyetlen, globális ranked highscore-ját,
 * összesített meccs-/győzelemszámát, tippeltérési statisztikáját és
 * a 2, 3, illetve 4 játékoshoz fenntartott helyezésszámlálókat tartalmazza.
 * A helyezések az adatbázisban egy JSON mezőben, a PlayerCache-ben
 * kicsomagolt tömbökként élnek. A ranked tippeltérés átlaga számított,
 * indexelt adatbázis-oszlop; a szerver csak a darabszámot és az
 * összesített abszolút eltérési arányt módosítja.
 */
