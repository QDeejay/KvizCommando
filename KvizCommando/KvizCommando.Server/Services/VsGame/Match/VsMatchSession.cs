using KvizCommando.Shared.Models.Dtos;
using KvizCommando.Shared.Models.Enums.VsGame;

namespace KvizCommando.Server.Services.VsGame.Match;

public sealed class VsMatchSession : IDisposable
{
    public Guid MatchId { get; init; } = Guid.NewGuid();
    public VsMatchProfile Profile { get; init; } = VsMatchProfiles.Ranked;
    public VsBattleClassificationDto Classification { get; init; } = new();
    public object SyncRoot { get; } = new();
    public List<VsMatchPlayerState> Players { get; init; } = [];
    public List<VsMatchEventLogEntry> EventLog { get; } = [];
    public VsMatchGuessQuestionState[] GuessQuestions { get; set; } = [];
    public VsMatchGameState Game { get; } = new();
    public VsMatchRewardState? Reward { get; set; }

    public VsMatchPhase Phase { get; set; } = VsMatchPhase.MatchLocked;
    public DateTime PhaseStartedUtc { get; set; }
    public DateTime? DeadlineUtc { get; set; }
    public CancellationTokenSource PhaseTimerCts { get; set; } = new();
    public bool IsInitializing { get; set; } = true;
    public bool IsClosed { get; set; }

    public VsMatchPlayerState? FindByConnection(string connectionId) =>
        Players.FirstOrDefault(player =>
            player.ConnectionId == connectionId);

    public void Dispose()
    {
        PhaseTimerCts.Cancel();
        PhaseTimerCts.Dispose();
    }
}

public sealed class VsMatchPlayerState
{
    public int PlayerId { get; init; }
    public int Position { get; init; }
    public string SessionId { get; init; } = string.Empty;
    public string ConnectionId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string TeamName { get; set; } = string.Empty;
    public int TeamLevel { get; set; }
    public string TeamPictureCode { get; set; } = string.Empty;
    public int ResponseTimeMilliseconds { get; init; }
    public VsConnectionQuality ConnectionQuality { get; init; }
    public VsMatchCharacterState[] Characters { get; set; } = [];
    public VsMatchLoadoutItemState[] Loadout { get; set; } = [];
    public int[] HelpLevels { get; set; } = new int[4];
    public int[] HelpCounts { get; set; } = new int[4];
    public VsMatchRoundState[] Rounds { get; init; } = [];
    public bool IsConnected { get; set; } = true;
    public bool IsBot { get; set; }
    public string BotName { get; set; } = string.Empty;
    public bool IsFinished { get; set; }
    public bool StakeLocked { get; set; }
    public int TotalPoints { get; set; }
    public double TotalTimeSeconds { get; set; }
    public int RoundPoints { get; set; }
    public double RoundTimeSeconds { get; set; }
    public VsMatchPlayerAnswerState? CurrentAnswer { get; set; }
    public VsHelpType ActiveQuestionHelp { get; set; }
    public double? GuessRangeMinimum { get; set; }
    public double? GuessRangeMaximum { get; set; }
    public int[] HiddenAnswerIndices { get; set; } = [];
    public int? SuggestedAnswerIndex { get; set; }
    public List<int> RoundProgress { get; } = [];
    public HashSet<int> CaptainUsedLoadoutPositions { get; } = [];
    public VsMatchCharacterRewardTotal[] CharacterRewardTotals { get; set; } = [];
    public VsMatchStatisticsState Statistics { get; } = new();
}

public sealed class VsMatchCharacterRewardTotal
{
    public int SlotNumber { get; init; }
    public int CharacterXp { get; set; }
    public int EnergyLoss { get; set; }
    public int PlayDuels { get; set; }
    public int WinDuels { get; set; }
}

public sealed class VsMatchCharacterState
{
    public int SlotNumber { get; init; }
    public string Name { get; init; } = string.Empty;
    public string PictureCode { get; init; } = string.Empty;
    public int Level { get; init; }
    public int Xp { get; init; }
    public int EnergyPoints { get; init; }
    public int OrientationId { get; init; }
    public Dictionary<int, double> CategoryModifiers { get; init; } = [];
}

public sealed class VsMatchLoadoutItemState
{
    public int LoadoutPosition { get; init; }
    public int CategoryId { get; init; }
    public int QuestionCategoryId { get; init; }
    public int QuestionId { get; init; }
    public bool IsOwnQuestion { get; init; }
    public bool IsAllCategories { get; init; }
    public string Question { get; init; } = string.Empty;
    public string[] Answers { get; init; } = [];
    public int CorrectOptionIndex { get; init; }
}

public sealed class VsMatchRoundState
{
    public int RoundNumber { get; init; }
    public bool IsCaptainRound { get; init; }
    public int? CharacterSlotNumber { get; set; }
    public int? LoadoutPosition { get; set; }
    public VsHelpType HelpType { get; set; }
    public bool HelpUsed { get; set; }
}

