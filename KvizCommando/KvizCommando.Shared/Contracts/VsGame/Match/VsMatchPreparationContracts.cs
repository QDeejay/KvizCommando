using KvizCommando.Shared.Models.Enums.VsGame;

namespace KvizCommando.Shared.Contracts.VsGame.Match;

public sealed class VsLoadoutAssignmentRequest
{
    public int LoadoutPosition { get; set; }
    public int RoundNumber { get; set; }
}

public sealed class VsHelpAssignmentRequest
{
    public VsHelpType HelpType { get; set; }
    public int RoundNumber { get; set; }
}

/**
 * MÓDOSÍTÁS: a kategóriakiosztás a loadout eleve egyedi, 0–9 közötti
 * pozícióját küldi vissza; külön GUID azonosítót nem használ.
 *
 * A preparációs kategória- és segítségkiosztás SignalR-parancsainak
 * közös request típusait tartalmazza.
 */
