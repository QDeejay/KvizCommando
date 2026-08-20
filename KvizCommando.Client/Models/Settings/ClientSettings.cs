namespace KvizCommando.Client.Models.Settings;

public sealed class ClientSettings
{
    public bool SoundEnabled { get; set; } = true;
    public int MusicVolume { get; set; } = 70;
    public int SfxVolume { get; set; } = 80;
    public VisualTheme Theme { get; set; } = VisualTheme.Default;
    public bool StartFullscreen { get; set; }
}
