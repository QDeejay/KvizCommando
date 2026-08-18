namespace KvizCommando.Client.Services.Visual.UiService
{
    public class PageHeaderService
    {
        public string Title { get; private set; } = string.Empty;
        public int PageIndex { get; private set; } = 0;
        public bool BackEna { get; private set; } = false;

        public string[] HeaderTexts { get; private set; } = [];
        public int SubPageIndex { get; private set; } = 0;

        public event Action? OnTitleChanged;
        public event Action? OnBackBtnEnaChanged;
        public event Action? OnBackBtnClicked;


        /// <summary>
        /// Beállítja az oldal fejlécének címét.
        /// </summary>
        public void SetTitle(string title, int index)
        {
            Title = title;
            PageIndex = index;
            OnTitleChanged?.Invoke();
        }

        /// <summary>
        /// Beállítja a vissza gomb engedélyezett állapotát.
        /// </summary>
        public void SetBackBtnEna(bool back)
        {
            BackEna = back;
            OnBackBtnEnaChanged?.Invoke();
        }

        /// <summary>
        /// Beállítja, hogy a vissza gomb böngészőelőzmény-műveletet kezdeményezzen.
        /// </summary>
        public void SetBackBtnToPushState()
        {
            OnBackBtnClicked?.Invoke();
        }

    }
}
