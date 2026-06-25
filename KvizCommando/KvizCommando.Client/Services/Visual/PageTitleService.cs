namespace KvizCommando.Client.Services.Visual
{
    public class PageTitleService
    {
        public string Title { get; private set; } = string.Empty;
        public int Rank { get; private set; } = -1;
        public int NavPage { get; private set; } = 0;
        public int PrevPage { get; private set; } = 0;
        public bool IsBackNavigationEnabled => NavPage > 100;

        public event Action? OnTitleChanged;

        public void SetTitle(string title, int currentPage, int rank)
        {
            PrevPage = NavPage;
            Title = title;
            NavPage = currentPage;
            Rank = rank>=0 ? rank : Rank;
            OnTitleChanged?.Invoke();
        }
    }
}
