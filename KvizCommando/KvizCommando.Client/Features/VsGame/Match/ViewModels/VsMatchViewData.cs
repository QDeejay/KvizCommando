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
    public VsGameViewData Game { get; set; } = new();
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
    public int TotalPoints { get; set; }
    public double TotalTimeSeconds { get; set; }
    public VsCharacterCardVm? ActiveCharacter { get; set; }
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

public sealed class VsGameViewData
{
    public int CurrentRoundNumber { get; set; }
    public int NormalRoundCount { get; set; }
    public int QuestionNumber { get; set; }
    public VsQuestionKind QuestionKind { get; set; }
    public int QuestionerPosition { get; set; }
    public string Question { get; set; } = string.Empty;
    public string[] Answers { get; set; } = [];
    public int? CorrectAnswerIndex { get; set; }
    public double? CorrectGuess { get; set; }
    public int? MyAnswerIndex { get; set; }
    public double? MyGuess { get; set; }
    public int MyRoundPoints { get; set; }
    public double MyRoundTimeSeconds { get; set; }
    public bool CanAnswer { get; set; }
    public bool CanChooseCaptainQuestion { get; set; }
    public VsQuestionPlayerVm[] QuestionPlayers { get; set; } = [];
    public VsRoundProgressVm[] Progress { get; set; } = [];
    public VsRoundResultVm[] RoundResult { get; set; } = [];
    public VsCaptainQuestionVm[] CaptainQuestions { get; set; } = [];
    public int[] CaptainOrder { get; set; } = [];
    public int CaptainOrderIndex { get; set; }
}

public sealed class VsQuestionPlayerVm
{
    public int Position { get; set; }
    public bool HasAnswered { get; set; }
    public int? AnswerIndex { get; set; }
    public double? Guess { get; set; }
    public bool IsCorrect { get; set; }
    public double AnswerTimeSeconds { get; set; }
    public double? ModifiedTimeSeconds { get; set; }
    public int Points { get; set; }
    public bool HasSpeedBonus { get; set; }
}

public sealed class VsRoundProgressVm
{
    public int StepNumber { get; set; }
    public int PlayerPosition { get; set; }
    public bool IsGuess { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsCurrent { get; set; }
    public int Points { get; set; }
}

public sealed class VsRoundResultVm
{
    public int Position { get; set; }
    public int TotalBefore { get; set; }
    public int RoundPoints { get; set; }
    public int TotalAfter { get; set; }
    public double RoundTimeSeconds { get; set; }
    public bool HasWinnerBonus { get; set; }
    public bool HasFastestBonus { get; set; }
    public int CharacterSlotNumber { get; set; }
    public int CharacterXp { get; set; }
    public int EnergyLoss { get; set; }
}

public sealed class VsCaptainQuestionVm
{
    public int LoadoutPosition { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string ImageSrc { get; set; } = string.Empty;
    public string Question { get; set; } = string.Empty;
}

/**
 * MÓDOSÍTÁS: a MatchId megmarad a későbbi reklamációs hivatkozáshoz,
 * a LoadoutPosition mellett felesleges LoadoutToken viszont kikerült.
 * A queue view model továbbra is tartalmazza a lobby rosterét és a
 * kötelező csapatméretét.
 *
 * MÓDOSÍTÁS: felvette az élő rangsor, kérdés, válaszállapot,
 * progressz, köreredmény és kapitányi kérdésválasztás kizárólag
 * megjelenítési célú modelljeit.
 *
 * A VS lobby, roster, preparáció és játéknézet view modeljeit
 * tartalmazza.
 */
