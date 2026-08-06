using Microsoft.JSInterop;

namespace KvizCommando.Client.Services.Audio
{
    public sealed class AudioService
    {
        public const string MUSIC_MENU = "Menu02.webm";
        public const string MUSIC_BATTLE = "Battle01.webm";

        public const string SFX_SELECT = "Select.webm";
        public const string SFX_HIT = "Hit.webm";
        public const string SFX_EMPTY = "Empty.webm";
        public const string SFX_MISS = "Miss.webm";
        public const string SFX_RESULT = "Result.webm";
        public const string SFX_NEW_RECORD = "NewRecord.webm";
        public const string SFX_MATCH_WIN = "MatchWin.webm";
        public const string SFX_MATCH_COMPLETE = "MatchComplete.webm";
        public const string SFX_MATCH_LOSS = "MatchLoss.webm";

        private readonly IJSRuntime _jsRuntime;
        private bool _isInitialized;
        private string? _requestedMusic;

        public bool IsEnabled { get; private set; }

        public AudioService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task InitializeAsync(bool? enabled = null)
        {
            if (!_isInitialized)
            {
                await _jsRuntime.InvokeVoidAsync(
                    "audioEngine.initialize");
                _isInitialized = true;
            }

            if (enabled.HasValue)
                await SetEnabledAsync(enabled.Value);
        }

        public async Task SetEnabledAsync(bool enabled)
        {
            IsEnabled = enabled;

            await _jsRuntime.InvokeVoidAsync(
                "audioEngine.setEnabled",
                enabled);

            if (!enabled)
                return;

            _requestedMusic ??= MUSIC_MENU;
            await PlayMusicInternalAsync(_requestedMusic);
        }

        public async Task PlayMusicAsync(string fileName)
        {
            _requestedMusic = fileName;

            await PlayMusicInternalAsync(fileName);
        }

        public async Task StopMusicAsync()
        {
            _requestedMusic = null;

            await _jsRuntime.InvokeVoidAsync(
                "audioEngine.stopMusic");
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

        public async Task SetSfxVolumeAsync(double volume)
        {
            volume = Math.Clamp(volume, 0.0, 1.0);

            await _jsRuntime.InvokeVoidAsync(
                "audioEngine.setSfxVolume",
                volume);
        }

        public async Task StopAllAsync()
        {
            _requestedMusic = null;

            await _jsRuntime.InvokeVoidAsync(
                "audioEngine.stopAll");
        }

        private Task PlayMusicInternalAsync(string fileName) =>
            _jsRuntime.InvokeVoidAsync(
                "audioEngine.playMusic",
                $"audio/music/{fileName}").AsTask();
    }
}

/**
 * MÓDOSÍTÁS: a perzisztált master állapot valódi némítás. Kikapcsolva
 * a zene és az effektek idővonala tovább fut, csak nem hallható.
 * Visszakapcsoláskor az aktuális zene folytatódik; ha nincs kért zene,
 * a menüzene indul. A játékok hangfájljainak nevei egy helyen vannak.
 */
