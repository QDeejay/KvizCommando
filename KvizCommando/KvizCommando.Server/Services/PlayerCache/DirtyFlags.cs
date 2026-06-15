namespace KvizCommando.Server.Services.PlayerCache;
[Flags]
public enum DirtyFlags : byte
{
    None = 0,
    Core = 1 << 0,
    Loadout = 1 << 1,
    Characters = 1 << 2,
    AskStats = 1 << 3,
    CategoryStats = 1 << 4,
    OrientStats = 1 << 5,
    Logout = 1 << 6
}