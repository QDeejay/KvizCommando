namespace KvizCommando.Client.Services.Visual
{
    public class PageTitleService
    {
        public string Title { get; private set; } = string.Empty;
        public int Rank { get; private set; } = -1;
        public int NavPage { get; private set; } = 0;

        public bool IsBackNavigationEnabled => NavPage > 100;

        public event Action? OnTitleChanged;

        public void SetTitle(string title, int backNavigation, int rank)
        {
            Title = title;
            NavPage = backNavigation;
            Rank = rank;
            OnTitleChanged?.Invoke();
        }
    }
}