public sealed class VsMatchPlayerSeed
{
    public int PlayerId { get; init; }
    public int Position { get; init; }
    public string SessionId { get; init; } = string.Empty;
    public string ConnectionId { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string TeamName { get; init; } = string.Empty;
    public int TeamLevel { get; init; }
    public int[] LoadoutCategories { get; init; } = [];
    public int[] HelpLevels { get; init; } = new int[4];
    public int[] HelpCounts { get; init; } = new int[4];
    public VsMatchCharacterState[] Characters { get; init; } = [];
    public VsOwnQuestionSeed[] OwnQuestions { get; init; } = [];
}

public sealed class VsOwnQuestionSeed
{
    public int QuestionId { get; init; }
    public string Question { get; init; } = string.Empty;
    public int CategoryId { get; init; }
    public string AnswersJson { get; init; } = string.Empty;
}

public sealed class VsMatchGuessQuestionState
{
    public int QuestionId { get; init; }
    public string Question { get; init; } = string.Empty;
    public double CorrectAnswer { get; init; }
}

public sealed class VsMatchGameState
{
    public int CurrentRoundNumber { get; set; }
    public int QuestionNumber { get; set; }
    public VsQuestionKind QuestionKind { get; set; }
    public VsMatchQuestionState? CurrentQuestion { get; set; }
    public int[] QuestionerOrder { get; set; } = [];
    public int CurrentQuestionerIndex { get; set; }
    public int[] CaptainOrder { get; set; } = [];
    public int CaptainOrderIndex { get; set; }
    public VsMatchQuestionResultState? QuestionResult { get; set; }
    public VsMatchRoundResultState[] RoundResult { get; set; } = [];
}

public sealed class VsMatchQuestionState
{
    public VsQuestionKind Kind { get; init; }
    public string Question { get; init; } = string.Empty;
    public string[] Answers { get; init; } = [];
    public int CorrectOptionIndex { get; init; }
    public double CorrectGuess { get; init; }
    public int QuestionerPosition { get; init; }
    public int CategoryId { get; init; }
    public int QuestionId { get; init; }
    public bool IsOwnQuestion { get; init; }
}

public sealed class VsMatchPlayerAnswerState
{
    public int QuestionNumber { get; init; }
    public int? AnswerIndex { get; init; }
    public double? Guess { get; init; }
    public double AnswerTimeSeconds { get; init; }
}

public sealed class VsMatchQuestionResultState
{
    public VsQuestionKind Kind { get; init; }
    public int CorrectOptionIndex { get; init; }
    public double CorrectGuess { get; init; }
    public VsMatchQuestionPlayerResultState[] Players { get; init; } = [];
}

public sealed class VsMatchQuestionPlayerResultState
{
    public int Position { get; init; }
    public int? AnswerIndex { get; init; }
    public double? Guess { get; init; }
    public bool IsCorrect { get; init; }
    public double AnswerTimeSeconds { get; init; }
    public double? ModifiedTimeSeconds { get; init; }
    public int Points { get; init; }
    public bool HasSpeedBonus { get; init; }
}

public sealed class VsMatchRoundResultState
{
    public int Position { get; init; }
    public int TotalBefore { get; init; }
    public int RoundPoints { get; init; }
    public int TotalAfter { get; init; }
    public double RoundTimeSeconds { get; init; }
    public bool HasWinnerBonus { get; init; }
    public bool HasFastestBonus { get; init; }
    public int CharacterSlotNumber { get; init; }
    public int CharacterXp { get; init; }
    public int EnergyLoss { get; init; }
}

/**
 * MÓDOSÍTÁS: a fázis időzítőjét kizárólag a saját cancellation tokenje
 * azonosítja, ezért a PhaseVersion megszűnt. A kiosztott loadout elem
 * az eleve egyedi LoadoutPosition értékkel szerepel a sessionben.
 *
 * MÓDOSÍTÁS: a session felvette a normál- és kapitánykör minimális,
 * szerveroldali állapotát, a növekvő QuestionNumbert, a játékosok
 * pont-/időadatait és a lezárt kérdés, illetve kör eredményét.
 * A loadoutelem külön tárolja a választott megjelenítési kategóriát
 * és a ténylegesen betöltött kérdés kategóriáját; ez az „összes”
 * választásnál szükséges a helyes időmódosítóhoz.
 * MÓDOSÍTÁS: a meccs eleji snapshot a segítségek szintjét is
 * megőrzi. A round az egyszer használható help állapotát, a játékos
 * pedig csak az aktuális kérdés személyre szabott help-hatását tárolja.
 * MÓDOSÍTÁS: a session tárolja a kapcsolatot elvesztő játékos
 * szerveroldali botállapotát, a karakterenként akkumulált körjutalmat
 * és a meccs végén egyszer elkészülő jutalomeredményt. A választási
 * kérdés azonosítója/saját jelzője és a játékos meccsstatisztikája a
 * későbbi egyetlen cache-mentéshez szintén itt marad.
 * MÓDOSÍTÁS: a karakterenkénti reward-akkumulátor a normál
 * nagykörök PlayDuels és WinDuels növekményét is megőrzi.
 * MÓDOSÍTÁS: a karakter meccskezdéskori XP-je is a snapshot része,
 * így a végső jutalom a következő szint határán szerveroldalon
 * levágható.
 * MÓDOSÍTÁS: a queue-ban egyszer megmért válaszidő és minősítés a
 * játékos meccsállapotában változatlan marad a meccs végéig.
 *
 * Egy lezárt meccs teljes, szerveroldali authoritative állapotát
 * tartalmazza. A SignalR Hub nem őriz játékállapotot.
 */
