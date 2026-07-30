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

    public VsMatchPhase Phase { get; set; } = VsMatchPhase.MatchLocked;
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
    public VsMatchCharacterState[] Characters { get; set; } = [];
    public VsMatchLoadoutItemState[] Loadout { get; set; } = [];
    public int[] HelpCounts { get; set; } = new int[4];
    public VsMatchRoundState[] Rounds { get; init; } = [];
    public bool IsConnected { get; set; } = true;
    public bool IsFinished { get; set; }
    public bool StakeLocked { get; set; }
}

public sealed class VsMatchCharacterState
{
    public int SlotNumber { get; init; }
    public string Name { get; init; } = string.Empty;
    public string PictureCode { get; init; } = string.Empty;
    public int Level { get; init; }
    public int OrientationId { get; init; }
    public Dictionary<int, double> CategoryModifiers { get; init; } = [];
}

public sealed class VsMatchLoadoutItemState
{
    public int LoadoutPosition { get; init; }
    public int CategoryId { get; init; }
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

/**
 * MÓDOSÍTÁS: a fázis időzítőjét kizárólag a saját cancellation tokenje
 * azonosítja, ezért a PhaseVersion megszűnt. A kiosztott loadout elem
 * az eleve egyedi LoadoutPosition értékkel szerepel a sessionben.
 *
 * Egy lezárt meccs teljes, szerveroldali és authoritative állapotát
 * tartalmazza. A SignalR Hub nem őriz játékállapotot.
 */
