namespace KvizCommando.Shared.Models.Enums.VsGame;

public enum VsConnectionQuality
{
    Unknown = 0,
    Good = 1,
    Medium = 2,
    Bad = 3
}

/**
 * A VS kapcsolat egyszeri, szerver által megállapított minősítése.
 * A kliens ezt kizárólag megjeleníti; a queue-belépésről a szerver
 * ugyanezen eredmény alapján dönt.
 */
