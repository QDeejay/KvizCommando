namespace KvizCommando.Client.Services.Visual.UiService
{
    public interface IDisplayMessageState
    {
        event Action? OnChange;
        IReadOnlyList<string> Messages { get; }
        /// <summary>
        /// Lecseréli a megjelenítendő üzenetek aktuális listáját.
        /// </summary>
        void SetMessages(IEnumerable<string> newMessages);
    }

}
