namespace KvizCommando.Client.Services.ClientCache
{
    public sealed class SessionService
    {
        public string? SessionId { get; set; }

        public bool PendingSessionReplacementWarning { get; set; }

        public bool HasSession => !string.IsNullOrWhiteSpace(SessionId);

        /// <summary>
        /// Törli a szolgáltatásban tárolt aktuális állapotot.
        /// </summary>
        public void Clear()
        {
            SessionId = null;
            PendingSessionReplacementWarning = false;
        }
    }
}
