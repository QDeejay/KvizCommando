using Microsoft.AspNetCore.Components;

namespace KvizCommando.Client.Services.Visual.UiService
{
    public class PageHeaderService
    {
        public string Title { get; private set; } = string.Empty;
        public int PageIndex { get; private set; } = 0;
        public int Rank { get; private set; } = -1;
        public bool BackEna { get; private set; } = false;

        public event Action? OnTitleChanged;
        public event Action? OnRankChanged;
        public event Action? OnBackBtnEnaChanged;
        public event Action? OnBackBtnClicked;
        public void SetTitle(string title, int index)
        {
            Title = title;
            PageIndex = index;
            OnTitleChanged?.Invoke();
        }
        public void SetRank(int rank)
        {
            Rank = rank;
            OnRankChanged?.Invoke();
        }
        public void SetBackBtnEna(bool back) 
        {
            BackEna = back;
            OnBackBtnEnaChanged?.Invoke();
        }

        public void SetBackBtnToPushState()
        {
            OnBackBtnClicked?.Invoke();
        }

    }
}
