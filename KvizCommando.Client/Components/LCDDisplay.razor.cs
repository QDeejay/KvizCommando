using Microsoft.AspNetCore.Components;
using KvizCommando.Client.Utilities;
using KvizCommando.Client.Services.Visual.UiService;

namespace KvizCommando.Client.Components
{
    public partial class LCDDisplay : IDisposable
    {
        [Parameter] public bool SoundEnabled { get; set; }
        [Parameter] public EventCallback<bool> SoundEnabledChanged { get; set; }
        [Inject] public IDisplayMessageState DisplayState { get; set; } = default!;

        private int _currentIndex = 0;
        private string? CurrentText;
        private bool IsVisible = false;
        private Timer? _timer;
        private bool _isPaused;
        private bool _isDisposed;

        protected override void OnInitialized()
        {
            DisplayState.OnChange += HandleStateChange;

            ShowFirstMessage();

            StartAnimation();
        }

        private void StartAnimation()
        {
            _timer = new Timer(_ =>
            {
                if (!_isPaused)
                    _ = InvokeAsync(() =>
                        ShowNextMessageAsync(false));
            }, null, 2000, 2000);
        }

        private async Task ShowNextMessageAsync(bool isManualStep)
        {
            if (_isDisposed)
                return;

            if (DisplayState.Messages.Count == 0)
            {
                CurrentText = null;
                StateHasChanged();
                return;
            }

            IsVisible = false;
            StateHasChanged();
            await Task.Delay(400);

            if (_isDisposed || DisplayState.Messages.Count == 0)
                return;

            if (_isPaused && !isManualStep)
            {
                IsVisible = true;
                StateHasChanged();
                return;
            }

            _currentIndex =
                (_currentIndex + 1) % DisplayState.Messages.Count;
            CurrentText = DisplayState.Messages[_currentIndex];
            IsVisible = true;
            StateHasChanged();
        }

        private void TogglePlayback() =>
            _isPaused = !_isPaused;

        private Task StepAsync() =>
            ShowNextMessageAsync(true);

        private Task ToggleSoundAsync() =>
            SoundEnabledChanged.InvokeAsync(!SoundEnabled);

        private void HandleStateChange() =>
            _ = InvokeAsync(() =>
            {
                ShowFirstMessage();
                StateHasChanged();
            });

        private void ShowFirstMessage()
        {
            _currentIndex = 0;
            CurrentText = DisplayState.Messages.Count == 0
                ? null
                : DisplayState.Messages[0];
            IsVisible = CurrentText is not null;
        }

        /// <summary>
        /// Felszabadítja a példány által használt erőforrásokat.
        /// </summary>
        public void Dispose()
        {
            _isDisposed = true;
            _timer?.Dispose();
            DisplayState.OnChange -= HandleStateChange;
            GC.SuppressFinalize(this);
        }
    }
}
