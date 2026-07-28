using KvizCommando.Client.Features.VsGame.Match.ViewModels;
using Microsoft.AspNetCore.Components;

namespace KvizCommando.Client.Features.VsGame.Match.Components;

public partial class VsMatchRoster
{
    [Parameter, EditorRequired]
    public VsRosterPlayerVm[] Players { get; set; } = [];

    [Parameter] public int ExpectedPlayers { get; set; }
    [Parameter] public string Title { get; set; } = string.Empty;

    private int MissingPlayers =>
        Math.Max(0, ExpectedPlayers - Players.Length);
}

/**
 * MÓDOSÍTÁS: a roster címe és várt játékosszáma paraméterezhető, ezért
 * a lobby a még hiányzó helyeket is ugyanebben a komponensben mutatja.
 * A címhez nincs beégetett nyelvi alapérték; azt a szülő lokalizált
 * feliratként adja át.
 *
 * A VS meccs publikus játékoslistáját megjelenítő komponens
 * paramétereit tartalmazza.
 */
