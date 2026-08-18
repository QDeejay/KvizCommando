using KvizCommando.Client.Models.ViewModels.Ui;
using KvizCommando.Shared.Models.Enums;
using System.Threading.Tasks;

namespace KvizCommando.Client.Services.Visual.UiService
{
    public sealed class ToastService
    {
        private const int SHOW_DELAY_MS = 1000;
        private const int DISPLAY_DURATION_MS = 3000;
        private const int HIDE_ANIMATION_MS = 250;

        private readonly Queue<ToastMessage> _queue = new();

        private bool _isProcessing;
        private TaskCompletionSource? _closeSignal;

        public ToastMessage? Current { get; private set; }

        public bool IsVisible { get; private set; }

        public event Action? OnChanged;

        /// <summary>
        /// Sikeres értesítést jelenít meg.
        /// </summary>
        /// <param name="text">A megjelenítendő vagy elküldendő szöveg.</param>
        public void Success(string text)
            => _ = Show(text, ToastType.Success);

        /// <summary>
        /// Hibaértesítést jelenít meg.
        /// </summary>
        /// <param name="text">A megjelenítendő vagy elküldendő szöveg.</param>
        public void Error(string text)
            => _ = Show(text, ToastType.Error);

        /// <summary>
        /// Figyelmeztető értesítést jelenít meg.
        /// </summary>
        /// <param name="text">A megjelenítendő vagy elküldendő szöveg.</param>
        public void Brief(string text)
            => _ = Show(text, ToastType.Warning);

        /// <summary>
        /// Tájékoztató értesítést jelenít meg.
        /// </summary>
        /// <param name="text">A megjelenítendő vagy elküldendő szöveg.</param>
        public void Complete(string text)
            => _ = Show(text, ToastType.Info);

        /// <summary>
        /// Sorba állítja az értesítést, és szükség esetén elindítja az értesítési sor feldolgozását.
        /// </summary>
        /// <param name="text">Az értesítésben megjelenítendő szöveg.</param>
        /// <param name="type">Az értesítés megjelenési típusa.</param>
        public async Task Show(string text, ToastType type)
        {
            var toast = new ToastMessage
            {
                Text = text,
                Type = type
            };
            await Task.Delay(SHOW_DELAY_MS);
            _queue.Enqueue(toast);

            if (_isProcessing)
                return;

            _isProcessing = true;
            _ = ProcessQueueAsync();
        }

        /// <summary>
        /// Bezárja az aktuálisan megjelenített értesítést.
        /// </summary>
        public void Close()
        {
            if (!IsVisible)
                return;

            _closeSignal?.TrySetResult();
        }

        private async Task ProcessQueueAsync()
        {
            while (_queue.Count > 0)
            {
                Current = _queue.Dequeue();
                IsVisible = true;
                OnChanged?.Invoke();

                _closeSignal = new TaskCompletionSource();
                await Task.WhenAny(
                    Task.Delay(DISPLAY_DURATION_MS),
                    _closeSignal.Task);
                _closeSignal = null;

                IsVisible = false;
                OnChanged?.Invoke();

                await Task.Delay(HIDE_ANIMATION_MS);

                Current = null;
                OnChanged?.Invoke();
            }

            _isProcessing = false;
        }
    }
}
