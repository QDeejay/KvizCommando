namespace KvizCommando.Client.Services.ClientCache
{
    public sealed class SessionService
    {
        public string? SessionId { get; set; }

        public bool HasSession => !string.IsNullOrWhiteSpace(SessionId);

        public void Clear()
        {
            SessionId = null;
        }
    }
}
