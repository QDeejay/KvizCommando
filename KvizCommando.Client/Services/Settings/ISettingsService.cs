using KvizCommando.Client.Models.Settings;

namespace KvizCommando.Client.Services.Settings;

public interface ISettingsService
{
    /// <summary>Az aktuálisan betöltött és normalizált kliensbeállítások.</summary>
    ClientSettings Current { get; }

    /// <summary>Betölti és alkalmazza a helyben tárolt kliensbeállításokat.</summary>
    Task LoadAsync();

    /// <summary>Elmenti, normalizálja és alkalmazza a megadott kliensbeállításokat.</summary>
    /// <param name="settings">A mentendő beállítások.</param>
    Task SaveAsync(ClientSettings settings);

    /// <summary>Alkalmazza az aktuális hang- és megjelenési beállításokat.</summary>
    Task ApplyAsync();

    /// <summary>Be- vagy kikapcsolja a hangot, majd elmenti a beállítást.</summary>
    /// <param name="enabled">A hang bekapcsolt állapota.</param>
    Task SetSoundEnabledAsync(bool enabled);

    /// <summary>A beállítás szerint megkísérli bekapcsolni a teljes képernyős módot.</summary>
    Task TryEnterStartFullscreenAsync();

    /// <summary>Kilép a teljes képernyős módból.</summary>
    Task ExitFullscreenAsync();
}
