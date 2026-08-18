namespace KvizCommando.Client.Services.Visual.UiService
{
    public interface IDisplayMessageState
    {
        event Action? OnChange;
        IReadOnlyList<string> Messages { get; }
        /// <summary>
        /// Lecseréli a megjelenítendő üzenetek aktuális listáját.
        /// </summary>
        /// <param name="newMessages">A felületen megjelenítendő üzenetek.</param>
        void SetMessages(IEnumerable<string> newMessages);
    }

}
