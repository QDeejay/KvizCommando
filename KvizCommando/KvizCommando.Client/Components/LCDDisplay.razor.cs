using Microsoft.AspNetCore.Components;
using KvizCommando.Client.Services.Visual;

namespace KvizCommando.Client.Components
{
    public partial class LCDDisplay : ComponentBase, IDisposable
    {
        [Parameter] public string Operation { get; set; } = "run"; // vagy "step"
        [Parameter] public bool Next { get; set; } = false;
        [Parameter] public string DisplayLed { get; set; } = "On"; // vagy "Off"
        [Parameter] public string LedColor { get; set; } = "#222F16"; // például
        [Parameter] public string BgOn { get; set; } = "#D2FF39";
        [Parameter] public string BgOff { get; set; } = "#D2FF39";
        [Parameter] public string FgOn { get; set; } = "#222F16";
        [Parameter] public string FgOff { get; set; } = "#222F16";
        [Inject] public IDisplayMessageState DisplayState { get; set; } = default!;

        private int _currentIndex = 0;
        private string? CurrentText;
        private bool IsVisible = false;
        private Timer? _timer;

        protected override void OnInitialized()
        {
            DisplayState.OnChange += HandleStateChange;

            // Az első szöveget azonnal betesszük, ha van!
            if (DisplayState.Messages.Count > 0)
            {
                CurrentText = DisplayState.Messages[0];
                IsVisible = true;
            }

            StartAnimation();
        }

        private void HandleStateChange()
        {
            _currentIndex = 0;
            if (DisplayState.Messages.Count > 0)
            {
                CurrentText = DisplayState.Messages[0];
                IsVisible = true;
            }
            InvokeAsync(StateHasChanged);
        }

        private void StartAnimation()
        {
            _timer = new Timer(_ =>
            {
                InvokeAsync(() =>
                {
                    if (DisplayState.Messages.Count == 0)
                    {
                        CurrentText = null;
                        StateHasChanged();
                        return;
                    }
                    IsVisible = false;
                    StateHasChanged();

                    // Késleltetés, hogy eltűnjön az előző
                    Task.Delay(400).ContinueWith(_ =>
                    {
                        _currentIndex = (_currentIndex + 1) % DisplayState.Messages.Count;
                        CurrentText = DisplayState.Messages[_currentIndex];
                        IsVisible = true;
                        InvokeAsync(StateHasChanged);
                    });
                });
            }, null, 2000, 2000);
        }
        protected override void OnParametersSet()
        {
            if (Operation == "step" && Next && DisplayState.Messages.Count > 0)
            {
                IsVisible = false;
                StateHasChanged();

                Task.Delay(400).ContinueWith(_ =>
                {
                    _currentIndex = (_currentIndex + 1) % DisplayState.Messages.Count;
                    CurrentText = DisplayState.Messages[_currentIndex];
                    IsVisible = true;
                    InvokeAsync(StateHasChanged);
                });
            }
        }

        public void Dispose()
        {
            _timer?.Dispose();
            DisplayState.OnChange -= HandleStateChange;
        }
    }
}
