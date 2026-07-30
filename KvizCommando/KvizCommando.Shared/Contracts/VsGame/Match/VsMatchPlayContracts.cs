using KvizCommando.Shared.Models.Enums.VsGame;

namespace KvizCommando.Shared.Contracts.VsGame.Match;

public sealed class VsGuessAnswerRequest
{
    public int QuestionNumber { get; set; }
    public double Value { get; set; }
}

public sealed class VsChoiceAnswerRequest
{
    public int QuestionNumber { get; set; }
    public int AnswerIndex { get; set; }
}

public sealed class VsCaptainQuestionRequest
{
    public int LoadoutPosition { get; set; }
}

public sealed class VsGameDto
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
    public VsQuestionPlayerDto[] QuestionPlayers { get; set; } = [];
    public VsRoundProgressDto[] Progress { get; set; } = [];
    public VsRoundResultDto[] RoundResult { get; set; } = [];
    public VsCaptainQuestionDto[] CaptainQuestions { get; set; } = [];
    public int[] CaptainOrder { get; set; } = [];
    public int CaptainOrderIndex { get; set; }
}

public sealed class VsQuestionPlayerDto
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

public sealed class VsRoundProgressDto
{
    public int StepNumber { get; set; }
    public int PlayerPosition { get; set; }
    public bool IsGuess { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsCurrent { get; set; }
    public int Points { get; set; }
}

public sealed class VsRoundResultDto
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

public sealed class VsCaptainQuestionDto
{
    public int LoadoutPosition { get; set; }
    public int CategoryId { get; set; }
    public string Question { get; set; } = string.Empty;
}

/**
 * ÚJ FÁJL: a VS normál- és kapitánykör explicit SignalR-parancsait,
 * valamint a játékosra szabott snapshot megjelenítési DTO-it
 * tartalmazza. A válaszparancsot kizárólag a növekvő QuestionNumber
 * köti az aktuális kérdéshez; technikai kérésazonosítót nem használ.
 */
