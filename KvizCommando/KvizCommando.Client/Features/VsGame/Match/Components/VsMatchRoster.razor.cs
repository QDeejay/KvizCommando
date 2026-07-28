using KvizCommando.Client.Features.VsGame.Match.ViewModels;
using Microsoft.AspNetCore.Components;

namespace KvizCommando.Client.Features.VsGame.Match.Components;

public partial class VsMatchRoster
{
    [Parameter, EditorRequired]
    public VsRosterPlayerVm[] Players { get; set; } = [];
}

/**
 * A VS meccs publikus játékoslistáját megjelenítő komponens
 * paramétereit tartalmazza.
 */
