using Blazored.LocalStorage;
using KvizCommando.Client.Features.Shared.Help;
using KvizCommando.Client.Models.Settings;
using KvizCommando.Client.Services.Audio;
using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Client.Services.Settings;
using KvizCommando.Client.Utilities;
using Microsoft.AspNetCore.Components;

namespace KvizCommando.Client.Features.Shared.Settings;

public partial class SettingsNavigator : KcComponentBase
{
    [Inject] private ISettingsService Settings { get; set; } = default!;
    [Inject] private ILocalStorageService LocalStorage { get; set; } = default!;
    [Inject] private AudioService Audio { get; set; } = default!;

    [Parameter] public AppState AppStates { get; set; } = default!;
    [Parameter] public EventCallback OnSettingsChanged { get; set; }

    private ClientSettings _draft = new();
    private SettingsSection _selectedSection = SettingsSection.Sound;
    private bool _isOpen;
    private bool _helpsReset;

    /// <summary>Megnyitja a beállításablakot az aktuális beállítások másolatával.</summary>
    public async Task ShowAsync()
    {
        await Settings.LoadAsync();
        _draft = Copy(Settings.Current);
        _selectedSection = SettingsSection.Sound;
        _helpsReset = false;
        _isOpen = true;
        StateHasChanged();
    }

    /// <summary>Bezárja a beállításablakot, ha nyitva van.</summary>
    public async Task Close()
    {
        if (!_isOpen)
            return;

        await Audio.PlaySfxAsync(AudioService.SFX_UI_TOUCH);
        _isOpen = false;
        _helpsReset = false;
    }

    private async Task SelectSection(SettingsSection section)
    {
        await Audio.PlaySfxAsync(AudioService.SFX_UI_TOUCH);
        _selectedSection = section;
        _helpsReset = false;
    }

    private async Task SetThemeAsync(VisualTheme theme)
    {
        await Audio.PlaySfxAsync(AudioService.SFX_UI_TOUCH);
        _draft.Theme = theme;
        await SaveAsync();
    }

    private async Task SaveAsync()
    {
        await Settings.SaveAsync(_draft);
        _draft = Copy(Settings.Current);
        await OnSettingsChanged.InvokeAsync();
    }

    private async Task SaveSfxVolumeAsync()
    {
        await SaveAsync();
        await Audio.PlaySfxAsync(AudioService.SFX_CLICK);
    }

    private async Task ResetHelpsAsync()
    {
        await Audio.PlaySfxAsync(AudioService.SFX_UI_TOUCH);
        await LocalStorage.RemoveItemAsync(HelpCollection.SEEN_STORAGE_KEY);
        AppStates.LocStoreStates.SeenHelps.Clear();
        _helpsReset = true;
    }

    private static ClientSettings Copy(ClientSettings source) => new()
    {
        SoundEnabled = source.SoundEnabled,
        MusicVolume = source.MusicVolume,
        SfxVolume = source.SfxVolume,
        Theme = source.Theme,
        StartFullscreen = source.StartFullscreen
    };

    private enum SettingsSection
    {
        Sound,
        Appearance,
        Help
    }
}
