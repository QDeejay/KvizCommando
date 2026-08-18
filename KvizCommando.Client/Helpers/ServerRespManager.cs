using System.Text.Json;

namespace KvizCommando.Client.Helpers
{
    public static class ServerRespManager
    {
        /// <summary>
        /// JSON response-ból kiolvassa a megadott kulcs értékét.
        /// Ha nincs ilyen kulcs vagy nem szöveg, null-t ad vissza.
        /// </summary>
        public static async Task<string?> GetValueAsync(HttpResponseMessage response, string key)
        {
            var json = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(json))
                return null;

            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty(key, out var prop))
            {
                return prop.GetString();
            }

            return null;
        }
    }
}
