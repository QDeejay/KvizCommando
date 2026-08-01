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
    TeamStats = 1 << 6,
    Logout = 1 << 7
}

/**
 * MÓDOSÍTÁS: a TeamStats az 1 << 6 dirty bitet kapta, a Logout
 * pedig az 1 << 7 helyre került. A felhasználás mindenhol az enum
 * néven keresztül történik.
 */
