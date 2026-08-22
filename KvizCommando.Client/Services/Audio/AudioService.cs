using Microsoft.JSInterop;

namespace KvizCommando.Client.Services.Audio
{
    public sealed class AudioService
    {
        private const string AUDIO_ASSET_VERSION = "3";

        public const string SFX_CLICK = "Click.webm";
        public const string SFX_UI_TOUCH = "UiTouch.webm";
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
            SFX_UI_TOUCH,
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
        private Task? _sfxPreloadTask;
        public AudioService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        /// <summary>
        /// Beállítja a teljes hangkimenet némítását.
        /// </summary>
        /// <param name="muted">Az összes hang némításának kívánt állapota.</param>
        public async Task SetMutedAsync(bool muted)
        {
            await _jsRuntime.InvokeVoidAsync(
                "kcHowler.setMuted",
                muted);
        }

        /// <summary>
        /// Elindítja a megadott háttérzene lejátszását.
        /// </summary>
        /// <param name="track">A lejátszandó háttérzene fájlneve vagy útvonala.</param>
        public async Task PlayMusicAsync(MusicTrack track)
        {
            await _jsRuntime.InvokeVoidAsync(
                "kcHowler.playMusic",
                WithAssetVersion($"audio/music/{track}.webm"));
        }

        /// <summary>
        /// Leállítja a háttérzene lejátszását.
        /// </summary>
        public async Task StopMusicAsync()
        {
            await _jsRuntime.InvokeVoidAsync(
                "kcHowler.stopMusic");
        }

        /// <summary>
        /// Beállítja a háttérzene hangerejét.
        /// </summary>
        /// <param name="volume">A beállítandó hangerő a szolgáltatás által elfogadott tartományban.</param>
        public async Task SetMusicVolumeAsync(double volume)
        {
            volume = Math.Clamp(volume, 0.0, 1.0);

            await _jsRuntime.InvokeVoidAsync(
                "kcHowler.setMusicVolume",
                volume);
        }

        /// <summary>
        /// Lejátssza a megadott hangeffektust.
        /// </summary>
        /// <param name="fileName">A lejátszandó hangeffektus fájlneve.</param>
        public async Task PlaySfxAsync(string fileName)
        {
            await EnsureSfxPreloadedAsync();
            await _jsRuntime.InvokeVoidAsync(
                "kcHowler.playSfx",
                BuildSfxPath(fileName));
        }

        /// <summary>
        /// Beállítja a hangeffektusok hangerejét.
        /// </summary>
        /// <param name="volume">A beállítandó hangerő a szolgáltatás által elfogadott tartományban.</param>
        public async Task SetSfxVolumeAsync(double volume)
        {
            volume = Math.Clamp(volume, 0.0, 1.0);

            await _jsRuntime.InvokeVoidAsync(
                "kcHowler.setSfxVolume",
                volume);
        }

        /// <summary>
        /// Leállítja az összes aktív hanglejátszást.
        /// </summary>
        public async Task StopAllAsync()
        {
            await _jsRuntime.InvokeVoidAsync(
                "kcHowler.stopAll");
        }

        private Task EnsureSfxPreloadedAsync() =>
            _sfxPreloadTask ??= _jsRuntime.InvokeVoidAsync(
                "kcHowler.preloadSfx",
                (object)Array.ConvertAll(
                    SFX_FILES,
                    BuildSfxPath)).AsTask();

        private static string WithAssetVersion(string path) =>
            $"{path}?v={AUDIO_ASSET_VERSION}";

        private static string BuildSfxPath(string fileName) =>
            WithAssetVersion($"audio/sfx/{fileName}");

    }
}
