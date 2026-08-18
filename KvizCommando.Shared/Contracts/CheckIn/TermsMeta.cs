// src/Shared/Contracts/CheckIn/TermsMeta.cs
namespace KvizCommando.Shared.Contracts.CheckIn
{
    public class TermsMeta
    {
        public string Version { get; set; } = default!;
        public string Url { get; set; } = default!;

        /// <summary>
        /// UTC time in ISO-8601 "O" (round-trip) format.
        /// </summary>
        public DateTime PublishedAt { get; set; }
    }
}
