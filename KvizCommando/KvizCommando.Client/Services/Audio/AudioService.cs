using Microsoft.JSInterop;

namespace KvizCommando.Client.Services.Audio
{
    public sealed class AudioService
    {
        private readonly IJSRuntime _jsRuntime;
        public bool EnteredNormal { get; set; }
        public AudioService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task InitializeAsync()
        {
            await _jsRuntime.InvokeVoidAsync(
                "audioEngine.initialize");
        }

        public async Task PlayMusicAsync(string fileName)
        {
            await _jsRuntime.InvokeVoidAsync(
                "audioEngine.playMusic",
                $"audio/music/{fileName}");
        }

        public async Task StopMusicAsync()
        {
            await _jsRuntime.InvokeVoidAsync(
                "audioEngine.stopMusic");
        }

        public async Task SetMusicEnabledAsync(bool enabled)
        {
            await _jsRuntime.InvokeVoidAsync(
                "audioEngine.setMusicEnabled",
                enabled);
        }

        public async Task SetMusicVolumeAsync(double volume)
        {
            volume = Math.Clamp(volume, 0.0, 1.0);

            await _jsRuntime.InvokeVoidAsync(
                "audioEngine.setMusicVolume",
                volume);
        }

        public async Task PlaySfxAsync(string fileName)
        {
            await _jsRuntime.InvokeVoidAsync(
                "audioEngine.playSfx",
                $"audio/sfx/{fileName}");
        }

        public async Task SetSfxEnabledAsync(bool enabled)
        {
            await _jsRuntime.InvokeVoidAsync(
                "audioEngine.setSfxEnabled",
                enabled);
        }

        public async Task SetSfxVolumeAsync(double volume)
        {
            volume = Math.Clamp(volume, 0.0, 1.0);

            await _jsRuntime.InvokeVoidAsync(
                "audioEngine.setSfxVolume",
                volume);
        }

        public async Task StopAllAsync()
        {
            await _jsRuntime.InvokeVoidAsync(
                "audioEngine.stopAll");
        }
    }
}
