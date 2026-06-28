namespace KvizCommando.Client.Services.Visual.UiService
{
    public class DisplayMessageState : IDisplayMessageState
    {
        public event Action? OnChange;

        private List<string> _messages = new();
        public IReadOnlyList<string> Messages => _messages;

        public void SetMessages(IEnumerable<string> newMessages)
        {
            _messages = newMessages.ToList();
            OnChange?.Invoke();
        }
    }
}
