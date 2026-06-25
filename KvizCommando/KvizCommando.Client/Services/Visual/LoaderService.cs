using KvizCommando.Client.Services.Dto;
using Microsoft.JSInterop;
using System.ComponentModel.Design;
using static System.Net.WebRequestMethods;

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

        public void Trigger()
        {
            _triggerAt = DateTime.UtcNow;
            if (!_running)
            {
                _running = true;
                _ = RunAsync();
            }
        }

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

                // 1. Még nem látszik
                if (!IsVisible)
                {
                    // Volt Hide az utolsó Trigger után? Akkor cooldown van
                    if (_hideAt > _triggerAt)
                    {
                        // Cooldown lejárt 1s után
                        if (now - _hideAt >= TimeSpan.FromSeconds(1))
                        {
                            _running = false;
                            return;
                        }
                        // Cooldown alatt jött új Trigger -> azonnal mutat
                        if (_triggerAt > _hideAt)
                        {
                            IsVisible = true;
                            OnShow?.Invoke();
                        }
                    }
                    // Nincs Hide, eltelt 500ms -> mutat
                    else if (now - _triggerAt >= TimeSpan.FromMilliseconds(500))
                    {
                        IsVisible = true;
                        OnShow?.Invoke();
                    }
                }
                // 2. Látszik
                else
                {
                    // Volt Hide és eltelt 1s -> elrejt
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
        /*
        public bool IsVisible { get; private set; }
        private DateTime _lastTrigger;
        private bool _workerRunning;

        public void Trigger()
        {
            _lastTrigger = DateTime.UtcNow;
            if (!IsVisible)
            {
                IsVisible = true;
                OnShow?.Invoke();
            }

            if (!_workerRunning)
            {
                _workerRunning = true;
                _ = WorkerAsync();
            }
        }
        private async Task WorkerAsync()
        {
            while (_workerRunning)
            {
                await Task.Delay(100);
                if (DateTime.UtcNow - _lastTrigger < TimeSpan.FromMilliseconds(1000))
                    continue;
                IsVisible = false;
                _workerRunning= false;
                OnHide?.Invoke();
                return;
            }
        }

        private async Task WatchPeriod()
        { 
                    
        }

    }
}
*/
