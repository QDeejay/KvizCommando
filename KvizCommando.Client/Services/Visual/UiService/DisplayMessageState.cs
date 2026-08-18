namespace KvizCommando.Client.Services.Visual.UiService
{
    public class DisplayMessageState : IDisplayMessageState
    {
        public event Action? OnChange;

        private List<string> _messages = new();
        public IReadOnlyList<string> Messages => _messages;

        /// <summary>
        /// Lecseréli a megjelenítendő üzenetek aktuális listáját.
        /// </summary>
        public void SetMessages(IEnumerable<string> newMessages)
        {
            _messages = newMessages.ToList();
            OnChange?.Invoke();
        }
    }
}
