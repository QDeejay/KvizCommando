using KvizCommando.Shared.Models.Enums.VsGame;

namespace KvizCommando.Shared.Contracts.VsGame.Match;

public sealed class VsMatchSnapshot
{
    public Guid MatchId { get; set; }
    public long PhaseVersion { get; set; }
    public int ClassificationId { get; set; }
    public int Stake { get; set; }
    public VsMatchPhase Phase { get; set; }
    public DateTime? DeadlineUtc { get; set; }
    public int PhaseDurationSeconds { get; set; }
    public string InfoKey { get; set; } = string.Empty;
    public VsMatchPlayerDto[] Players { get; set; } = [];
    public VsPreparationDto Preparation { get; set; } = new();
}

public sealed class VsMatchPlayerDto
{
    public int Position { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string TeamName { get; set; } = string.Empty;
    public int TeamLevel { get; set; }
    public string TeamPictureCode { get; set; } = string.Empty;
    public bool IsMe { get; set; }
    public bool IsConnected { get; set; }
    public bool IsFinished { get; set; }
}

public sealed class VsPreparationDto
{
    public int TeamSize { get; set; }
    public bool IsFinished { get; set; }
    public bool CanReset { get; set; }
    public bool CanFinish { get; set; }
    public VsPreparationRoundDto[] Rounds { get; set; } = [];
    public VsCharacterCardDto[] CharacterInventory { get; set; } = [];
    public VsLoadoutCardDto[] LoadoutInventory { get; set; } = [];
    public VsHelpCardDto[] HelpInventory { get; set; } = [];
    public VsCategoryModifierDto[] CategoryModifiers { get; set; } = [];
}

public sealed class VsPreparationRoundDto
{
    public int RoundNumber { get; set; }
    public bool IsCaptainRound { get; set; }
    public VsCharacterCardDto? Character { get; set; }
    public VsLoadoutCardDto? Loadout { get; set; }
    public VsHelpType HelpType { get; set; }
}

public sealed class VsCharacterCardDto
{
    public int SlotNumber { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PictureCode { get; set; } = string.Empty;
    public int Level { get; set; }
    public int OrientationId { get; set; }
}

public sealed class VsLoadoutCardDto
{
    public Guid LoadoutToken { get; set; }
    public int LoadoutPosition { get; set; }
    public int CategoryId { get; set; }
    public bool IsOwnQuestion { get; set; }
    public bool IsAllCategories { get; set; }
    public bool IsSelectable { get; set; }
}

public sealed class VsHelpCardDto
{
    public VsHelpType HelpType { get; set; }
    public int Count { get; set; }
}

public sealed class VsCategoryModifierDto
{
    public int RoundNumber { get; set; }
    public int CategoryId { get; set; }
    public double Seconds { get; set; }
}

/**
 * Egy játékosra szabott VS snapshot. Csak a megjelenítéshez és az
 * aktuálisan engedélyezett preparációs műveletekhez szükséges adatokat
 * küldi ki; kérdésszöveget és helyes választ nem tartalmaz.
 */
