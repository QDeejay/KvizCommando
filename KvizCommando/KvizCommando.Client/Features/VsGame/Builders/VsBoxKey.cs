namespace KvizCommando.Client.Features.VsGame.Builders;

internal enum VsBoxKeyRoot
{
    RtBtnCreateBattlefield,
    RtBtnJoinBattlefield,
    RtBtnRankedBattlefields
}

internal enum VsBoxKeyContent
{
    RankedBattleTeamManager,
    RankedMatchManager
}

internal enum VsBoxKeyRanked
{
    Classification = 5
}

/**
 * MÓDOSÍTÁS: a VS tartalmi kulcsok közé felveszi a DynamicComponent
 * alapú ranked meccsmanagert.
 *
 * A fájl a VS menü root-, tartalmi és rangsorolási dobozkulcsait
 * tartalmazza.
 */
