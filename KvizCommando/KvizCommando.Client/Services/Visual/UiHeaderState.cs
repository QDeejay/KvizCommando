namespace KvizCommando.Client.Services.Visual
{
    public sealed class UiHeaderState
    {
        private bool _hideLanguageSelector;

        public bool HideLanguageSelector => _hideLanguageSelector;

        public event Action? Changed;

        public void HideLang()
        {
            if (_hideLanguageSelector) return;
            _hideLanguageSelector = true;
            Changed?.Invoke();
        }

        public void ShowLang()
        {
            if (!_hideLanguageSelector) return;
            _hideLanguageSelector = false;
            Changed?.Invoke();
        }
    }
}
