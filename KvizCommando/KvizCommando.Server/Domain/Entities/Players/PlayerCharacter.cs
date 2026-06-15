using System;

namespace KvizCommando.Server.Domain.Entities.Players
{
    /// <summary>
    /// Egy játékos karakter-slotja (1..8). Az Attitude és CharStatistic JSON-ban tárolt adatok.
    /// </summary>
   
    public class PlayerCharacter
    {
        public int PlayerId { get; set; } // PK, FK → Players.PlayerId
        public string CharactersJson { get; set; } = string.Empty;
        public string CandidatesJson { get; set; } = string.Empty;
        public DateTime UpdatedUtc { get; set; }
    }

}
