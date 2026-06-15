using System.ComponentModel.DataAnnotations.Schema;

namespace KvizCommando.Server.Domain.Entities.Statistics
{
    /// <summary>
    /// Egyszerű, végeredményes kérdezői statok játékos-szinten.
    /// </summary>
    public class PlayerAskStats
    {
        public int PlayerId { get; set; }   // PK, FK → Players.PlayerId

        public int TotalQuestionsAsked { get; set; }
        public int TotalAskPointsEarned { get; set; }

        /// <summary>
        /// Perzisztált számított átlag (gyors rangsoroláshoz).
        /// EF konfiguráció adja a HAS COMPUTED COLUMN SQL-t.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public decimal AveragePointsPerAsk { get; private set; }
    }
}
