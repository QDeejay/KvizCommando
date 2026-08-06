using Microsoft.JSInterop;

namespace KvizCommando.Client.Services.Audio
{
    public sealed class AudioService
    {
        public const string SFX_CLICK = "Click.webm";
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
        private MusicTrack? _requestedMusic;

        public bool IsMuted { get; private set; } = true;

        public AudioService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task InitializeAsync()
        {
            if (_isInitialized)
                return;

            await _jsRuntime.InvokeVoidAsync(
                "audioEngine.initialize");
            await _jsRuntime.InvokeVoidAsync(
                "audioEngine.setMuted",
                true);
            _isInitialized = true;
            IsMuted = true;
        }

        public async Task SetMutedAsync(
            bool muted,
            MusicTrack? fallbackMusic = null)
        {
            IsMuted = muted;

            await _jsRuntime.InvokeVoidAsync(
                "audioEngine.setMuted",
                muted);

            if (!muted &&
                !_requestedMusic.HasValue &&
                fallbackMusic.HasValue)
            {
                await PlayMusicAsync(fallbackMusic.Value);
            }
        }

        public async Task PlayMusicAsync(MusicTrack track)
        {
            _requestedMusic = track;

            await _jsRuntime.InvokeVoidAsync(
                "audioEngine.playMusic",
                $"audio/music/{track}.webm");
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
            if (IsMuted)
                return;

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

    }
}

/**
 * MÓDOSÍTÁS: minden böngészőindulás némán kezdődik. A master mute nem
 * állítja le a lejátszást; unmute-kor az aktuális sáv folytatódik, vagy
 * a hívó által megadott fallback zene indul, ha még nincs aktív sáv.
 * Némított állapotban az egyszeri effektek létre sem jönnek.
 * A zenéket MusicTrack enum, az effekteket központi fájlnevek jelölik.
 */
