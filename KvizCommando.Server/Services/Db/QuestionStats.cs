namespace KvizCommando.Server.Services.Db
{
    /// <summary>
    /// Összesíti, hogy a kérdésadatok mentése hány rekordot érintett.
    /// </summary>
    public class QuestionStats
    {
        /// <summary>Az összes érintett kérdés száma.</summary>
        public int totalQuestions { get; set; } = 0;

        /// <summary>A mentett saját kérdések száma.</summary>
        public int userQuestions { get; set; } = 0;

        /// <summary>A mentett függőben lévő kérdések száma.</summary>
        public int pendingQuestions { get; set; } = 0;

        /// <summary>A gyári kérdések közé áthelyezett kérdések száma.</summary>
        public int transferedQuestions { get; set; } = 0;
    }
}
