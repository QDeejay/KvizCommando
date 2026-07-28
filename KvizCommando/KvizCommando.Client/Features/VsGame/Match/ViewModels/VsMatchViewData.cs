using KvizCommando.Shared.Models.Enums.VsGame;

namespace KvizCommando.Client.Features.VsGame.Match.ViewModels;

public sealed class VsMatchViewData
{
    public Guid MatchId { get; set; }
    public VsMatchPhase Phase { get; set; }
    public DateTime? DeadlineUtc { get; set; }
    public int PhaseDurationSeconds { get; set; }
    public string InfoText { get; set; } = string.Empty;
    public string ClassificationText { get; set; } = string.Empty;
    public int Stake { get; set; }
    public VsRosterPlayerVm[] Players { get; set; } = [];
    public VsPreparationViewData Preparation { get; set; } = new();
}

public sealed class VsQueueViewData
{
    public string ClassificationText { get; set; } = string.Empty;
    public string StatusText { get; set; } = string.Empty;
    public int WaitingPlayers { get; set; }
    public int RequiredPlayers { get; set; }
    public int RequiredPartySize { get; set; }
    public int Stake { get; set; }
    public VsRosterPlayerVm[] Players { get; set; } = [];
}

public sealed class VsRosterPlayerVm
{
    public int Position { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string TeamName { get; set; } = string.Empty;
    public string TeamLevel { get; set; } = string.Empty;
    public string TeamPictureSrc { get; set; } = string.Empty;
    public bool IsMe { get; set; }
    public bool IsConnected { get; set; }
    public bool IsFinished { get; set; }
}

public sealed class VsPreparationViewData
{
    public bool IsFinished { get; set; }
    public bool CanReset { get; set; }
    public bool CanFinish { get; set; }
    public VsPreparationRoundVm[] Rounds { get; set; } = [];
    public VsCharacterCardVm[] Characters { get; set; } = [];
    public VsLoadoutCardVm[] Loadout { get; set; } = [];
    public VsHelpCardVm[] Helps { get; set; } = [];
    public VsCategoryModifierVm[] CategoryModifiers { get; set; } = [];
}

public sealed class VsPreparationRoundVm
{
    public int RoundNumber { get; set; }
    public string RoundText { get; set; } = string.Empty;
    public bool IsCaptainRound { get; set; }
    public VsCharacterCardVm? Character { get; set; }
    public VsLoadoutCardVm? Loadout { get; set; }
    public VsHelpCardVm? Help { get; set; }
}

public sealed class VsCharacterCardVm
{
    public int SlotNumber { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PictureCode { get; set; } = string.Empty;
    public string LevelText { get; set; } = string.Empty;
    public string OrientationName { get; set; } = string.Empty;
    public string OrientationImageSrc { get; set; } = string.Empty;
}

public sealed class VsLoadoutCardVm
{
    public Guid LoadoutToken { get; set; }
    public int LoadoutPosition { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string ImageSrc { get; set; } = string.Empty;
    public bool IsOwnQuestion { get; set; }
    public bool IsSelectable { get; set; }
}

public sealed class VsHelpCardVm
{
    public VsHelpType HelpType { get; set; }
    public string Name { get; set; } = string.Empty;
    public string IconCss { get; set; } = string.Empty;
    public int Count { get; set; }
}

public sealed class VsCategoryModifierVm
{
    public int RoundNumber { get; set; }
    public int CategoryId { get; set; }
    public double Seconds { get; set; }
}

/**
 * MÓDOSÍTÁS: a queue view model a lobby rosterét és kötelező
 * csapatméretét is tartalmazza.
 *
 * A VS lobby, roster és preparáció komponenseinek kizárólag
 * megjelenítési célú view modeljeit tartalmazza.
 */
