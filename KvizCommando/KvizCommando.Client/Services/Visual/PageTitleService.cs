namespace KvizCommando.Client.Services.Visual
{
    public class PageTitleService
    {
        public string Title { get; private set; } = string.Empty;
        public int NavPage { get; private set; } = 0;

        public bool IsBackNavigationEnabled => NavPage > 100;

        public event Action? OnTitleChanged;

        public void SetTitle(string title, int backNavigation)
        {
            Title = title;
            NavPage = backNavigation;
            OnTitleChanged?.Invoke();
        }
    }
}
