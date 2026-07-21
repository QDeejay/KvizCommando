namespace KvizCommando.Shared.Contracts.SoloGame
{
    public enum SoloGameMode
    {
        Category = 1,
        Orientation = 2
    }

    public sealed class StartSoloGameRequest
    {
        public string SessionId { get; set; } = string.Empty;
        public SoloGameMode Mode { get; set; }
        public int SelectionId { get; set; }
    }

    public sealed class SoloQuestionDto
    {
        public Guid QuestionToken { get; set; }
        public string Question { get; set; } = string.Empty;
        public string[] Answers { get; set; } = [];
    }

    public sealed class StartSoloGameResponse
    {
        public Guid GameId { get; set; }
        public int QuestionCount { get; set; }
        public int AnswerTimeSeconds { get; set; }
        public int FeedbackTimeSeconds { get; set; }

        public int MaxPointsPerQuestion { get; set; }
        public SoloQuestionDto[] Questions { get; set; } = [];
    }

    public sealed class SoloAnswerDto
    {
        public Guid QuestionToken { get; set; }
        public int SelectedOptionIndex { get; set; }
        public int AnswerTimeMs { get; set; }
    }

    public sealed class FinishSoloGameRequest
    {
        public string SessionId { get; set; } = string.Empty;
        public int ClientElapsedMs { get; set; }
        public SoloAnswerDto[] Answers { get; set; } = [];
    }

    public sealed class SoloRewardDto
    {
        public int Xp { get; set; }
        public int Credit { get; set; }
        public int Voucher { get; set; }
    }

    public sealed class FinishSoloGameResponse
    {
        public bool[] AnswerResults { get; set; } = [];
        public int CorrectAnswers { get; set; }
        public int WrongAnswers { get; set; }
        public int TotalAnswerTimeMs { get; set; }
        public int[] TotalPoints { get; set; } = [];
        public bool IsNewHighScore { get; set; }
        public SoloRewardDto Rewards { get; set; } = new();
    }

    public sealed class AbandonSoloGameRequest
    {
        public string SessionId { get; set; } = string.Empty;
    }
}
