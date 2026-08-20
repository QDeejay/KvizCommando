using Blazored.LocalStorage;
using KvizCommando.Client.Models.Settings;
using KvizCommando.Client.Services.Audio;
using Microsoft.JSInterop;

namespace KvizCommando.Client.Services.Settings;

public sealed class SettingsService : ISettingsService
{
    private const string SETTINGS_STORAGE_KEY = "kc:settings";
    private const int MIN_VOLUME = 0;
    private const int MAX_VOLUME = 100;

    private readonly ILocalStorageService _localStorage;
    private readonly AudioService _audio;
    private readonly IJSRuntime _jsRuntime;
    private readonly SemaphoreSlim _loadLock = new(1, 1);

    private bool _isLoaded;

    public ClientSettings Current { get; private set; } = new();

    public SettingsService(
        ILocalStorageService localStorage,
        AudioService audio,
        IJSRuntime jsRuntime)
    {
        _localStorage = localStorage;
        _audio = audio;
        _jsRuntime = jsRuntime;
    }

    public async Task LoadAsync()
    {
        if (_isLoaded)
            return;

        await _loadLock.WaitAsync();

        try
        {
            if (_isLoaded)
                return;

            var stored = await _localStorage.GetItemAsync<ClientSettings>(
                SETTINGS_STORAGE_KEY);

            Current = Normalize(stored ?? new ClientSettings());

            if (stored is null || !HasSameValues(stored, Current))
            {
                await _localStorage.SetItemAsync(
                    SETTINGS_STORAGE_KEY,
                    Current);
            }

            await ApplyAsync();
            _isLoaded = true;
        }
        finally
        {
            _loadLock.Release();
        }
    }

    public async Task SaveAsync(ClientSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        Current = Normalize(settings);

        await _localStorage.SetItemAsync(
            SETTINGS_STORAGE_KEY,
            Current);

        await ApplyAsync();
        _isLoaded = true;
    }

    public Task SetSoundEnabledAsync(bool enabled) =>
        SaveAsync(new ClientSettings
        {
            SoundEnabled = enabled,
            MusicVolume = Current.MusicVolume,
            SfxVolume = Current.SfxVolume,
            Theme = Current.Theme,
            StartFullscreen = Current.StartFullscreen
        });

    public async Task ApplyAsync()
    {
        await _audio.SetMutedAsync(!Current.SoundEnabled);
        await _audio.SetMusicVolumeAsync(Current.MusicVolume / 100.0);
        await _audio.SetSfxVolumeAsync(Current.SfxVolume / 100.0);

        await _jsRuntime.InvokeVoidAsync(
            "settingsInterop.setTheme",
            Current.Theme.ToString().ToLowerInvariant());
    }

    public async Task TryEnterStartFullscreenAsync()
    {
        if (!Current.StartFullscreen)
            return;

        await _jsRuntime.InvokeVoidAsync(
            "settingsInterop.tryEnterFullscreen");
    }

    public async Task ExitFullscreenAsync()
    {
        await _jsRuntime.InvokeVoidAsync(
            "settingsInterop.exitFullscreen");
    }

    private static ClientSettings Normalize(ClientSettings settings)
    {
        var theme = Enum.IsDefined(settings.Theme)
            ? settings.Theme
            : VisualTheme.Default;

        return new ClientSettings
        {
            SoundEnabled = settings.SoundEnabled,
            MusicVolume = Math.Clamp(
                settings.MusicVolume,
                MIN_VOLUME,
                MAX_VOLUME),
            SfxVolume = Math.Clamp(
                settings.SfxVolume,
                MIN_VOLUME,
                MAX_VOLUME),
            Theme = theme,
            StartFullscreen = settings.StartFullscreen
        };
    }

    private static bool HasSameValues(
        ClientSettings left,
        ClientSettings right) =>
        left.SoundEnabled == right.SoundEnabled &&
        left.MusicVolume == right.MusicVolume &&
        left.SfxVolume == right.SfxVolume &&
        left.Theme == right.Theme &&
        left.StartFullscreen == right.StartFullscreen;
}
