using KvizCommando.Shared.Models.Enums.VsGame;

namespace KvizCommando.Shared.Contracts.VsGame.Match;

public sealed class VsMatchSnapshot
{
    public Guid MatchId { get; set; }
    public int ClassificationId { get; set; }
    public int Stake { get; set; }
    public VsMatchPhase Phase { get; set; }
    public DateTime? DeadlineUtc { get; set; }
    public int PhaseDurationSeconds { get; set; }
    public string InfoKey { get; set; } = string.Empty;
    public VsMatchPlayerDto[] Players { get; set; } = [];
    public VsPreparationDto Preparation { get; set; } = new();
    public VsGameDto Game { get; set; } = new();
    public VsMatchRewardDto Reward { get; set; } = new();
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
    public bool IsBot { get; set; }
    public bool IsFinished { get; set; }
    public int TotalPoints { get; set; }
    public double TotalTimeSeconds { get; set; }
    public VsCharacterCardDto? ActiveCharacter { get; set; }
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
 * MÓDOSÍTÁS: a technikai PhaseVersion és a LoadoutPosition mellett
 * felesleges LoadoutToken kikerült. A MatchId publikus hivatkozási
 * számként megmarad a későbbi reklamációhoz és admin-visszakereséshez.
 *
 * MÓDOSÍTÁS: a snapshot felvette a játék állását, a rendezett
 * összpont-/összidőadatokat és a normál kör aktív karakterét. A
 * helyes válasz kizárólag lezárt kérdésnél kerül a címzetthez.
 * MÓDOSÍTÁS: a roster külön botjelzőt, a GameCompleted snapshot pedig
 * nyilvános végső sorrendet és kizárólag a címzett saját rewardját
 * tartalmazza.
 *
 * Egy játékosra szabott VS snapshot. Csak a megjelenítéshez és az
 * aktuálisan engedélyezett műveletekhez szükséges adatokat küldi ki.
 */
