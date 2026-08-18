namespace KvizCommando.Server.Domain.Entities.Statistics
{
    public sealed class PlayerOrientStat
    {
        public int PlayerId { get; set; }       // FK → Players.PlayerId
        public short OrientId { get; set; }   // 1..8 

        public int HighScore { get; set; }
        public double HighScoreTime { get; set; }

    }
}
