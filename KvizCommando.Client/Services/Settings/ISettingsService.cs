using KvizCommando.Client.Models.Settings;

namespace KvizCommando.Client.Services.Settings;

public interface ISettingsService
{
    ClientSettings Current { get; }

    Task LoadAsync();
    Task SaveAsync(ClientSettings settings);
    Task ApplyAsync();
    Task SetSoundEnabledAsync(bool enabled);
    Task TryEnterStartFullscreenAsync();
    Task ExitFullscreenAsync();
}
