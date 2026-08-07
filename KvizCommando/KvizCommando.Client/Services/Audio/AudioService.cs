using Microsoft.JSInterop;

namespace KvizCommando.Client.Services.Audio
{
    public sealed class AudioService
    {
        private const string AUDIO_ASSET_VERSION = "2";

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

        private static readonly string[] SFX_FILES =
        [
            SFX_CLICK,
            SFX_SELECT,
            SFX_HIT,
            SFX_EMPTY,
            SFX_MISS,
            SFX_RESULT,
            SFX_NEW_RECORD,
            SFX_MATCH_WIN,
            SFX_MATCH_COMPLETE,
            SFX_MATCH_LOSS
        ];

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
                "audioEngine.preloadSfx",
                (object)Array.ConvertAll(
                    SFX_FILES,
                    BuildSfxPath));
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
                WithAssetVersion($"audio/music/{track}.webm"));
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
                BuildSfxPath(fileName));
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

        private static string WithAssetVersion(string path) =>
            $"{path}?v={AUDIO_ASSET_VERSION}";

        private static string BuildSfxPath(string fileName) =>
            WithAssetVersion($"audio/sfx/{fileName}");

    }
}

/**
 * MÓDOSÍTÁS: minden böngészőindulás némán kezdődik. A master mute nem
 * állítja le a lejátszást; unmute-kor az aktuális sáv folytatódik, vagy
 * a hívó által megadott fallback zene indul, ha még nincs aktív sáv.
 * Némított állapotban az egyszeri effektek lejátszása el sem indul.
 * A zenéket MusicTrack enum, az effekteket központi fájlnevek jelölik.
 * Az AUDIO_ASSET_VERSION kézi emelése új URL-t ad minden audiofájlnak,
 * ezért tartalomcsere után a böngésző biztosan az új hangot tölti le.
 * A rövid effektek inicializáláskor csak előtöltődnek; nem játszódnak le,
 * viszont az első tényleges használatkor sincs hálózati/dekódolási késés.
 */
