namespace KvizCommando.Server.Services.PlayerCache
{
    /// <summary>
    /// A gyorsítótárból indított mentés eredményét jelzi.
    /// </summary>
    public enum SaveResult
    {
        /// <summary>Nincs mentendő változás.</summary>
        None = 0,

        /// <summary>A módosított adatok mentése megtörtént.</summary>
        Dirty = 1,

        /// <summary>A kijelentkezés miatt a bejegyzés kikerült a gyorsítótárból.</summary>
        Logout = 2,

        /// <summary>A lejárt bejegyzés kikerült a gyorsítótárból.</summary>
        Obscolated = 3
    }
}
