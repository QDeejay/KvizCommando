namespace KvizCommando.Client.Models.StoreModels
{
    public class CheckInSessionCache
    {
        public bool needsName { get; set; } = false;
        public bool needsTerms { get; set; } = false;
        public string termsVersion { get; set; } = string.Empty;
            
        public string url { get; set; } = string.Empty;

        public DateTime PublishedAt { get; set; } = DateTime.MinValue;
    }
}
