using System.Text.Json.Serialization;

namespace KvizCommando.Client.Services.User
{
    /// <summary>
    /// Az Identity végpontok szabványos hibaválaszát írja le.
    /// </summary>
    public class IdentityProblemDetails
    {
        /// <summary>A hibatípus azonosítója.</summary>
        [JsonPropertyName("type")]
        public string Type { get; set; }

        /// <summary>A hiba rövid címe.</summary>
        [JsonPropertyName("title")]
        public string Title { get; set; }

        /// <summary>A válasz HTTP-státuszkódja.</summary>
        [JsonPropertyName("status")]
        public int? Status { get; set; }

        /// <summary>A hiba részletes leírása.</summary>
        [JsonPropertyName("detail")]
        public string Detail { get; set; }

        /// <summary>A mezőkhöz tartozó validációs hibák.</summary>
        [JsonPropertyName("errors")]
        public Dictionary<string, string[]> Errors { get; set; }
    }
}
