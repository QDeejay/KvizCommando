using System.ComponentModel.DataAnnotations.Schema;

namespace KvizCommando.Server.Domain.Entities.Statistics
{
    /// <summary>
    /// Kategóriánkénti válasz számlálók egy játékosra.
    /// PK: (PlayerId, CategoryId)
    /// </summary>
   
    public class PlayerCategoryStat
    {
        public int PlayerId { get; set; }       // FK → Players.PlayerId
        public short CategoryId { get; set; }   // 1..16 (későbbi bővíthetőség miatt short)

        public int Answered { get; set; }       // összes megválaszolt
        public int Correct { get; set; } // ebből helyes

        public int HighScore { get; set; }
        public double HighScoreTime { get; set; }


        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public decimal Ratio { get; private set; }
    }
    
}
