namespace KvizCommando.Shared.Contracts.CheckIn
{
    public class TermsMeta
    {
        public string Version { get; set; } = default!;
        public string Url { get; set; } = default!;

        /// <summary>
        /// Az ÁSZF közzétételének UTC-időpontja ISO 8601 round-trip formátumban.
        /// </summary>
        public DateTime PublishedAt { get; set; }
    }
}
