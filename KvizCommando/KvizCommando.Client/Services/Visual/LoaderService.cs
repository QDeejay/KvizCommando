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
    }
}

