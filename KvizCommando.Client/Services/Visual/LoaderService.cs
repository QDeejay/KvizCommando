
namespace KvizCommando.Client.Services.Visual
{
    public sealed class LoaderService
    {
        public event Action? OnShow;
        public event Action? OnHide;

        public bool IsVisible { get; private set; }
        private DateTime _triggerAt = DateTime.MinValue;
        private DateTime _hideAt = DateTime.MinValue;
        private bool _running = false;

        /// <summary>
        /// Elindítja a késleltetett betöltésjelző időzítését.
        /// </summary>
        public void Trigger()
        {
            _triggerAt = DateTime.UtcNow;
            if (!_running)
            {
                _running = true;
                _ = RunAsync();
            }
        }

        /// <summary>
        /// Elrejti az aktuális felületi elemet.
        /// </summary>
        public void Hide()
        {
            _hideAt = DateTime.UtcNow;
        }

        private async Task RunAsync()
        {
            while (_running)
            {
                await Task.Delay(50);
                var now = DateTime.UtcNow;

                if (!IsVisible)
                {
                    // A gyorsan befejeződő műveletek nem villantják fel a betöltésjelzőt.
                    if (_hideAt > _triggerAt)
                    {
                        if (now - _hideAt >= TimeSpan.FromSeconds(1))
                        {
                            _running = false;
                            return;
                        }
                        // A várakozási idő alatt érkező új művelet azonnal láthatóvá teszi a jelzőt.
                        if (_triggerAt > _hideAt)
                        {
                            IsVisible = true;
                            OnShow?.Invoke();
                        }
                    }
                    else if (now - _triggerAt >= TimeSpan.FromMilliseconds(500))
                    {
                        IsVisible = true;
                        OnShow?.Invoke();
                    }
                }
                else
                {
                    // A minimális láthatósági idő megakadályozza a rövid felvillanást.
                    if (_hideAt > _triggerAt && now - _hideAt >= TimeSpan.FromSeconds(1))
                    {
                        IsVisible = false;
                        OnHide?.Invoke();
                        _running = false;
                        return;
                    }
                }
            }
        }

    }

}
