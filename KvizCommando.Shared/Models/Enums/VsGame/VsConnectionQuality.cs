namespace KvizCommando.Shared.Models.Enums.VsGame;

/// <summary>
/// A kapcsolat szerver által meghatározott minősítése.
/// A kliens az eredményt kizárólag megjeleníti.
/// </summary>
public enum VsConnectionQuality
{
    Unknown = 0,
    Good = 1,
    Medium = 2,
    Bad = 3
}
