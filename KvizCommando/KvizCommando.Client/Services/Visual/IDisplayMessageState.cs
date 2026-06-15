namespace KvizCommando.Client.Services.Visual
{
    public interface IDisplayMessageState
    {
        event Action? OnChange;
        IReadOnlyList<string> Messages { get; }
        void SetMessages(IEnumerable<string> newMessages);
    }

}
