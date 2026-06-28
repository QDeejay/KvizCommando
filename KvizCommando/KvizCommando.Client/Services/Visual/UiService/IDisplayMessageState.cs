namespace KvizCommando.Client.Services.Visual.UiService
{
    public interface IDisplayMessageState
    {
        event Action? OnChange;
        IReadOnlyList<string> Messages { get; }
        void SetMessages(IEnumerable<string> newMessages);
    }

}
