using KvizCommando.Client.Models.ViewModels.Ui;

namespace KvizCommando.Client.Services.Visual.UiService
{
    public sealed class SubHeaderService
    {
        private SubHeaderVm[] _tabs = [];
        private int _index = 0;

        public IReadOnlyList<SubHeaderVm> Tabs => _tabs;
        public int Index => _index;


        public bool IsVisible => _tabs.Length > 0;

        public event Action? OnButtonsChanged;

        public event Action<int>? OnButtonClicked;

        /// <summary>
        /// Megjeleníti a gombsort, és az index alapján kijelöli az aktív gombot.
        /// </summary>
        /// <param name="buttons">A fejlécben megjelenítendő gombok.</param>
        /// <param name="index">A lokalizálandó érték indexe.</param>
        public void Show(List<SubHeaderVm> buttons, int index)
        {
            _tabs = [.. buttons];
            _index = index;
            OnButtonsChanged?.Invoke();
        }

        /// <summary>
        /// Elrejti az aktuális felületi elemet.
        /// </summary>
        public void Hide()
        {
            Show([], 0);
        }

        /// <summary>
        /// Továbbítja a kiválasztott alfejlécgomb indexét a feliratkozóknak.
        /// </summary>
        /// <param name="index">A lokalizálandó érték indexe.</param>
        public void Click(int index)
        {
            if ((uint)index > (uint)_tabs.Length)
                return;

            OnButtonClicked?.Invoke(index);
        }
    }
}
